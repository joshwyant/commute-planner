using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace commute_planner.TransitApiModels;



public record Line(
  string Id,
  string Name,
  string? TransportMode,
  string? PublicCode,
  string OperatorRef,
  bool? Monitored);

public class Location
{
  public string? Latitude { get; set; }
  public string? Longitude { get; set; }
}

public record ScheduledStopPoint(
  string id,
  string? Name,
  Location? Location,
  string? Url,
  string? StopType);
public record Operator(
  string Id,
  string? Name,
  string? ShortName,
  string? TimeZone,
  string? PrimaryMode,
  string? OtherModes,
  bool? Monitored
);

public class StopPlace
{
  [JsonPropertyName("@id")]
  public string Id { get; set; }
  public string? Name { get; set; }
  public string? TransportMode { get; set; }
  public string? StopPlaceType { get; set; }
}

public record MonitoredStopVisit
  (
    DateTime RecordedAtTime,
    string? MonitoringRef,
    MonitoredVehicleJourney MonitoredVehicleJourney);

public record MonitoredVehicleJourney
(
  string OperatorRef,
  string LineRef,
  string DirectionRef,
  string PublishedLineName,
  string? OriginRef,
  string? OriginName,
  string? DestinationRef,
  string? DestinationName,
  object? Monitored,
  string? InCongestion,
  Location? VehicleLocation,
  string? Bearing,
  string? VehicleRef,
  MonitoredCall? MonitoredCall,
  OnwardCall[]? OnwardCalls
);

[XmlType(TypeName = "MonitoredVehicleJourney")]
public class MonitoredVehicleJourneyXml
{
  public string OperatorRef;
  public string LineRef;
  public string DirectionRef;
  public string PublishedLineName;
  public string? OriginRef;
  public string? OriginName;
  public string? DestinationRef;
  public string? DestinationName;
  public object? Monitored;
  public string? InCongestion;
  public Location? VehicleLocation;
  public string? Bearing;
  public string? VehicleRef;
  public MonitoredCallXml? MonitoredCall;
  public OnwardCallXml[]? OnwardCalls;
}

public record MonitoredCall(
  string StopPointRef,
  string StopPointName,
  string? VehicleLocationAtStop,
  string? VehicleAtStop,
  string? DestinationDisplay,
  string? AimedArrivalTime,
  string? ExpectedArrivalTime,
  string? ActualArrivalTime,
  string? AimedDepartureTime,
  string? ExpectedDepartureTime,
  string? ActualDepartureTime);

[XmlType(TypeName = "MonitoredCall")]
public class MonitoredCallXml
{
  public string StopPointRef;
  public string StopPointName;
  public string? VehicleLocationAtStop;
  public string? VehicleAtStop;
  public string? DestinationDisplay;
  public string? AimedArrivalTime;
  public string? ExpectedArrivalTime;
  public string? ActualArrivalTime;
  public string? AimedDepartureTime;
  public string? ExpectedDepartureTime;
  public string? ActualDepartureTime;
}

[XmlType(TypeName = "OnwardCall")]
public record OnwardCallXml
{
  public required string StopPointRef;
  public required string StopPointName;
  public string? VehicleLocationAtStop;
  public string? VehicleAtStop;
  public string? DestinationDisplay;
  // public string? AimedArrivalTime;
  // public string? ExpectedArrivalTime;
  public string? ActualArrivalTime;
  public string? AimedDepartureTime;
  // public string? ExpectedDepartureTime;
  // public string? ActualDepartureTime;
}

public record OnwardCall
{
  public required string StopPointRef;
  public required string StopPointName;
  public string? VehicleLocationAtStop;
  public string? VehicleAtStop;
  public string? DestinationDisplay;
  // public string? AimedArrivalTime;
  // public string? ExpectedArrivalTime;
  public string? ActualArrivalTime;
  public string? AimedDepartureTime;
  // public string? ExpectedDepartureTime;
  // public string? ActualDepartureTime;
}

public record TimetabledStopVisit(
  DateTime RecordedAtTime,
  string MonitoringRef,
  TargetedVehicleJourney TargetedVehicleJourney);

public record TargetedVehicleJourney(
  string LineRef,
  string DirectionRef,
  string? PublishedLineName,
  string? OperatorRef,
  string? OriginRef,
  string? OriginName,
  string? DestinationRef,
  string? DestinationName,
  string? VehicleJourneyName,
  TargetedCall? TargetedCall
  );
  
public record TargetedCall(
  string? StopPointRef,
  string? StopPointName,
  string? DestinationDisplay,
  string VisitNumber,
  DateTime AimedArrivalTime,
  DateTime AimedDepartureTime);
  