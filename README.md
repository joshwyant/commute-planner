Class project

To run with Docker:

1. `docker-compose up -d`
2. visit http://localhost:5015

To run using Aspire, requires .net 8.0 (mac, linux, windows) and docker:

1. `% dotnet workload restore`
2. `% dotnet build`
3. `% dotnet test`
4. `% dotnet run --project Applications/commute-planner.AppHost`

CI/CD deployment:

https://joshcolorado.azurewebsites.net

(https://github.com/joshwyant/commute-planner)
