using System.Text.RegularExpressions;
using commute_planner.ApiService;
using commute_planner.CommuteDatabase;
using commute_planner.EventCollaboration;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components:
builder.AddServiceDefaults();

// RabbitMQ
// builder.AddRabbitMQ("messaging", settings 
//   => settings.ConnectionString = 
//     builder.Environment.IsDevelopment()
//       ? "Host=ampq://localhost:5672/"
//       : builder.Configuration.GetConnectionString("CLOUDAMPQ_CONNECTION_STRING"));
// builder.Services.AddEventCollaborationServices<ApiExchange>();

var connectionString = builder.Configuration.GetConnectionString(
                         "AZURE_POSTGRESQL_CONNECTIONSTRING")
                       ?? "Host=localhost;Database=commute_db";

var maskedConnectionString = Regex.Replace(connectionString,
  @"((?<=password=)|(?<=(pass|word)=))[^;]*(?=;|$)", "<hidden>",
  RegexOptions.Multiline | RegexOptions.IgnoreCase);

// Database
builder.AddNpgsqlDbContext<CommutePlannerDbContext>("commute_db",
  settings => settings.ConnectionString = connectionString);

// Add services to the container.
builder.Services.AddProblemDetails();

// Add logging
builder.Services.AddLogging(configure => configure.AddConsole());

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

using var scope = app.Services.CreateScope();
var log = scope.ServiceProvider.GetService<ILogger>();

log.LogInformation($"PostgreSQL connection string: '{maskedConnectionString}'");

//var exchange = scope.ServiceProvider.GetService<ApiExchange>();
var db = scope.ServiceProvider.GetService<CommutePlannerDbContext>();

app.MapGet("/routes", async () =>
{
  var cts = new CancellationTokenSource(30000); // 30 seconds timeout
  var backoff = 1000;
  while (!cts.Token.IsCancellationRequested)
  {
    var routes = await db.MatchingRoutes
      .AsNoTracking()  // So it hits the database each time (no checking for unmodified objects)
      .Include(r => r.DrivingRoute)
      .Include(r => r.TransitRoute)
      .ToArrayAsync(cts.Token);
    
    if (routes.Length > 0)
    {
      return routes
        .Select(r => new Route(r.MatchingRouteId, r.Name,
          new TransitRoute(r.TransitRoute.Description, r.TransitRoute.LineId),
          new DrivingRoute(r.DrivingRoute.Description)))
        .ToArray();
    }

    log.LogInformation($"No routes data was available. Waiting {backoff}ms...");
    await Task.Delay(backoff);
    backoff = backoff * 3 / 2;  // + 50%
  }
  log.LogError("/routes Timed out with no data in the database.");

  return default;
});

app.MapGet("/latestTrip", async (int routeId) =>
{
  var cts = new CancellationTokenSource(30000); // 30 seconds timeout
  var backoff = 1000;
  while (!cts.Token.IsCancellationRequested)
  {
    var trip = db.TripData.AsNoTracking()
      .Include(t => t.Route)
      .Include(t => t.Route.DrivingRoute)
      .Include(t => t.Route.TransitRoute)
      .OrderByDescending(trip => trip.Created).SingleOrDefault();

    if (trip != null)
    {
      var route = trip.Route;
      var transitRoute = route.TransitRoute;
      var drivingRoute = route.DrivingRoute;
      var drivingTime = $"{trip.DrivingTimeInSeconds/60} minutes";
      var transitTime = $"{trip.TransitTimeInSeconds/60} minutes";
      var lastUpdate = DateTime.Now - trip.Created;
      var lastUpdatedTime = lastUpdate.Minutes switch
      {
        0 => "just now",
        var m => $"{m} minutes ago"
      };
      var isDrivingFaster =
        trip.DrivingTimeInSeconds > trip.TransitTimeInSeconds;
      return new Trip(
        new Route(trip.MatchingRouteId, route.Name,
          new TransitRoute(transitRoute.Description, transitRoute.LineId),
          new DrivingRoute(drivingRoute.Description)), drivingTime, transitTime,
        lastUpdatedTime, isDrivingFaster);
    }

    log.LogInformation($"No trip data was available. Waiting {backoff}ms...");
    await Task.Delay(backoff);
    backoff = backoff * 3 / 2;  // + 50%
  }
  log.LogError("/latestTrip Timed out with no data in the database.");

  return default;
});

app.MapDefaultEndpoints();

app.Run();

// API models
public record Trip(
  Route Route,
  string DrivingTime,
  string TransitTime,
  string LastUpdated,
  bool IsDrivingFaster);
public record Route(
  int MatchingRouteId,
  string Name,
  TransitRoute TransitRoute,
  DrivingRoute DrivingRoute);
public record TransitRoute(string Description, string Line);
public record DrivingRoute(string Description);
