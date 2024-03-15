using System.Diagnostics;
using System.Net.Sockets;
using commute_planner.CommuteDatabase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace commute_planner.CommuteDatabase;

public static class ServiceExtensions
{
  public static async Task SetupCommuteDatabaseAsync(
    this IServiceProvider services, ILogger<CommutePlannerDbContext> log, 
    CancellationToken token = default)
  {
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider
      .GetRequiredService<CommutePlannerDbContext>();

    var backoff = 250;
    var dbIsNew = false;
    var sw = new Stopwatch(); sw.Start();
    var timedOut = true;
    using (var timeoutCts = new CancellationTokenSource(300000))  // Wait up to 5 minutes
    using (var linkedCts =
           CancellationTokenSource.CreateLinkedTokenSource(token,
             timeoutCts.Token))
    {
      while (true)
      {
        try
        {
          dbIsNew = await db.Database.EnsureCreatedAsync(linkedCts.Token);

          timedOut = false;
          sw.Stop();
          log.LogInformation(
            $"Connected to the database in {sw.ElapsedMilliseconds}ms");
          break;
        }
        catch (NpgsqlException e)
        {
          if (e.InnerException is SocketException
              {
                SocketErrorCode: SocketError.ConnectionRefused
              })
          {
            log.LogInformation(
              $"Attempt to connect to the database failed. Trying again in {backoff}ms");
            // The database is not ready; let's pause a little bit.
            await Task.Delay(backoff, linkedCts.Token);
            backoff = backoff * 3 / 2; // 1.5x
          }
        }
      }
    }

    if (timedOut)
      throw new TimeoutException(
        "Task canceled or timed out trying to connect to the database.");

    // Seed the database
    if (dbIsNew)
    {
      foreach (var pair in _pairs)
      {
        var transitRoute = new TransitRoute()
        {
          Name = pair.RouteName,
          Description = pair.TransitRouteDescription,
          OperatorId = pair.OperatorId,
          LineId = pair.LineId,
          ToStopId = pair.ToStopId,
          FromStopId = pair.FromStopId,
        };
        var autoRoute = new DrivingRoute()
        {
          Name = pair.RouteName,
          Description = pair.DrivingRouteDescription,
          FromAddress = pair.FromAddress,
          ToAddress = pair.ToAddress
        };
        var match = new MatchingRoute()
        {
          Name = pair.RouteName,
          DrivingRoute = autoRoute,
          TransitRoute = transitRoute,
        };
        db.TransitRoutes.Add(transitRoute);
        db.DrivingRoutes.Add(autoRoute);
        db.MatchingRoutes.Add(match);
      }

      await db.SaveChangesAsync(token);
    }

    // Perform migrations
    await db.Database.MigrateAsync(token);
  }

  // Seed data for the database.
  // Route suggestions and descriptions by ChatGPT.
  private static readonly RoutePair[] _pairs =
  [
    new("Ocean Beach to Financial District",
      "Utilize the N Judah line, beginning at Ocean Beach and concluding at Embarcadero Station, showcasing a scenic to urban commute.",
      "SF",
      "N",
      "15223",
      "16992",
      "A scenic drive along the Great Highway, transitioning to urban streets towards downtown, encountering varying traffic conditions.",
      "Judah St & La Playa St, San Francisco, CA 94122",
      "Financial District, San Francisco, CA"),
    new("Mission District to Salesforce Tower",
      "Journey on the J Church from the vibrant Mission District directly to the heart of tech at Salesforce Tower, emphasizing connectivity within the city.",
      "SF",
      "J",
      "16213",
      "15731",
      "Travels through the heart of San Francisco, highlighting the contrast between the Mission's vibrant streets and downtown's bustling business district.",
      "18th St & Church St, San Francisco, CA 94114",
      "Salesforce Tower, 415 Mission St, San Francisco, CA 94105"),
    new("Sunset District to Stonestown Galleria",
      "Take the L Taraval for a shopping excursion from the residential Sunset District to Stonestown Galleria, linking neighborhoods to commercial hubs.",
      "SF",
      "LBUS",
      "13599",
      "16617",
      "A straightforward route, mostly along 19th Avenue, offering a quick connection between residential areas and shopping destinations.",
      "46th Ave & Taraval St, San Francisco, CA 94116",
      "Stonestown Galleria, 3251 20th Ave, San Francisco, CA 94132"),
    new("Sunnydale to UCSF/Chase Center",
      "The T Third Street line connects Sunnydale to the UCSF/Chase Center, bridging community areas to major health and entertainment venues.",
      "SF",
      "T",
      "17396",
      "17360",
      "From the outskirts to the city's burgeoning biomedical and entertainment district, showcasing urban redevelopment and traffic diversity.",
      "Sunnydale Ave & Bayshore Blvd, San Francisco, CA 94134",
      "Chase Center, 1 Warriors Way, San Francisco, CA 94158"),
  ];

  record RoutePair(
    string RouteName,
    string TransitRouteDescription,
    string OperatorId,
    string LineId,
    string FromStopId,
    string ToStopId,
    string DrivingRouteDescription,
    string FromAddress,
    string ToAddress
  );
}