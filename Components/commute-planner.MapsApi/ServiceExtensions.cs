using Microsoft.Extensions.DependencyInjection;

namespace commute_planner.MapsApi;

public static class ServiceExtensions
{
  public static ServiceCollection AddMapsApiHttpClient(
    this ServiceCollection services, string baseUrl,
    string apiKey)
  {
    services.AddHttpClient<MapsApiClient>(client =>
    {
      client.BaseAddress = new Uri(baseUrl);
      client.DefaultRequestHeaders.Add("X-Goog-Api-Key", apiKey);
    });

    return services;
  }
}