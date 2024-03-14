using System.Text.RegularExpressions;
using commute_planner.EventCollaboration;
using commute_planner.MapsApi;
using commute_planner.TransitApi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace commute_planner.DataCollection;

public class DataCollectionService : IHostedService
{
  private readonly CancellationTokenSource _cts;
  private readonly MapsApiClient _maps;
  private readonly TransitApiClient _transit;
  private readonly DataCollectionExchange _exchange;
  private readonly ILogger<DataCollectionService> _log;

  public DataCollectionService(
    MapsApiClient maps,
    TransitApiClient transit,
    DataCollectionExchange exchange,
    ILogger<DataCollectionService> log)
  {
    _cts = new();
    _maps = maps;
    _transit = transit;
    _exchange = exchange;
    _log = log;
  }
  public async Task StartAsync(CancellationToken token = default)
  {
    var cts =
      CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, token);

    _exchange.DrivingDataCollectionRequested += async (sender, request) =>
    {
      var result = await _maps.ComputeRoutes(new ComputeRoutesRequest(
        new Waypoint() { Address = request.FromAddress },
        new Waypoint() { Address = request.ToAddress }));

      if (result?.Routes is null || result.Routes.Length == 0) return;
      var match = Regex.Match(result?.Routes[0]?.Duration!, "(\\d+)s");
      if (!match.Success) return;
      if (match.Groups.Count < 1) return;
      var timeStr = match.Groups[0].Value;
      var time = int.Parse(timeStr);

      var minutes = time / 60;

      var res =
        new CollectedDrivingDataResponse(request.routeId, $"{minutes} minutes");

      // Send the data to the data processor service
      _exchange.Publish(CommutePlannerExchange.DataProcessingRoutingKey, res);
    };

    _exchange.TransitDataCollectionRequested += async (sender, request) =>
    {
      var result = await _transit.StopMonitoring(request.OperatorId, request.FromStopId);
      var result1 = result.ToArray();
      var res = new CollectedStopMonitoringResponse(request.routeId, result1);
      
      // Send the data to the processing service
      _exchange.Publish(CommutePlannerExchange.DataProcessingRoutingKey, res);
    };
  }

  public async Task StopAsync(CancellationToken token = default)
  {

    await _cts.CancelAsync();
  }
}