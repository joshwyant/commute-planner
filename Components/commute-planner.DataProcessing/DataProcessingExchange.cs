using System.Text.Json;
using commute_planner.EventCollaboration;
using commute_planner.TransitApiModels;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace commute_planner.DataProcessing;

public class DataProcessingExchange : CommutePlannerExchange
{
  public DataProcessingExchange(IConnection messaging, ILogger<CommutePlannerExchange> log)
    : base(messaging, log, DataProcessingRoutingKey)
  {
  }

  protected override void OnMessage(string messageType, string routingKey, string message)
  {
    switch (messageType)
    {
      case nameof(CollectedStopsResponse):
      {
        var obj = JsonSerializer.Deserialize<CollectedStopsResponse>(message);
        if (obj != null)
        {
          StopsPosted?.Invoke(this, obj);
        }
        break;
      }
      case nameof(CollectedLinesResponse):
      {
        var obj = JsonSerializer.Deserialize<CollectedLinesResponse>(message);
        if (obj != null)
        {
          LinesPosted?.Invoke(this, obj);
        }
        break;
      }
      case nameof(CollectedStopMonitoringResponse):
      {
        var obj = JsonSerializer.Deserialize<CollectedStopMonitoringResponse>(message);
        if (obj != null)
        {
          StopMonitoringPosted?.Invoke(this, obj);
        }
        break;
      }
      case nameof(CollectedStopPlacesResponse):
      {
        var obj = JsonSerializer.Deserialize<CollectedStopPlacesResponse>(message);
        if (obj != null)
        {
          StopPlacesPosted?.Invoke(this, obj);
        }
        break;
      }
      case nameof(CollectedVehicleMonitoringResponse):
      {
        var obj = JsonSerializer.Deserialize<CollectedVehicleMonitoringResponse>(message);
        if (obj != null)
        {
          VehicleMonitoringPosted?.Invoke(this, obj);
        }
        break;
      }
      case nameof(CollectedScheduledDeparturesAtStopResponse):
      {
        var obj = JsonSerializer.Deserialize<CollectedScheduledDeparturesAtStopResponse>(message);
        if (obj != null)
        {
          ScheduledDeparturesPosted?.Invoke(this, obj);
        }
        break;
      }
    }
  }

  public event EventHandler<DrivingTrip> DrivingTripPosted;
  public event EventHandler<TransitTrip> TransitTripPosted;
  public event EventHandler<CollectedStopsResponse> StopsPosted;
  public event EventHandler<CollectedLinesResponse> LinesPosted;
  public event EventHandler<CollectedStopMonitoringResponse> StopMonitoringPosted;
  public event EventHandler<CollectedStopPlacesResponse> StopPlacesPosted;
  public event EventHandler<CollectedVehicleMonitoringResponse> VehicleMonitoringPosted;
  public event EventHandler<CollectedScheduledDeparturesAtStopResponse> ScheduledDeparturesPosted;
}
