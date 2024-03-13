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
  public Task StartAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
}