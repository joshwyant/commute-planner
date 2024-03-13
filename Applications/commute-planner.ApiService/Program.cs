using commute_planner.CommuteDatabase;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.AddNpgsqlDbContext<CommutePlannerDbContext>("commute_db",
  settings => settings.ConnectionString = "Host=localhost;db=commute_db");

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseRouting();
app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
