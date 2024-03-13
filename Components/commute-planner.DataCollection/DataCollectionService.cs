using commute_planner.MapsApi;
using commute_planner.TransitApi;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace commute_planner.DataCollection;

public class DataCollectionService(
  MapsApiClient maps,
  TransitApiClient transit,
  IConnection messaging) : IHostedService
{
  protected readonly CancellationTokenSource _cts =
    new CancellationTokenSource();
  public async Task StartAsync(CancellationToken cancellationToken)
  {
    var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token,
      cancellationToken);
    
    // Any tasks including starting the service
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token,
      cancellationToken);
    
    // Any async cancellation here
  }
}