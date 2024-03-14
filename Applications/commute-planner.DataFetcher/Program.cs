﻿using System.Net;
using commute_planner.DataCollection;
using commute_planner.MapsApi;
using commute_planner.TransitApi;
using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.AddRabbitMQ("messaging", settings =>
{
  var config =
    builder.Configuration.GetConnectionString("CLOUDAMPQ_CONNECTION_STRING");

  if (config != null)
    settings.ConnectionString = config;
});

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, eventArgs) =>
{
  Console.WriteLine("\nCancel keys pressed, exiting.");
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

var transitBaseUrl = Environment.GetEnvironmentVariable("TRANSIT_BASE_URL")
                     ?? "http://localhost:5273/transit/";

// Add HTTP client configurations for our Maps and Transit APIs
builder.Services.AddMapsApiHttpClient(googleBaseUrl, googleApiKey);
builder.Services.AddTransitApiHttpClient(transitBaseUrl, transitApiKey);

// Add a console logger.
builder.Services.AddLogging(configure => configure.AddConsole());

// Add our hosted services
builder.Services.AddDataCollectionServices();  // This app

var app = builder.Build();

// When running hosted services
await app.RunAsync(cts.Token);
