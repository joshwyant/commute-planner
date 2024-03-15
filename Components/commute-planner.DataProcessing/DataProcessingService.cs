using System.Threading.Tasks.Dataflow;
using commute_planner.CommuteDatabase;
using commute_planner.CommuteDatabase.Models;
using commute_planner.EventCollaboration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace commute_planner.DataProcessing;

public class DataProcessingService : IHostedService
{
  private CancellationTokenSource _cts;
  private IServiceScopeFactory _scopeFactory;
  private DataProcessingExchange _exchange;
  private readonly ILogger<DataProcessingService> _log;
  private Dictionary<int, JoinBlock<DrivingTrip, TransitTrip>> _joinBlocks;
  private ActionBlock<Tuple<DrivingTrip, TransitTrip>> _actionBlock;

  public DataProcessingService(IServiceScopeFactory scopeFactory,
    DataProcessingExchange exchange,
    ILogger<DataProcessingService> log)
  {
    _cts = new CancellationTokenSource();
    _scopeFactory = scopeFactory;
    _exchange = exchange;
    _joinBlocks = new Dictionary<int, JoinBlock<DrivingTrip, TransitTrip>>();
    _log = log;
  }

  public async Task StartAsync(CancellationToken token = default)
  {
    var linkedCts =
      CancellationTokenSource.CreateLinkedTokenSource(token, _cts.Token);
    await StartDataflowServiceAsync(linkedCts.Token);

    await StartTriggerServiceAsync(linkedCts.Token);

    // Now start consuming
    // ...
  }

  private async Task StartTriggerServiceAsync(CancellationToken token = default)
  {
    var task = Task.Run(async () =>
    {
      while (!token.IsCancellationRequested)
      {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider
          .GetRequiredService<CommutePlannerDbContext>();
        // Trigger fresh data collection from the data collection service
        // every 5 minutes.
        foreach (var route in db.MatchingRoutes)
        {
          var drivingRoute = route.DrivingRoute;
          var transitRoute = route.TransitRoute;
          // Collect driving times for each route
          var drivingRequest = new CollectFreshDrivingRouteRequest(
            route.MatchingRouteId,
            drivingRoute.FromAddress,
            drivingRoute.ToAddress);
          await _exchange.PublishAsync(DataProcessingExchange.DataCollectionRoutingKey,
            drivingRequest, token);
          await Task.Yield();
          
          // Collect transit times for each route
          var transitRequest = new CollectFreshTransitRouteRequest(
            route.MatchingRouteId,
            transitRoute.OperatorId,
            transitRoute.LineId,
            transitRoute.FromStopId,
            transitRoute.ToStopId);
          await _exchange.PublishAsync(DataProcessingExchange.DataCollectionRoutingKey,
            transitRequest, token);
          await Task.Yield();
        }
        await Task.Delay(300000, token);  // 5 minutes
      }
    }, token);
  }

  private async Task StartDataflowServiceAsync(CancellationToken token = default)
  {
    // Consume data from the service!
    _actionBlock = new ActionBlock<Tuple<DrivingTrip, TransitTrip>>(
      async tuple =>
      {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider
          .GetRequiredService<CommutePlannerDbContext>();
        var (drivingTrip, transitTrip) = tuple;
        db.TripData.Add(new TripData
        {
          Created = drivingTrip.Created < transitTrip.Created
            ? drivingTrip.Created
            : transitTrip.Created, // Oldest of the 2
          DrivingTimeInSeconds = drivingTrip.TimeInSeconds,
          TransitTimeInSeconds = drivingTrip.TimeInSeconds,
          MatchingRouteId = drivingTrip.RouteId,
        });
        try
        {
          await db.SaveChangesAsync(token);
        }
        catch (TaskCanceledException e)
        {
          _log.LogInformation(
            "Task canceled while saving changes to the database");
        }
        
      }, new ExecutionDataflowBlockOptions()
      {
        CancellationToken = token
      });
    
    
    using (var scope = _scopeFactory.CreateScope())
    {
      var db = scope.ServiceProvider
        .GetRequiredService<CommutePlannerDbContext>();
      // Wire them up
      foreach (var route in db.MatchingRoutes)
      {
        var joinBlock = new JoinBlock<DrivingTrip, TransitTrip>(
          new GroupingDataflowBlockOptions()
          {
            CancellationToken = token
          });
        joinBlock.LinkTo(_actionBlock,
          new DataflowLinkOptions() { PropagateCompletion = true });
        _joinBlocks.Add(route.MatchingRouteId, joinBlock);
      }
    }

    // Subscribe
    _exchange.DrivingTripPosted += async (sender, trip) =>
      await _joinBlocks[trip.RouteId].Target1.SendAsync(trip, token);
    
    _exchange.TransitTripPosted += async (sender, trip) =>
      await _joinBlocks[trip.RouteId].Target2.SendAsync(trip, token);

    _exchange.StopMonitoringPosted += async (sender, response) =>
    {
      var scope = _scopeFactory.CreateScope();
      var db = scope.ServiceProvider
        .GetRequiredService<CommutePlannerDbContext>();
      
      var transitRoute = (await db.MatchingRoutes
          .Include(r => r.TransitRoute)
          .SingleAsync(r => r.MatchingRouteId == response.routeId, token))
        .TransitRoute;
      scope.Dispose();
      
      var fromStopId = transitRoute.FromStopId;
      var toStopId = transitRoute.ToStopId;
      var lineId = transitRoute.LineId;
      var data = response.data;

      // Get journeys for the relevant line ID on our route.
      var journeys = data
        .Where(d =>
          d.MonitoredVehicleJourney.LineRef == lineId)
        .Select(sv =>
          sv.MonitoredVehicleJourney)
        .ToArray();
      
      // Get every upcoming stop for those vehicles' journeys.
      var stops = journeys
        .SelectMany(j => 
          j.OnwardCalls.Select(c => new
          {
            Journey = j,
            StopId = c.StopPointRef,
            DepartureTime = DateTime.Parse(c.AimedDepartureTime)
          }))
      // ... which stops at the starting point
      // ... and are also part of a journey that stops at the destination
        .Where(s => 
          s.DepartureTime > DateTime.UtcNow
            && s.StopId == fromStopId 
            && s.Journey.OnwardCalls.Any(c => 
              c.StopPointRef == toStopId))
        .ToArray();
        
      var earliestStop = stops
        .MinBy(s => s.DepartureTime);

      if (earliestStop != null)
      {
        var tripStops = earliestStop
          ?.Journey?.OnwardCalls
          ?.Select(c => new
          {
            StopId = c.StopPointRef,
            DepartureTime = DateTime.Parse(c.AimedDepartureTime),
          }).ToArray();
        var sourceStop = tripStops.Single(
          s => s.StopId == fromStopId);

        var destStop = tripStops.Single(
          s => s.StopId == toStopId);

        var tripTime = (destStop.DepartureTime - sourceStop.DepartureTime)
          .Seconds;
        
        // Queue it into our joined block target!
        await _joinBlocks[response.routeId].Target2
          .SendAsync(new TransitTrip(response.routeId, tripTime, DateTime.Now),
            token);
      }
    };
  }

  public async Task StopAsync(CancellationToken token = default)
  {
    foreach (var joinBlock in _joinBlocks.Values)
    {
      joinBlock.Complete();
    }

    await _actionBlock.Completion;

    await _cts.CancelAsync();
  }
}
