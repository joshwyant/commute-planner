using System.Net.Http.Json;

namespace commute_planner.ApiClient;

public class CommutePlannerApiClient(HttpClient httpClient)
{
  public HttpClient HttpClient { get; } = httpClient;

  public Task<Trip?> GetLatestTripAsync(int routeId)
    => HttpClient.GetFromJsonAsync<Trip>(
      $"latestTrip?routeId={routeId}");

  public Task<Route[]?> GetRoutesAsync()
    => HttpClient.GetFromJsonAsync<Route[]?>($"routes");
}

// ReSharper disable ClassNeverInstantiated.Global
public record Trip(
  Route Route,
  string DrivingTime,
  string TransitTime,
  string LastUpdated,
  bool IsDrivingFaster);

public record Route(
  int MatchingRouteId,
  string Name,
  TransitRoute TransitRoute,
  DrivingRoute DrivingRoute);
public record TransitRoute(string Description, string Line);
public record DrivingRoute(string Description);
// ReSharper restore ClassNeverInstantiated.Global
