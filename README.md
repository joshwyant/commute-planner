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


Apps:
- Orchestrated through Docker, Aspire (AppHost), or Azure
- Web App: Home page - select a popular route, see a description and whether transit or driving is faster; Routes - 
  see the featured routes
- DataFetcher app collects data from https://511.org and Google Maps REST APIs, upon a freshness request message.
- DataProcessor app periodically sends message to DataFetcher app, which then sends message with the fresh data recieved. 
  DataProcessor analyzes, combines, and persists the transformed data to be efficiently retrieved.
- ApiService provides a REST API for the Web front-end to consume, acting as an interface to the database.
- AppHost - handles Aspire orchestration and deployment, as well as a dashboard, logs, and graphs. Provides interfaces
  to Azure Insights and OpenTelemetry for production monitoring and instrumenting used by logging throughout.
- RabbitMQ is deployed and connected by the Aspire library as a container for messaging.
- PostgreSQL is deployed and connected by the Aspire library as a container for the data store, which is an ACID
  database, since the data is small, structured, and transactional.

Components:
- ApiClient - used by the Web app as a model and interface to the REST API.
- DataCollection - the logic for how to fetch new third party transportation data, along with communication with the
  data processor app.
- DataProcessing - the logic for how to analyze, combine, and store the transformed data, as well as for signaling to
  the DataFetcher app.
- EventCollaboration - Tools and extensions used to handle the bulk of the message queueing work and interfaces with
  RabbitMQ.
- MapsApiClient - An implementation of the interface to the Routes REST API that Google Maps exposes.
- ServiceDefaults - Configures the orchestration of the distributed app using Aspire.
- TransitApiClient - An implementation of the interface to the part of the 511 Transit REST API that is exposed and used
  by the app.
- TransitApiModels - Models shared by the data fetcher and transit API.

Tests:
- Integrated into the build and deploy process. Uses Nunit for Unit tests, Moq for mocks and spies, and Aspire to
  handle the orchestration for integration testing
- Fake Google Maps server
- Fake Transit API server
- Separate folders

Database
- Uses Entity Framework for the Object Relational Mapping, Aspire for orchestration, and PostgreSQL for persistence.

Continuous integration/Continuous Delivery
- On each commit to main, triggers GitHub Actions (.github/) to prepare .NET environment, install build tools, build, test, and deploy to Azure as
  a cohesive Web App composed of containerized apps and services. Uses Azure
  Secrets to store real API keys, production passwords, etc.
