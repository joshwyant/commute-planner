using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace commute_planner.MapsApi;

public class MapsApiClient(HttpClient httpClient)
{
  public HttpClient HttpClient { get; } = httpClient;
  public async Task<ComputeRoutesResponse?> ComputeRoutes(
    ComputeRoutesRequest request, CancellationToken token = default)
  {
    var content = JsonContent.Create(request);
    content.Headers.Add("X-Goog-FieldMask",
      "routes.distanceMeters,routes.duration");

    var response =
      await HttpClient.PostAsync("/directions/v2:computeRoutes", content,
        token);

    var json = await response.Content.ReadAsStringAsync(token);
    return JsonSerializer.Deserialize<ComputeRoutesResponse>(json);
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