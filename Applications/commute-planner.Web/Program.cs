using commute_planner.MapsApi;
using commute_planner.Web;
using commute_planner.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddHttpClient<WeatherApiClient>(client => client.BaseAddress = new("http://apiservice"));
builder.Services.AddHttpClient<MapsApiClient>(client =>
{
    client.BaseAddress =
        new Uri("https://routes.googleapis.com/directions");

    client.DefaultRequestHeaders.Add("X-Goog-Api-Key",
        Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ??
        throw new InvalidOperationException("Missing API key."));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();

app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
