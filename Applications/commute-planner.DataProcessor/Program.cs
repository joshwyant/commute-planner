using commute_planner.CommuteDatabase;
using commute_planner.DataProcessing;
using RabbitMQ.Client;
using RabbitMQ.Util;

var builder = Host.CreateApplicationBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<CommutePlannerDbContext>("commutedb");

// Add a hosted service
builder.AddDataProcessingServices("messaging");  // This app

var app = builder.Build();

// When running hosted services
await app.RunAsync();
