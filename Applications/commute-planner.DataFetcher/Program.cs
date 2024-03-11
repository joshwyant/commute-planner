﻿using System.Net;
using commute_planner.MapsApi;
using commute_planner.TransitApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.AddRabbitMQ("messaging");

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, eventArgs) =>
{
  Console.WriteLine("Cancel keys pressed, exiting.");
  cts.Cancel();
};

var googleApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ??
                   throw new InvalidOperationException(
                     "Missing Google API key.");

var transitApiKey = Environment.GetEnvironmentVariable("TRANSIT_API_KEY") ??
                    throw new InvalidOperationException(
                      "Missing transit API key.");

var googleBaseUrl = Environment.GetEnvironmentVariable("GOOGLE_BASE_URL")
                    ?? "http://localhost:5043";
                    //?? "https://routes.googleapis.com/";

var transitBaseUrl = Environment.GetEnvironmentVariable("TRANSIT_BASE_URL")
                    ?? "https://api.511.org/transit/";

// Add HTTP client configurations for our Maps and Transit APIs
builder.Services.AddMapsApiHttpClient(googleBaseUrl, googleApiKey);
builder.Services.AddTransitApiHttpClient(transitBaseUrl, transitApiKey);

// Add a console logger.
builder.Services.AddLogging(configure => configure.AddConsole());

// Add a hosted service
// builder.Services.AddHostedService<DataFetcherService>();

var app = builder.Build();

// When running hosted services
// await app.StartAsync(cts.Token);

var log = app.Services.GetService<ILogger<Program>>()!;
var mapsApi = app.Services.GetService<MapsApiClient>();
var transitApi = app.Services.GetService<TransitApiClient>();

const int apiInterval = 60000; // Once per minute
var backoff = apiInterval;

while (!cts.IsCancellationRequested)
{
  log.LogInformation("About to read some maps data.");
  try
  {
    var mapsResult = await mapsApi.ComputeRoutes(new ComputeRoutesRequest(
      new Waypoint() { Address = "345 Spear St. San Francisco, CA 94105" },
      new Waypoint() { Address = "415 Mission St, San Francisco, CA 94105" }));

    if (mapsResult is not null && mapsResult.Routes is not null)
    foreach (var r in mapsResult.Routes)
    {
      log.LogInformation($"Maps result: (DistanceMeters: {r.DistanceMeters}, Duration: {r.Duration})");
    }
  }
  catch (Exception e)
  {
    log.LogError(e.ToString());
  }

  try
  {
    // Read some more data
    log.LogInformation("About to read some transit data.");
    
    log.LogInformation("Read from Vehicle monitoring API:");
    var vehicleMonitoringResult =
      await transitApi.VehicleMonitoring("SF", "2009", cts.Token);
    log.LogInformation($"Transit response: {vehicleMonitoringResult}");
    if (vehicleMonitoringResult?.OnwardCalls is not null)
    foreach (var call in vehicleMonitoringResult.OnwardCalls)
    {
      log.LogInformation($"Onward call: {call}");
    }

    // log.LogInformation("Read from Stops API:");
    // var stopsResult = await transitApi.Stops("SF", cts.Token);
    // foreach (var stop in stopsResult)
    // {
    //   log.LogInformation($"Stop: {stop}");
    // }
    
    log.LogInformation("Read from StopPlaces API:");
    var stopPlacesResult =
      await transitApi.StopPlaces("SF", "15184", cts.Token);
    log.LogInformation($"Stop place: {stopPlacesResult}");
    // if (stopPlacesResult is not null)
    // foreach (var place in stopPlacesResult)
    // {
    //   log.LogInformation($"Stop place: {place}");
    // }
    
    log.LogInformation("Read from Stop Monitoring API:");
    var stopMonitoringResult =
      await transitApi.StopMonitoring("SF", "15184", cts.Token);
    foreach (var visit in stopMonitoringResult)
    {
      log.LogInformation($"Visit: {visit}");
    }
    
    log.LogInformation("Read from Lines API:");
    var linesResult = await transitApi.Lines("SF", cts.Token);
    foreach (var line in linesResult)
    {
      log.LogInformation($"Line: {line}");
    }
    
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
