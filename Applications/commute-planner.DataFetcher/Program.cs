﻿using commute_planner.DataCollection;
using commute_planner.MapsApi;
using commute_planner.TransitApi;

var builder = Host.CreateApplicationBuilder(args);

var googleBaseUrl = builder.Configuration["GOOGLE_BASE_URL"];
var googleApiKey = builder.Configuration["GOOGLE_API_KEY"];

var transitBaseUrl = builder.Configuration["TRANSIT_BASE_URL"];
var transitApiKey = builder.Configuration["TRANSIT_API_KEY"];

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.AddRabbitMQ("messaging");

// Add HTTP client configurations for our Maps and Transit APIs
builder.Services.AddMapsApiHttpClient(googleBaseUrl, googleApiKey);
builder.Services.AddTransitApiHttpClient(transitBaseUrl, transitApiKey);
// Add a console logger.
builder.Services.AddLogging(configure => configure.AddConsole());
// Add our hosted services
builder.Services.AddDataCollectionServices();  // This app

var app = builder.Build();

// When running hosted services
await app.RunAsync();
