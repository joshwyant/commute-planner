using commute_planner.CommuteDatabase;
using commute_planner.DataProcessing;

var builder = Host.CreateApplicationBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.AddRabbitMQ("messaging");
builder.AddNpgsqlDbContext<CommutePlannerDbContext>("commute_db");

// Add a console logger.
builder.Services.AddLogging(configure => configure.AddConsole());
// Add a hosted service
builder.Services.AddDataProcessingServices();  // This app

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
  var logger = scope.ServiceProvider
    .GetRequiredService<ILogger<CommutePlannerDbContext>>();
  await app.Services.SetupCommuteDatabaseAsync(logger);
}

// When running hosted services
await app.RunAsync();
