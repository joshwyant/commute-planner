using commute_planner.EventCollaboration;
using Microsoft.Extensions.DependencyInjection;

namespace commute_planner.DataCollection;

public static class ServiceExtensions
{
  public static IServiceCollection AddDataCollectionServices(this IServiceCollection services)
  {
    services.AddHostedService<DataCollectionService>();
    services.AddEventCollaborationServices<DataCollectionExchange>();
    return services;
  }
}