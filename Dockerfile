# Use the SDK image for building the solution
FROM mcr.microsoft.com/dotnet/sdk:9.0.100-preview.2-bookworm-slim-arm64v8 AS build
WORKDIR /src
RUN dotnet workload install aspire
COPY ["commute-planner.sln", "."]
COPY ["Applications/commute-planner.ApiService/commute-planner.ApiService.csproj", "Applications/commute-planner.ApiService/"]
COPY ["Applications/commute-planner.AppHost/commute-planner.AppHost.csproj", "Applications/commute-planner.AppHost/"]
COPY ["Applications/commute-planner.DataFetcher/commute-planner.DataFetcher.csproj", "Applications/commute-planner.DataFetcher/"]
COPY ["Applications/commute-planner.DataProcessor/commute-planner.DataProcessor.csproj", "Applications/commute-planner.DataProcessor/"]
COPY ["Applications/commute-planner.Web/commute-planner.Web.csproj", "Applications/commute-planner.Web/"]
COPY ["Components/commute-planner.ApiClient/commute-planner.ApiClient.csproj", "Components/commute-planner.ApiClient/"]
COPY ["Components/commute-planner.DataCollection/commute-planner.DataCollection.csproj", "Components/commute-planner.DataCollection/"]
COPY ["Components/commute-planner.DataProcessing/commute-planner.DataProcessing.csproj", "Components/commute-planner.DataProcessing/"]
COPY ["Components/commute-planner.EventCollaboration/commute-planner.EventCollaboration.csproj", "Components/commute-planner.EventCollaboration/"]
COPY ["Components/commute-planner.MapsApiClient/commute-planner.MapsApiClient.csproj", "Components/commute-planner.MapsApiClient/"]
COPY ["Components/commute-planner.ServiceDefaults/commute-planner.ServiceDefaults.csproj", "Components/commute-planner.ServiceDefaults/"]
COPY ["Components/commute-planner.TransitApiClient/commute-planner.TransitApiClient.csproj", "Components/commute-planner.TransitApiClient/"]
COPY ["Components/commute-planner.TransitApiModels/commute-planner.TransitApiModels.csproj", "Components/commute-planner.TransitApiModels/"]
COPY ["Components/Tests/commute-planner.DataCollection.Tests/commute-planner.DataCollection.Tests.csproj", "Components/Tests/commute-planner.DataCollection.Tests/"]
COPY ["Components/Tests/commute-planner.DataProcessing.Tests/commute-planner.DataProcessing.Tests.csproj", "Components/Tests/commute-planner.DataProcessing.Tests/"]
COPY ["Components/Tests/commute-planner.MapsApiClient.Tests/commute-planner.MapsApiClient.Tests.csproj", "Components/Tests/commute-planner.MapsApiClient.Tests/"]
COPY ["Components/Tests/commute-planner.TransitApiClient.Tests/commute-planner.TransitApiClient.Tests.csproj", "Components/Tests/commute-planner.TransitApiClient.Tests/"]
COPY ["Databases/commute-planner.CommuteDatabase/commute-planner.CommuteDatabase.csproj", "Databases/commute-planner.CommuteDatabase/"]
COPY ["TestSupport/commute-planner.FakeMapsServer/commute-planner.FakeMapsServer.csproj", "TestSupport/commute-planner.FakeMapsServer/"]
COPY ["TestSupport/commute-planner.FakeTransitServer/commute-planner.FakeTransitServer.csproj", "TestSupport/commute-planner.FakeTransitServer/"]
COPY ["TestSupport/commute-planner.IntegrationTests/commute-planner.IntegrationTests.csproj", "TestSupport/commute-planner.IntegrationTests/"]
RUN dotnet restore

COPY . .
RUN dotnet build "commute-planner.sln" -c ${BUILD_CONFIGURATION:-Debug} -o /app/build
RUN dotnet test

# Publish ApiService
FROM build AS publish-apiservice
RUN dotnet publish "Applications/commute-planner.ApiService/commute-planner.ApiService.csproj" -c ${BUILD_CONFIGURATION:-Debug} -o /app/publish/apiservice

# Publish AppHost
FROM build AS publish-apphost
RUN dotnet publish "Applications/commute-planner.AppHost/commute-planner.AppHost.csproj" -c ${BUILD_CONFIGURATION:-Debug} -o /app/publish/apphost

# Publish Web
FROM build AS publish-web
RUN dotnet publish "Applications/commute-planner.Web/commute-planner.Web.csproj" -c ${BUILD_CONFIGURATION:-Debug} -o /app/publish/web

# Publish DataFetcher
FROM build AS publish-datafetcher
RUN dotnet publish "Applications/commute-planner.DataFetcher/commute-planner.DataFetcher.csproj" -c ${BUILD_CONFIGURATION:-Debug} -o /app/publish/datafetcher

# Publish DataProcessor
FROM build AS publish-dataprocessor
RUN dotnet publish "Applications/commute-planner.DataProcessor/commute-planner.DataProcessor.csproj" -c ${BUILD_CONFIGURATION:-Debug} -o /app/publish/dataprocessor

# Publish FakeMapsServer
FROM build AS publish-maps
RUN dotnet publish "TestSupport/commute-planner.FakeMapsServer/commute-planner.FakeMapsServer.csproj" -c ${BUILD_CONFIGURATION:-Debug} -o /app/publish/maps

# Publish FakeTransitServer
FROM build AS publish-transit
RUN dotnet publish "TestSupport/commute-planner.FakeTransitServer/commute-planner.FakeTransitServer.csproj" -c ${BUILD_CONFIGURATION:-Debug} -o /app/publish/transit

# Final stage/image for Web
FROM mcr.microsoft.com/dotnet/sdk:9.0.100-preview.2-bookworm-slim-arm64v8 AS final-web
WORKDIR /app
COPY --from=publish-web /app/publish/web .
ENTRYPOINT ["dotnet", "commute-planner.Web.dll"]

# Final stage/image for ApiService
FROM mcr.microsoft.com/dotnet/sdk:9.0.100-preview.2-bookworm-slim-arm64v8 AS final-apiservice
WORKDIR /app
COPY --from=publish-apiservice /app/publish/apiservice .
ENTRYPOINT ["dotnet", "commute-planner.ApiService.dll"]

# Final stage/image for DataFetcher
FROM mcr.microsoft.com/dotnet/sdk:9.0.100-preview.2-bookworm-slim-arm64v8 AS final-datafetcher
WORKDIR /app
COPY --from=publish-datafetcher /app/publish/datafetcher .
ENTRYPOINT ["dotnet", "commute-planner.DataFetcher.dll"]

# Final stage/image for DataProcessor
FROM mcr.microsoft.com/dotnet/sdk:9.0.100-preview.2-bookworm-slim-arm64v8 AS final-dataprocessor
WORKDIR /app
COPY --from=publish-dataprocessor /app/publish/dataprocessor .
ENTRYPOINT ["dotnet", "commute-planner.DataProcessor.dll"]

# Final stage/image for FakeMapsServer
FROM mcr.microsoft.com/dotnet/sdk:9.0.100-preview.2-bookworm-slim-arm64v8 AS final-maps
WORKDIR /app
COPY --from=publish-maps /app/publish/maps .
ENTRYPOINT ["dotnet", "commute-planner.FakeMapsServer.dll"]

# Final stage/image for FakeTransitServer
FROM mcr.microsoft.com/dotnet/sdk:9.0.100-preview.2-bookworm-slim-arm64v8 AS final-transit
WORKDIR /app
COPY --from=publish-transit /app/publish/transit .
ENTRYPOINT ["dotnet", "commute-planner.FakeTransitServer.dll"]
