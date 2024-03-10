using System.Net;
using commute_planner.MapsApi;
using commute_planner.TransitApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, eventArgs) =>
{
  Console.WriteLine("Cancel keys pressed, exiting.");
  cts.Cancel();
};

var services = new ServiceCollection();

var googleApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ??
                   throw new InvalidOperationException(
                     "Missing Google API key.");

var transitApiKey = Environment.GetEnvironmentVariable("TRANSIT_API_KEY") ??
                    throw new InvalidOperationException(
                      "Missing transit API key.");

var googleBaseUrl = Environment.GetEnvironmentVariable("GOOGLE_BASE_URL")
                    ?? "https://routes.googleapis.com/directions/";

var transitBaseUrl = Environment.GetEnvironmentVariable("TRANSIT_BASE_URL")
                    ?? "https://api.511.org/transit/";

// Add HTTP client configurations for our Maps and Transit APIs
services.AddMapsApiHttpClient(googleBaseUrl, googleApiKey);
services.AddTransitApiHttpClient(transitBaseUrl, transitApiKey);

// Add a console logger.
services.AddLogging(configure => configure.AddConsole());

var serviceProvider = services.BuildServiceProvider();
using var scope = serviceProvider.CreateScope();
var scopedServices = scope.ServiceProvider;

var log = scopedServices.GetService<ILogger<Program>>()!;
var mapsApi = scopedServices.GetService<MapsApiClient>();
var transitApi = scopedServices.GetService<TransitApiClient>();

const int apiInterval = 60000; // Once per minute
var backoff = apiInterval;

while (!cts.IsCancellationRequested)
{
  log.LogInformation("About to read some maps data.");
  try
  {
    var mapsResult = await mapsApi.ComputeRoutes(new ComputeRoutesRequest(
      new Waypoint() { Address = "123 Main St." },
      new Waypoint() { Address = "555 Maple Ave." }));
    log.LogInformation(String.Join("\n",
      mapsResult?.Routes?.Select(r =>
        $"(DistanceMeters: {r.DistanceMeters}, Duration: {r.Duration})")?.ToArray() ?? []));
  }
  catch (Exception e)
  {
    log.LogError(e.ToString());
  }

  try
  {
    // Read some more data
    log.LogInformation("About to read some transit data.");
    var transitResult = await transitApi.Lines("SF", cts.Token);
    log.LogInformation($"Transit response: {transitResult}");
    backoff = apiInterval; // reset to normal interval
  }
  catch (HttpRequestException ex)
  {
    if (ex.StatusCode == HttpStatusCode.TooManyRequests)
    {
      log.LogInformation("Too many requests!");
      backoff = 3*backoff/2; // Add 50%
    }
  }
  catch (Exception ex)
  {
    log.LogError(ex.ToString());
  }

  // Delay
  log.LogInformation($"Backoff Delay {backoff}ms...");
  await Task.Delay(backoff);
}
