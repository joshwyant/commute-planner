using System.Text.Json;
using commute_planner.CommuteDatabase;
using commute_planner.CommuteDatabase.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.AddRabbitMQ("messaging", settings =>
{
    //settings.ConnectionString = "amqp://localhost:5672/";
});
builder.AddNpgsqlDbContext<CommutePlannerDbContext>("commute_db",
    settings =>
        settings.ConnectionString = "Host=localhost;Database=commute_db");

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, eventArgs) =>
{
  Console.WriteLine("\nCancel keys pressed, exiting.");
  cts.Cancel();
};

// Add a console logger.
builder.Services.AddLogging(configure => configure.AddConsole());

// Add a hosted service
// builder.Services.AddHostedService<DataFetcherService>();

var app = builder.Build();

// Route suggestions and descriptions by ChatGPT.
RoutePair[] pairs = [
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

// Seed the database.
using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider
    .GetService<CommutePlannerDbContext>();

if (dbContext.Database.EnsureCreated())
{
    foreach (var pair in pairs)
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
        dbContext.TransitRoutes.Add(transitRoute);
        dbContext.DrivingRoutes.Add(autoRoute);
        dbContext.MatchingRoutes.Add(match);
    }

    await dbContext.SaveChangesAsync();
}

// When running hosted services
// await app.StartAsync(cts.Token);

// Simple inline model for defining the seed data constants
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
