Class project

To run with Docker:

1. `docker-compose up -d`
2. visit http://localhost:5015

To run using Aspire, also requires docker, and .net 9.0 (all platforms):

1. `% dotnet workload restore` (only once)
2. `% dotnet build`
3. `% dotnet test`
4. `% dotnet run --project Applications/commute-planner.AppHost`

CI/CD deployed to:

https://colorado.heyjosh.io

(https://github.com/joshwyant/commute-planner/actions/workflows/azure-dev.yml)
