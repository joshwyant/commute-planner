var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.commute_planner_ApiService>("apiservice");

builder.AddProject<Projects.commute_planner_Web>("webfrontend")
    .WithReference(apiService);

builder.Build().Run();
