using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using commute_planner.TransitApiModels;

namespace commute_planner.TransitApi;

public class TransitApiClient(HttpClient httpClient)
{
  HttpClient HttpClient { get; } = httpClient;

  public async Task<IEnumerable<ScheduledStopPoint>> Stops(string operatorId,
    CancellationToken token = default)
  {
    var response =
      await HttpClient.GetAsync($"stops?format=json&operator_id={operatorId}",
        token);
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

  public async Task<StopPlace?> StopPlaces(string operatorId, string stopId,
    CancellationToken token = default)
  {
    // return await HttpClient.GetFromJsonAsync<StopPlace[]>(  );

    var response = await HttpClient.GetAsync(
      $"stopplaces?format=json&operator_id={operatorId}&stop_id={stopId}",
      token);
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
      .GetProperty("StopPlace")
      .Deserialize<StopPlace>();
    // .EnumerateArray()
    // .Select(stopVisit => 
    //   stopVisit.Deserialize<StopPlace>())
    // .Where(visit => visit is not null).Cast<StopPlace>();

    return stopPlaces;
  }

  public async Task<IEnumerable<MonitoredStopVisit>> StopMonitoring(
    string operatorId, string stopId, CancellationToken token = default)
  {
    var response = await HttpClient.GetAsync(
      $"StopMonitoring?format=json&agency={operatorId}&stopcode={stopId}",
      token);
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

  public async Task<MonitoredVehicleJourneyXml?> VehicleMonitoring(
    string operatorId, string vehicleId, CancellationToken token = default)
  {
    var response = await HttpClient.GetAsync(
      $"VehicleMonitoring?format=xml&agency={operatorId}&vehicleID={vehicleId}",
      token);
    response.EnsureSuccessStatusCode();
    var stream = await response.Content.ReadAsStreamAsync(token);
    var xml = await XDocument.LoadAsync(stream, LoadOptions.None, token);
    XmlNamespaceManager namespaceManager =
      new XmlNamespaceManager(new NameTable());
    namespaceManager.AddNamespace("siri", "http://www.siri.org.uk/siri");
    var element =
      xml.XPathSelectElement("//MonitoredVehicleJourney", namespaceManager) ??
      throw new XmlException("Could not find the data.");
    var serializer = new XmlSerializer(typeof(MonitoredVehicleJourneyXml));
    using var reader = element.CreateReader();
    return (MonitoredVehicleJourneyXml?)serializer.Deserialize(reader);
  }

  public async Task<Line[]?> Lines(string operatorId,
    CancellationToken token = default)
  {
    return await HttpClient.GetFromJsonAsync<Line[]>(
      $"lines?format=json&operator_id={operatorId}", token);
  }

  public async Task<IEnumerable<TimetabledStopVisit>> ScheduledDeparturesAtStop(
    string operatorId, string stopId, CancellationToken token = default)
  {
    var response = await HttpClient.GetAsync(
      $"stoptimetable?format=json&operatorref={operatorId}&monitoringref={stopId}",
      token);
    response.EnsureSuccessStatusCode();

    var stream = await response.Content.ReadAsStreamAsync(token);
    var json = await JsonDocument.ParseAsync(stream, default, token);
    var visits = json.RootElement
      .GetProperty("Siri")
      .GetProperty("ServiceDelivery")
      .GetProperty("StopTimetableDelivery")
      .GetProperty("TimetabledStopVisit")
      .EnumerateArray()
      .Select(stopVisit =>
        stopVisit.Deserialize<TimetabledStopVisit>())
      .Where(visit => visit is not null)
      .Cast<TimetabledStopVisit>();

    return visits;
  }
}
