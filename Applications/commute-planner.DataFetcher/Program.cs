using System.Net;
using commute_planner.DataCollection;
using commute_planner.EventCollaboration;
using commute_planner.MapsApi;
using commute_planner.TransitApi;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.AddRabbitMQ("messaging");

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
builder.Services.AddHostedService<DataCollectionService>();  // This app
builder.Services.AddHostedService<EventCollaborationService>();

var app = builder.Build();

// When running hosted services
await app.StartAsync(cts.Token);
