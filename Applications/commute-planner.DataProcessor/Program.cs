using System.Text.Json;
using commute_planner.CommuteDatabase;
using commute_planner.DataProcessing;

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

builder.AddNpgsqlDbContext<CommutePlannerDbContext>("commute_db",
  settings =>
    settings.ConnectionString = 
      builder.Configuration.GetConnectionString("AZURE_POSTGRESQL_CONNECTIONSTRING")
      ?? "Host=localhost;Database=commute_db");

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, eventArgs) =>
{
  Console.WriteLine("\nCancel keys pressed, exiting.");
  cts.Cancel();
};

// Add a console logger.
builder.Services.AddLogging(configure => configure.AddConsole());

// Add a hosted service
builder.Services.AddDataProcessingServices();  // This app

var app = builder.Build();

// When running hosted services
await app.RunAsync(cts.Token);
