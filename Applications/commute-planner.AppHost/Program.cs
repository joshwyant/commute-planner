var builder = DistributedApplication.CreateBuilder(args);

var postgresdb = builder.AddPostgres("pg")
                        .AddDatabase("commute_db");

var messaging = builder.AddRabbitMQ("messaging");

var apiService = builder
    .AddProject<Projects.commute_planner_ApiService>("apiservice")
    .WithReference(postgresdb);

builder.AddProject<Projects.commute_planner_Web>("webfrontend")
    .WithReference(apiService);

builder.AddProject<Projects.commute_planner_DataFetcher>("datafetcher")
  .WithReference(messaging);

builder.Build().Run();
