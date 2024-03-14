using System.Threading.Tasks.Dataflow;
using commute_planner.CommuteDatabase;
using commute_planner.CommuteDatabase.Models;
using commute_planner.EventCollaboration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace commute_planner.DataProcessing;

public class DataProcessingService : IHostedService
{
  private readonly CancellationTokenSource _cts;
  private readonly CommutePlannerDbContext _db;
  private DataProcessingExchange _exchange;
  private readonly ILogger<DataProcessingService> _log;
  private Dictionary<int, JoinBlock<DrivingTrip, TransitTrip>> _joinBlocks;

  private Dictionary<int, JoinBlock<
    CollectedStopsResponse,
    CollectedLinesResponse,
    CollectedStopMonitoringResponse>> _rawTransitBlocks1;
  private Dictionary<int, JoinBlock<
    CollectedStopPlacesResponse,
    CollectedVehicleMonitoringResponse,
    CollectedScheduledDeparturesAtStopResponse>> _rawTransitBlocks2;

  private Dictionary<int, JoinBlock<Tuple<CollectedStopsResponse,
    CollectedLinesResponse,
    CollectedStopMonitoringResponse>, Tuple<CollectedStopPlacesResponse,
    CollectedVehicleMonitoringResponse,
    CollectedScheduledDeparturesAtStopResponse>>> _rawTransitBlocks;
  
  private ActionBlock<Tuple<DrivingTrip, TransitTrip>> _actionBlock;

  public DataProcessingService(CommutePlannerDbContext db,
    DataProcessingExchange exchange,
    ILogger<DataProcessingService> log)
  {
    _cts = new CancellationTokenSource();
    _db = db;
    _exchange = exchange;
    _joinBlocks = new Dictionary<int, JoinBlock<DrivingTrip, TransitTrip>>();
    _log = log;
  }

  public async Task StartAsync(CancellationToken token = default)
  {
    var cts =
      CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, token);
    await SeedDatabaseAsync(cts.Token);

    await StartDataflowServiceAsync(token);

    await StartTriggerServiceAsync(token);

    // Now start consuming
    // ...
  }

  private async Task StartTriggerServiceAsync(CancellationToken token = default)
  {
    var cts =
      CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, token);

    var task = Task.Run(async () =>
    {
      while (!cts.IsCancellationRequested)
      {
        // Trigger fresh data collection from the data collection service
        // every 5 minutes.
        foreach (var route in _db.MatchingRoutes)
        {
          var drivingRoute = route.DrivingRoute;
          var transitRoute = route.TransitRoute;
          // Collect driving times for each route
          var drivingRequest = new CollectFreshDrivingRouteRequest(
            route.MatchingRouteId,
            drivingRoute.FromAddress,
            drivingRoute.ToAddress);
          _exchange.Publish(DataProcessingExchange.DataCollectionRoutingKey,
            drivingRequest);
          await Task.Yield();
          
          // Collect transit times for each route
          var transitRequest = new CollectFreshTransitRouteRequest(
            route.MatchingRouteId,
            transitRoute.OperatorId,
            transitRoute.LineId,
            transitRoute.FromStopId,
            transitRoute.ToStopId);
          _exchange.Publish(DataProcessingExchange.DataCollectionRoutingKey,
            transitRequest);
          await Task.Yield();
        }
        await Task.Delay(300000);  // 5 minutes
      }
    }, cts.Token);
  }

  private async Task StartDataflowServiceAsync(CancellationToken token = default)
  {
    var cts =
      CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, token);
    
    // Consume data from the service!
    _actionBlock = new ActionBlock<Tuple<DrivingTrip, TransitTrip>>(
      async tuple =>
      {
        var (drivingTrip, transitTrip) = tuple;
        _db.TripData.Add(new TripData
        {
          Created = drivingTrip.Created < transitTrip.Created
            ? drivingTrip.Created
            : transitTrip.Created, // Oldest of the 2
          DrivingTimeInSeconds = drivingTrip.TimeInSeconds,
          TransitTimeInSeconds = drivingTrip.TimeInSeconds,
          MatchingRouteId = drivingTrip.RouteId,
        });
        await _db.SaveChangesAsync();
      }, new ExecutionDataflowBlockOptions()
      {
        CancellationToken = cts.Token
      });
    
    // Wire them up
    foreach (var route in _db.MatchingRoutes)
    {
      var joinBlock = new JoinBlock<DrivingTrip, TransitTrip>(new GroupingDataflowBlockOptions()
      {
        CancellationToken = cts.Token
      });
      joinBlock.LinkTo(_actionBlock);
      _joinBlocks.Add(route.MatchingRouteId, joinBlock);
    }

    // Subscribe
    _exchange.DrivingTripPosted += async (sender, trip) =>
      await _joinBlocks[trip.RouteId].Target1.SendAsync(trip, cts.Token);
    
    _exchange.TransitTripPosted += async (sender, trip) =>
      await _joinBlocks[trip.RouteId].Target2.SendAsync(trip, cts.Token);
    
    // Fix these later
    _exchange.StopsPosted += async (sender, response) =>
      await _rawTransitBlocks1[response.routeId].Target1.SendAsync(response, cts.Token);

    _exchange.StopMonitoringPosted += async (sender, response) =>
    {
      var transitRoute = _db.MatchingRoutes
        .Single(r => r.MatchingRouteId == response.routeId)
        .TransitRoute;
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
            cts.Token);
      }
    };
      
    _exchange.StopPlacesPosted += async (sender, response) =>
      await _rawTransitBlocks2[response.routeId].Target1.SendAsync(response, cts.Token);
    
    _exchange.VehicleMonitoringPosted += async (sender, response) =>
      await _rawTransitBlocks2[response.routeId].Target2.SendAsync(response, cts.Token);
    
    _exchange.LinesPosted += async (sender, response) =>
      await _rawTransitBlocks1[response.routeId].Target2.SendAsync(response, cts.Token);
    
    _exchange.ScheduledDeparturesPosted += async (sender, response) =>
      await _rawTransitBlocks2[response.routeId].Target3.SendAsync(response, cts.Token);
  }

  public async Task StopAsync(CancellationToken token = default)
  {
    await _cts.CancelAsync();
    
    // Not using CancellationToken here
    foreach (var joinBlock in _joinBlocks.Values)
    {
      joinBlock.Complete();
    }

    await _actionBlock.Completion;
  }

  private async Task SeedDatabaseAsync(CancellationToken token)
  {
    if (await _db.Database.EnsureCreatedAsync(token))
    {
      foreach (var pair in _pairs)
      {
        var transitRoute = new TransitRoute()
        {
          Name = pair.RouteName,
          Description = pair.TransitRouteDescription,
          OperatorId = pair.OperatorId,
          LineId = pair.LineId,
          ToStopId = pair.ToStopId,
          FromStopId = pair.FromStopId,
        };
        var autoRoute = new DrivingRoute()
        {
          Name = pair.RouteName,
          Description = pair.DrivingRouteDescription,
          FromAddress = pair.FromAddress,
          ToAddress = pair.ToAddress
        };
        var match = new MatchingRoute()
        {
          Name = pair.RouteName,
          DrivingRoute = autoRoute,
          TransitRoute = transitRoute,
        };
        _db.TransitRoutes.Add(transitRoute);
        _db.DrivingRoutes.Add(autoRoute);
        _db.MatchingRoutes.Add(match);
      }

      await _db.SaveChangesAsync(token);
    }
  }
  
  // Seed data for the database.
  // Route suggestions and descriptions by ChatGPT.
  private readonly RoutePair[] _pairs = [
      new("Ocean Beach to Financial District",
          "Utilize the N Judah line, beginning at Ocean Beach and concluding at Embarcadero Station, showcasing a scenic to urban commute.",
          "SF",
          "N",
          "15223",
          "16992",
          "A scenic drive along the Great Highway, transitioning to urban streets towards downtown, encountering varying traffic conditions.",
          "Judah St & La Playa St, San Francisco, CA 94122",
          "Financial District, San Francisco, CA"),
      new("Mission District to Salesforce Tower",
          "Journey on the J Church from the vibrant Mission District directly to the heart of tech at Salesforce Tower, emphasizing connectivity within the city.",
          "SF",
          "J",
          "16213",
          "15731",
          "Travels through the heart of San Francisco, highlighting the contrast between the Mission's vibrant streets and downtown's bustling business district.",
          "18th St & Church St, San Francisco, CA 94114",
          "Salesforce Tower, 415 Mission St, San Francisco, CA 94105"),
      new("Sunset District to Stonestown Galleria",
          "Take the L Taraval for a shopping excursion from the residential Sunset District to Stonestown Galleria, linking neighborhoods to commercial hubs.",
          "SF",
          "LBUS",
          "13599",
          "16617",
          "A straightforward route, mostly along 19th Avenue, offering a quick connection between residential areas and shopping destinations.",
          "46th Ave & Taraval St, San Francisco, CA 94116",
          "Stonestown Galleria, 3251 20th Ave, San Francisco, CA 94132"),
      new("Sunnydale to UCSF/Chase Center",
          "The T Third Street line connects Sunnydale to the UCSF/Chase Center, bridging community areas to major health and entertainment venues.",
          "SF",
          "T",
          "17396",
          "17360",
          "From the outskirts to the city's burgeoning biomedical and entertainment district, showcasing urban redevelopment and traffic diversity.",
          "Sunnydale Ave & Bayshore Blvd, San Francisco, CA 94134",
          "Chase Center, 1 Warriors Way, San Francisco, CA 94158"),
  ];
  
  record RoutePair(
    string RouteName,
    string TransitRouteDescription,
    string OperatorId,
    string LineId,
    string FromStopId,
    string ToStopId,
    string DrivingRouteDescription,
    string FromAddress,
    string ToAddress
  );
}
