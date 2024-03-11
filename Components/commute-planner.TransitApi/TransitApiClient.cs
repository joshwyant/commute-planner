using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace commute_planner.TransitApi;

public class TransitApiClient(HttpClient httpClient)
{
  HttpClient HttpClient { get; } = httpClient;
  public async Task<IEnumerable<ScheduledStopPoint>> Stops(string operatorId, CancellationToken token = default)
  {
    var response = await HttpClient.GetAsync($"stops?format=json&operator_id={operatorId}", token);
    response.EnsureSuccessStatusCode();
    
    var stream = await response.Content.ReadAsStreamAsync(token);
    var json = await JsonDocument.ParseAsync(stream, default, token);
    var stops = json.RootElement.GetProperty("Contents")
      .GetProperty("dataObjects")
      .GetProperty("ScheduledStopPoint")
      .EnumerateArray()
      .Select(stop => 
        stop.Deserialize<ScheduledStopPoint>())
      .Where(stop => stop is not null).Cast<ScheduledStopPoint>();
    
    return stops;
  }
  public async Task<StopPlace?> StopPlaces(string operatorId, string stopId, CancellationToken token = default)
  {
    // return await HttpClient.GetFromJsonAsync<StopPlace[]>(  );
    
    var response = await HttpClient.GetAsync($"stopplaces?format=json&operator_id={operatorId}&stop_id={stopId}", token);
    response.EnsureSuccessStatusCode();
    
    var stream = await response.Content.ReadAsStreamAsync(token);
    var json = await JsonDocument.ParseAsync(stream, default, token);
    
    var stopPlaces = json.RootElement
      .GetProperty("Siri")
      .GetProperty("ServiceDelivery")
      .GetProperty("DataObjectDelivery")
      .GetProperty("dataObjects")
      .GetProperty("SiteFrame")
      .GetProperty("stopPlaces")
      .Deserialize<StopPlace>();
      // .EnumerateArray()
      // .Select(stopVisit => 
      //   stopVisit.Deserialize<StopPlace>())
      // .Where(visit => visit is not null).Cast<StopPlace>();
    
    return stopPlaces;
  }
  public async Task<IEnumerable<MonitoredStopVisit>> StopMonitoring(string operatorId, string stopId, CancellationToken token = default)
  {
    var response = await HttpClient.GetAsync($"StopMonitoring?format=json&agency={operatorId}&stopcode={stopId}", token);
    response.EnsureSuccessStatusCode();
    
    var stream = await response.Content.ReadAsStreamAsync(token);
    var json = await JsonDocument.ParseAsync(stream, default, token);
    var visits = json.RootElement
      .GetProperty("ServiceDelivery")
      .GetProperty("StopMonitoringDelivery")
      .GetProperty("MonitoredStopVisit")
      .EnumerateArray()
      .Select(stopVisit => 
        stopVisit.Deserialize<MonitoredStopVisit>())
      .Where(visit => visit is not null).Cast<MonitoredStopVisit>();
    
    return visits;
  }
  public async Task<MonitoredVehicleJourney?> VehicleMonitoring(string operatorId, string vehicleId, CancellationToken token = default)
  {
    var response = await HttpClient.GetAsync($"VehicleMonitoring?format=xml&agency={operatorId}&vehicleID={vehicleId}", token);
    response.EnsureSuccessStatusCode();
    var stream = await response.Content.ReadAsStreamAsync(token);
    var xml = await XDocument.LoadAsync(stream, LoadOptions.None, token);
    XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
    namespaceManager.AddNamespace("siri", "http://www.siri.org.uk/siri");
    var element =
      xml.XPathSelectElement("//MonitoredVehicleJourney", namespaceManager) ??
      throw new XmlException("Could not find the data.");
    var serializer = new XmlSerializer(typeof(MonitoredVehicleJourney));
    using var reader = element.CreateReader();
    return (MonitoredVehicleJourney?)serializer.Deserialize(reader);
  }
  
  public async Task<Line[]?> Lines(string operatorId, CancellationToken token = default)
  {
    return await HttpClient.GetFromJsonAsync<Line[]>(
      $"lines?format=json&operator_id={operatorId}", token);
  }
}

public record Line(
  string Id,
  string Name,
  string? TransportMode,
  string? PublicCode,
  string OperatorRef,
  bool? Monitored);

public record Location
{
  public string? Latitude;
  public string? Longitude;
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

public record StopPlace(
  string Id,
  string? Name, 
  string? ShortName, 
  string? TimeZone, 
  string? PrimaryMode, 
  bool? Monitored, 
  string? OtherModes);

public record MonitoredStopVisit(
  DateTime RecordedAtTime,
  string? MonitoringRef,
  MonitoredVehicleJourney MonitoredVehicleJourney
);

public record MonitoredVehicleJourney
{
  public required string OperatorRef;
  public required string LineRef;
  public required string DirectionRef;
  public required string PublishedLineName;
  public string? OriginRef;
  public string? OriginName;
  public string? DestinationRef;
  public string? DestinationName;
  public string? Monitored;
  public string? InCongestion;
  public Location? VehicleLocation;
  public MonitoredCall? MonitoredCall;
  public OnwardCall[]? OnwardCalls;
}

public record MonitoredCall
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