using System.Text.RegularExpressions;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using commute_planner.CommuteDatabase;
using commute_planner.CommuteDatabase.Models;
using commute_planner.EventCollaboration;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components:
builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("commutedb");
// Console.WriteLine($"Connection string: '{connectionString}'");

// Database
builder.AddNpgsqlDbContext<CommutePlannerDbContext>("commutedb",
  settings => settings.ConnectionString = connectionString);

// Add services to the container.
builder.Services.AddProblemDetails();

var app = builder.Build();

await app.Services.SetupCommuteDatabaseAsync<Program>();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapGet("/routes",
  async (CancellationToken token, CommutePlannerDbContext db,
    ILogger<Program> log) =>
  {
    using var
      timeoutCts = new CancellationTokenSource(30000); // 30 seconds timeout
    using var linkedCts =
      CancellationTokenSource.CreateLinkedTokenSource(token, timeoutCts.Token);
    var backoff = 1000;
    while (!linkedCts.Token.IsCancellationRequested)
    {
      var routes = await db.MatchingRoutes
        .AsNoTracking() // So it hits the database each time (no checking for unmodified objects)
        .Include(r => r.DrivingRoute)
        .Include(r => r.TransitRoute)
        .ToArrayAsync(linkedCts.Token);

      if (routes.Length > 0)
      {
        return routes
          .Select(r => new Route(r.MatchingRouteId, r.Name,
            new TransitRoute(r.TransitRoute.Description, r.TransitRoute.LineId),
            new DrivingRoute(r.DrivingRoute.Description)))
          .ToArray();
      }

      log.LogInformation(
        $"No routes data was available. Waiting {backoff}ms...");
      await Task.Delay(backoff, linkedCts.Token);
      backoff = backoff * 3 / 2; // + 50%
    }

    log.LogError("/routes Timed out with no data in the database.");

    return default;
  });

app.MapGet("/latestTrip", async (CancellationToken token,
  CommutePlannerDbContext db, ILogger<Program> log, int routeId) =>
{
  using var
    timeoutCts = new CancellationTokenSource(30000); // 30 seconds timeout
  using var linkedCts =
    CancellationTokenSource.CreateLinkedTokenSource(token, timeoutCts.Token);
  var backoff = 1000;
  while (!linkedCts.Token.IsCancellationRequested)
  {
    try
    {
      /// Fake data
      
      var matchingRoute = await db.MatchingRoutes
        .Include(r => r.TransitRoute)
        .Include(r => r.DrivingRoute)
        .SingleOrDefaultAsync(r => r.MatchingRouteId == routeId,
          linkedCts.Token);

      var driving_m = Random.Shared.Next(15000);
      var driving_kph = Random.Shared.Next(20, 80);
      var driving_mps = driving_kph * 1000 / 3600;
      var driving_t = driving_m / driving_mps;
      
      var tm = Random.Shared.Next(15000);
      var kph = Random.Shared.Next(20, 80);
      var mps = kph * 1000 / 3600;
      var t = tm / mps;
      
      var trip = new TripData
      {
        Created = DateTime.Now,
        Route = matchingRoute,
        MatchingRouteId = routeId,
        DrivingTimeInSeconds = driving_t,
        TransitTimeInSeconds = t
      };
      
      /// End fake data

      if (trip != null)
      {
        var route = trip.Route;
        var transitRoute = route.TransitRoute;
        var drivingRoute = route.DrivingRoute;
        var drivingTime = $"{trip.DrivingTimeInSeconds / 60} minutes";
        var transitTime = $"{trip.TransitTimeInSeconds / 60} minutes";
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
            new DrivingRoute(drivingRoute.Description)), drivingTime,
          transitTime,
          lastUpdatedTime, isDrivingFaster);
      }
    }
    catch (TaskCanceledException)
    {
      log.LogInformation("Task canceled while finding trip data");
    }

    log.LogInformation($"No trip data was available. Waiting {backoff}ms...");
    await Task.Delay(backoff, linkedCts.Token);
    backoff = backoff * 3 / 2; // + 50%
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
