using commute_planner.EventCollaboration;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.Services.AddHostedService<EventCollaborationService>();

// Add services to the container.
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapGet("/routes", async () =>
{
  return new Route[]
  {
    new (1,
      "RouteName",
      new("TransitDescription", "J"),
      new("Driving Description"))
  };
});

app.MapGet("/latestTrip", async (int routeId) =>
{
  return new Trip(new(1, "RouteName", new("TransitDescription", "J"),
      new("Driving Description")), "Driving time", "Transit time",
    "Last updated time", IsDrivingFaster: true);
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
