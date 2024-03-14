using System.Net;
using System.Text.Json;
using commute_planner.TransitApi;
using commute_planner.TransitApiModels;
using Moq;
using Moq.Protected;

namespace commute_planner.TransitApiClient.Tests;

public class TransitApiClientTests
{
  [SetUp]
  public void Setup()
  {
  }

  [Test]
  public async Task TestLines()
  {
    // Arrange
    var json = await File.ReadAllTextAsync("Resources/lines.json");
    var httpClient = CreateMockHttpClient(HttpMethod.Get, "/transit/lines",
      new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(json)
      });
    var apiClient = new TransitApi.TransitApiClient(httpClient);
    
    // Act
    var lines = await apiClient.Lines("SF");
    
    // Assert
    Assert.That(lines, Is.Not.Null);
    Assert.That(lines, Has.Length.Not.Zero);
    Assert.That(lines, Has.One.Matches<Line>(l => l.Id == "K"));
    var k = lines?.SingleOrDefault(l => l.Id == "K");
    Assert.That(k?.Name, Is.EqualTo("INGLESIDE"));
    Assert.That(k?.OperatorRef, Is.EqualTo("SF"));
  }

  [Test]
  public async Task TestStopMonitoring()
  {
    // Arrange
    var json = await File.ReadAllTextAsync("Resources/StopMonitoring.json");
    var httpClient = CreateMockHttpClient(HttpMethod.Get, "/transit/StopMonitoring",
      new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(json)
      });
    var apiClient = new TransitApi.TransitApiClient(httpClient);
    
    // Act
    var response = (await apiClient.StopMonitoring("SF", "15665")).ToArray();

    // Assert
    Assert.That(response, Has.Length.Not.Zero);
    Assert.That(response,
      Has.One.Matches<MonitoredStopVisit>(v =>
        v?.MonitoredVehicleJourney?.VehicleRef == "8527"));
    var bus = response?.Single(visit =>
      visit.MonitoredVehicleJourney.VehicleRef == "8527");
    Assert.That(bus.MonitoringRef == "15665");
    var journey = bus.MonitoredVehicleJourney;
    Assert.That(((JsonElement)journey.Monitored).GetBoolean(), Is.True);
    Assert.That(journey.VehicleLocation, Is.Not.Null);
    Assert.That(journey.VehicleLocation?.Longitude, Is.Not.Empty);
    Assert.That(journey.VehicleLocation?.Latitude, Is.Not.Empty);
    Assert.That(journey.DirectionRef, Is.EqualTo("IB"));
    Assert.That(journey.DestinationName, Is.EqualTo("Masonic Ave & Haight St"));
    Assert.That(journey.VehicleRef, Is.EqualTo("8527"));
    Assert.That(journey.MonitoredCall, Is.Not.Null);
    Assert.That(journey.MonitoredCall?.StopPointName, Is.EqualTo("Market St & Castro St"));
    Assert.That(journey.MonitoredCall?.StopPointRef, Is.EqualTo("15665"));
  }

  [Test]
  public async Task TestStopPlaces()
  {
    // Arrange
    var json = await File.ReadAllTextAsync("Resources/stopplaces.json");
    var httpClient = CreateMockHttpClient(HttpMethod.Get, "/transit/stopplaces",
      new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(json)
      });
    var apiClient = new TransitApi.TransitApiClient(httpClient);
    
    // Act
    var response = await apiClient.StopPlaces("SF", "15665");

    // Assert
    Assert.That(response, Is.Not.Null);
    Assert.That(response?.Name, Is.Not.Null);
    Assert.That(response?.Name, Is.EqualTo("Market St & Castro St"));
    Assert.That(response?.Id, Is.EqualTo("15665"));
    Assert.That(response?.TransportMode, Is.Not.Null);
    Assert.That(response?.TransportMode, Is.EqualTo("bus"));
    Assert.That(response?.StopPlaceType, Is.Not.Null);
    Assert.That(response?.StopPlaceType, Is.EqualTo("onstreetBus"));
  }

  [Test]
  public async Task TestStops()
  {
    // Arrange
    var json = await File.ReadAllTextAsync("Resources/stops.json");
    var httpClient = CreateMockHttpClient(HttpMethod.Get, "/transit/stops",
      new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(json)
      });
    var apiClient = new TransitApi.TransitApiClient(httpClient);
    
    // Act
    var response = (await apiClient.Stops("SF")).ToArray();

    // Assert
    Assert.That(response, Has.Length.Not.Zero);
    Assert.That(response,
      Has.One.Matches<ScheduledStopPoint>(stop => stop.id == "15829"));
    var stop = response.Single(stop => stop.id == "15829");
    Assert.That(stop.Name, Is.EqualTo("100 O'Shaughnessy Blvd"));
    Assert.That(stop.Location, Is.Not.Null);
    Assert.That(stop.Location?.Latitude, Is.Not.Null);
    Assert.That(stop.Location?.Longitude, Is.Not.Null);
  }

  [Test]
  public async Task TestStopTimetable()
  {
    // Arrange
    var json = await File.ReadAllTextAsync("Resources/stoptimetable.json");
    var httpClient = CreateMockHttpClient(HttpMethod.Get, "/transit/stoptimetable",
      new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(json)
      });
    var apiClient = new TransitApi.TransitApiClient(httpClient);
    
    // Act
    var response = (await apiClient.ScheduledDeparturesAtStop("SF", "15665"))
      .ToArray();

    // Assert
    Assert.That(response, Has.Length.Not.Zero);
    var firstVisit = response.First();
    Assert.That(firstVisit.MonitoringRef, Is.EqualTo("15665"));
    Assert.That(firstVisit.TargetedVehicleJourney, Is.Not.Null);
    var journey = firstVisit.TargetedVehicleJourney;
    Assert.That(journey.DestinationName, Is.EqualTo("Masonic Ave & Haight St"));
    Assert.That(journey.DirectionRef, Is.EqualTo("IB"));
    Assert.That(journey.TargetedCall, Is.Not.Null);
    Assert.That(journey.TargetedCall?.StopPointName,
      Is.EqualTo("Market St & Castro St"));
  }

  [Test]
  public async Task TestVehicleMonitoring()
  {
    // Arrange
    var xml = await File.ReadAllTextAsync("Resources/VehicleMonitoring.xml");
    var httpClient = CreateMockHttpClient(HttpMethod.Get, "/transit/VehicleMonitoring",
      new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(xml)
      });
    var apiClient = new TransitApi.TransitApiClient(httpClient);
    
    // Act
    var response = await apiClient.VehicleMonitoring("SF", "2110");

    // Assert
    Assert.That(response, Is.Not.Null);
    Assert.That(response?.VehicleRef, Is.EqualTo("2110"));
    Assert.That(response?.OperatorRef, Is.EqualTo("SF"));
    Assert.That(response?.OriginName, Is.EqualTo("Chinatown - Rose Pak Station"));
    Assert.That(response?.MonitoredCall, Is.Not.Null);
    Assert.That(response?.MonitoredCall?.StopPointRef, Is.Not.Null);
    Assert.That(response?.MonitoredCall?.StopPointName, Is.EqualTo("Third Street & Williams Ave"));
    Assert.That(response?.OnwardCalls, Is.Not.Null);
    Assert.That(response?.OnwardCalls, Has.Length.Not.Zero);
    var call = response?.OnwardCalls?.First();
    Assert.That(call.StopPointName, Is.EqualTo("Third Street & Carroll Ave"));
  }

  private HttpClient CreateMockHttpClient(HttpMethod method, string apiPath, HttpResponseMessage response)
  {
    // Set up a mock HTTP handler.
    var handler = new Mock<HttpMessageHandler>();
    handler.Protected()
      .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(msg =>
          msg.Method == method &&
          msg.RequestUri != null &&
          msg.RequestUri.AbsolutePath
          == apiPath),
        ItExpr.IsAny<CancellationToken>())
      .ReturnsAsync(response);
    var httpClient = new HttpClient(handler.Object);
    httpClient.BaseAddress = new Uri("https://example.transit.api.com/transit/");

    return httpClient;
  }
}