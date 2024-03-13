using commute_planner.MapsApi;
using commute_planner.TransitApi;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace commute_planner.DataCollection;

public class DataCollectionService : IHostedService
{
  private readonly CancellationTokenSource _cts;
  private readonly IConnection _messaging;
  private readonly MapsApiClient _maps;
  private readonly TransitApiClient _transit;
  private readonly IModel _channel;

  public DataCollectionService(
    MapsApiClient maps,
    TransitApiClient transit,
    IConnection messaging)
  {
    _cts = new();
    _maps = maps;
    _transit = transit;
    _messaging = messaging;
    _channel = messaging.CreateModel();
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