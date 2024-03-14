using commute_planner.TransitApiModels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace commute_planner.EventCollaboration;

public interface ICommutePlannerExchange : IBasicConsumer
{
  void Close();
  void Publish<T>(string routingKey, T message);
}

public record DrivingTrip(int RouteId, int TimeInSeconds, DateTime Created);
public record TransitTrip(int RouteId, int TimeInSeconds, DateTime Created);

public record CollectFreshDrivingRouteRequest(
  int routeId,
  string FromAddress,
  string ToAddress);

public record CollectedDrivingDataResponse(
  int routeId, 
  string DrivingTime);

public record CollectFreshTransitRouteRequest(
  int routeId,
  string OperatorId,
  string LineId,
  string FromStopId,
  string ToStopId);
  
public record CollectedStopsResponse(int routeId, ScheduledStopPoint[] data);
public record CollectedStopPlacesResponse(int routeId, StopPlace data);
public record CollectedStopMonitoringResponse(int routeId, MonitoredStopVisit[] data);
public record CollectedVehicleMonitoringResponse(int routeId, MonitoredVehicleJourneyXml data);
public record CollectedLinesResponse(int routeId, Line[] data);
public record CollectedScheduledDeparturesAtStopResponse(int routeId, TimetabledStopVisit[] data);
