using commute_planner.DataCollection;
using commute_planner.MapsApi;
using commute_planner.TransitApi;

var builder = Host.CreateApplicationBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

var googleBaseUrl = builder.Configuration["GOOGLE_BASE_URL"];
var googleApiKey = builder.Configuration["GOOGLE_API_KEY"];

var transitBaseUrl = builder.Configuration["TRANSIT_BASE_URL"];
var transitApiKey = builder.Configuration["TRANSIT_API_KEY"];

// Add HTTP client configurations for our Maps and Transit APIs
builder.Services.AddMapsApiHttpClient(googleBaseUrl, googleApiKey);
builder.Services.AddTransitApiHttpClient(transitBaseUrl, transitApiKey);
// Add a console logger.
// Add our hosted services
builder.AddDataCollectionServices("messaging");  // This app

var app = builder.Build();

// When running hosted services
await app.RunAsync();
