using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace commute_planner.EventCollaboration;

public class EventCollaborationConfigurationOptions
{
  public Uri? Uri;
}

public static class ServiceExtensions
{
  public static IHostApplicationBuilder AddEventCollaborationServices<T>(
    this IHostApplicationBuilder builder, string? connectionStringName = null)
  where T : class, ICommutePlannerExchange
  {
    var connectionString
      = (connectionStringName is null
          ? null
          : builder.Configuration.GetConnectionString(connectionStringName))
        ?? null; //builder.Configuration[""] // TODO: Aspire configuration value
        //?? throw new InvalidOperationException("Connection string not found.");

    return builder.AddEventCollaborationServices<T>(config =>
      new() { Uri = new(connectionString) });
  }
  public static IHostApplicationBuilder AddEventCollaborationServices<T>(
    this IHostApplicationBuilder builder, Func<EventCollaborationConfigurationOptions, EventCollaborationConfigurationOptions>? config)
  where T : class, ICommutePlannerExchange
  {
    // Register ICommutePlannerExchange as a singleton
    builder.Services.AddSingleton<ICommutePlannerExchange, T>();
    builder.Services.AddSingleton<T>();

    // This is how we'll inject RabbitMQ instead of Aspire.
    // builder.Services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory
    //   { Uri = config?.Invoke(new()).Uri });

    // Register EventCollaborationService, assuming it depends on CommutePlannerExchange
    builder.Services.AddHostedService<EventCollaborationService>();

    return builder;
  }
}
