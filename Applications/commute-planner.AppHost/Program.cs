var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder
  .AddProject<Projects.commute_planner_ApiService>("apiservice");

var datafetcher =
  builder.AddProject<Projects.commute_planner_DataFetcher>("datafetcher");

var dataprocessor =
  builder.AddProject<Projects.commute_planner_DataProcessor>("dataprocessor");

builder.AddProject<Projects.commute_planner_Web>("webfrontend");
builder.AddProject<Projects.commute_planner_FakeMapsServer>("maps");
builder.AddProject<Projects.commute_planner_FakeTransitServer>("transit");

// Pass along API environment variables
foreach (var name in new[]
         {
           "GOOGLE_BASE_URL", "GOOGLE_API_KEY", "TRANSIT_BASE_URL",
           "TRANSIT_API_KEY"
         })
{
  var val = Environment.GetEnvironmentVariable(name);
  if (val != null) datafetcher.WithEnvironment(name, val);
}

if (true)
{
  // Start and link some containers.
  var postgresdb = builder.AddPostgres("pg")
                          .AddDatabase("commutedb");

  var messaging = builder.AddRabbitMQ("messaging");

  apiService.WithReference(postgresdb);
  datafetcher.WithReference(messaging);
  dataprocessor
    .WithReference(postgresdb)
    .WithReference(messaging);
}

builder.Build().Run();
