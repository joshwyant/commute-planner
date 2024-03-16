using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace commute_planner.TransitApi;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddTransitApiHttpClient(
    this IServiceCollection services,
    string? baseUrl, string apiKey)
  {
    services.AddHttpClient<TransitApiClient>(client =>
      {
        client.BaseAddress = new Uri(baseUrl ?? "http://transit");
      })
      .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
      {
        AutomaticDecompression =
          DecompressionMethods.GZip | DecompressionMethods.Deflate
      })
      .AddHttpMessageHandler(() =>
        new TransitApiHttpMessageHandler(apiKey));

    return services;
  }
}