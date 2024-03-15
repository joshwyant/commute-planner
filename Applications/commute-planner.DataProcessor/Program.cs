using commute_planner.CommuteDatabase;
using commute_planner.DataProcessing;

var builder = Host.CreateApplicationBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.AddRabbitMQ("messaging");
builder.AddNpgsqlDbContext<CommutePlannerDbContext>("commute_db");

// Add a hosted service
builder.Services.AddDataProcessingServices();  // This app

var app = builder.Build();

await app.Services.SetupCommuteDatabaseAsync<Program>();

// When running hosted services
await app.RunAsync();
