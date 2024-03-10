using System.Text.Json.Nodes;

namespace commute_planner.TransitApi;

public class TransitApiClient(HttpClient httpClient)
{
  HttpClient HttpClient { get; } = httpClient;
  public async Task<JsonNode?> Stops(string operatorId, CancellationToken token = default)
  {
    var response = await HttpClient.GetAsync($"stops?operator_id={operatorId}", token);
    response.EnsureSuccessStatusCode();
    var jsonResponse = await response.Content.ReadAsStringAsync(token);
    return JsonNode.Parse(jsonResponse);
  }
  public async Task<JsonNode?> StopPlaces(string operatorId, string stopId, CancellationToken token = default)
  {
    var response = await HttpClient.GetAsync($"stopplaces?operator_id={operatorId}&stop_id={stopId}", token);
    response.EnsureSuccessStatusCode();
    var jsonResponse = await response.Content.ReadAsStringAsync(token);
    return JsonNode.Parse(jsonResponse);
  }
  public async Task<JsonNode?> StopMonitoring(string operatorId, string stopId, CancellationToken token = default)
  {
    var response = await HttpClient.GetAsync($"StopMonitoring?agency={operatorId}&stopcode={stopId}", token);
    response.EnsureSuccessStatusCode();
    var jsonResponse = await response.Content.ReadAsStringAsync(token);
    return JsonNode.Parse(jsonResponse);
  }
  public async Task<JsonNode?> VehicleMonitoring(string operatorId, string vehicleId, CancellationToken token = default)
  {
    var response = await HttpClient.GetAsync($"VehicleMonitoring?agency={operatorId}&vehicleID={vehicleId}", token);
    response.EnsureSuccessStatusCode();
    var jsonResponse = await response.Content.ReadAsStringAsync(token);
    return JsonNode.Parse(jsonResponse);
  }
  public async Task<JsonNode?> Lines(string operatorId, CancellationToken token = default)
  {
    var response = await HttpClient.GetAsync($"lines?operator_id={operatorId}", token);
    response.EnsureSuccessStatusCode();
    var jsonResponse = await response.Content.ReadAsStringAsync(token);
    return JsonNode.Parse(jsonResponse);
  }
}