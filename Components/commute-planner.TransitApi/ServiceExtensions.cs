using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace commute_planner.TransitApi;

public static class ServiceCollectionExtensions
{
  public static ServiceCollection AddTransitApiHttpClient(
    this ServiceCollection services,
    string baseUrl, string apiKey)
  {
    services.AddHttpClient<TransitApiClient>(client =>
      {
        client.BaseAddress = new Uri(baseUrl);
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