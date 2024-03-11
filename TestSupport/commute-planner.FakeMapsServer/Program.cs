using System.Text.Json.Serialization;
using commute_planner.FakeMapsServer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ReSharper disable once UnusedParameter.Local
app.MapPost("/directions/v2:computeRoutes", (ComputeRoutesRequest request) =>
    {
        var m = Random.Shared.Next(15000);
        var kph = Random.Shared.Next(20, 80);
        var mps = kph * 1000 / 3600;
        var t = m / mps;
    return new ComputeRoutesResponse([new MapsRoute(m, $"{t}s")]);
})
.WithName("ComputeRoutes")
.WithOpenApi();

app.Run();

namespace commute_planner.FakeMapsServer
{
    public class ComputeRoutesResponse(MapsRoute[] routes)
    {
        [JsonPropertyName("routes")]
        public MapsRoute[]? Routes { get; set; } = routes;
    }
    
    public class MapsRoute(int distanceInMeters, string? duration)
    {
        [JsonPropertyName("distanceMeters")]
        public int DistanceMeters { get; set; } = distanceInMeters;

        [JsonPropertyName("duration")]
        public string? Duration { get; set; } = duration;
    }

    // ReSharper disable ClassNeverInstantiated.Global
    public class ComputeRoutesRequest(Waypoint origin, Waypoint destination)
    {
        [JsonPropertyName("origin")] public Waypoint Origin { get; } = origin;

        [JsonPropertyName("destination")]
        public Waypoint Destination { get; } = destination;

        [JsonPropertyName("routingPreference")]
        public string? RoutingPreference { get; set; }
    }

    public class Waypoint
    {
        [JsonPropertyName("address")] public string? Address { get; set; }
    }
    // ReSharper restore ClassNeverInstantiated.Global
}
