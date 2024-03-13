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
  private readonly ILogger<DataCollectionService> _log;

  public DataCollectionService(
    MapsApiClient maps,
    TransitApiClient transit,
    ILogger<DataCollectionService> log)
  {
    _cts = new();
    _maps = maps;
    _transit = transit;
    _log = log;
  }
  public async Task StartAsync(CancellationToken token = default)
  {
    var cts =
      CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, token);

    await Task.CompletedTask;
  }

  public async Task StopAsync(CancellationToken token = default)
  {
    var cts =
      CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, token);

    await Task.CompletedTask;
  }
}