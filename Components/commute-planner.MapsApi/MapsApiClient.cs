using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace commute_planner.MapsApi;

public class MapsApiClient(HttpClient httpClient)
{
  public async Task<ComputeRoutesResponse?> ComputeRoutes(
    ComputeRoutesRequest request, CancellationToken token = default)
  {
    var response = await httpClient.PostAsJsonAsync<ComputeRoutesRequest>(
      "/v2:computeRoutes", request, JsonSerializerOptions.Default, token);

    return await response.Content.ReadFromJsonAsync<ComputeRoutesResponse>(
      token);
  }
}

public enum RoutingPreference
{
  [JsonPropertyName("TRAFFIC_AWARE")]
  TrafficAware
}

public class ComputeRoutesResponse
{
  [JsonPropertyName("routes")]
  public Route[]? Routes { get; set; }
}

public class Route
{
  [JsonPropertyName("distanceMeters")]
  public int DistanceMeters { get; set; }
  
  [JsonPropertyName("duration")]
  public string? Duration { get; set; }
}

public class ComputeRoutesRequest(Waypoint origin, Waypoint destination)
{
  [JsonPropertyName("origin")]
  public Waypoint Origin { get; } = origin;
  
  [JsonPropertyName("destination")]
  public Waypoint Destination { get; } = destination;
  
  [JsonPropertyName("routingPreference")]
  public RoutingPreference? RoutingPreference { get; set; }
}

public class Waypoint
{
  [JsonPropertyName("address")]
  public string? Address { get; set; }
}