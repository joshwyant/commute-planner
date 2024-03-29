using Microsoft.Extensions.DependencyInjection;

namespace commute_planner.MapsApi;

public static class ServiceExtensions
{
  class HttpHandler : DelegatingHandler
  {
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      return base.SendAsync(request, cancellationToken);
    }
  }
  public static IServiceCollection AddMapsApiHttpClient(
    this IServiceCollection services, string? baseUrl,
    string apiKey)
  {
    services.AddHttpClient<MapsApiClient>(client =>
    {
      client.BaseAddress = new Uri(baseUrl ?? "http://maps");
      client.DefaultRequestHeaders.Add("X-Goog-Api-Key", apiKey);
    }).AddHttpMessageHandler(i => new HttpHandler());

    return services;
  }
}