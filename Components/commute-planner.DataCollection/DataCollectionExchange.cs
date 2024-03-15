using System.Text.Json;
using commute_planner.EventCollaboration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace commute_planner.DataCollection;

public class DataCollectionExchange : CommutePlannerExchange
{
  public DataCollectionExchange(IConnectionFactory factory,
    ILogger<CommutePlannerExchange> log) : base(factory, log,
    DataCollectionRoutingKey)
  {
  }

  protected override async Task OnMessageAsync(string messageType, string routingKey,
    string message, CancellationToken token)
  {
    switch (messageType)
    {
      // We received a request to collect fresh driving route data. Trigger
      // the corresponding event.
      case nameof(CollectFreshDrivingRouteRequest):
        var drivingRequest =
          JsonSerializer.Deserialize<CollectFreshDrivingRouteRequest>(message);
        if (drivingRequest != null)
        {
          DrivingDataCollectionRequested?.Invoke(this, drivingRequest);
        }
        break;
      
      // We received a request to collect fresh transit route data. Trigger
      // the corresponding event.
      case nameof(CollectFreshTransitRouteRequest):
        var transitRequest =
          JsonSerializer.Deserialize<CollectFreshTransitRouteRequest>(message);
        if (transitRequest != null)
        {
          TransitDataCollectionRequested?.Invoke(this, transitRequest);
        }
        break;
    }
  }

  public event EventHandler<CollectFreshTransitRouteRequest>
    TransitDataCollectionRequested;

  public event EventHandler<CollectFreshDrivingRouteRequest>
    DrivingDataCollectionRequested;
}