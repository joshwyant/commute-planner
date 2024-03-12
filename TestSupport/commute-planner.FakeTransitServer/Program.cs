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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/transit/lines", () => File.ReadAllText("Resources/lines.json"));
app.MapGet("/transit/StopMonitoring",
    () => File.ReadAllText("Resources/StopMonitoring.json"));
app.MapGet("/transit/stopplaces",
    () => File.ReadAllText("Resources/stopplaces.json"));
app.MapGet("/transit/stops", () => File.ReadAllText("Resources/stops.json"));
app.MapGet("/transit/stoptimetable",
    () => File.ReadAllText("Resources/stoptimetable.json"));
app.MapGet("/transit/VehicleMonitoring",
    () => File.ReadAllText("Resources/VehicleMonitoring.xml"));

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
