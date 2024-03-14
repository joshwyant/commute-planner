using Microsoft.Extensions.DependencyInjection;

namespace commute_planner.EventCollaboration;

public static class ServiceExtensions
{
  public static IServiceCollection AddEventCollaborationServices<T>(
    this IServiceCollection services)
  where T : class, ICommutePlannerExchange
  {
    // Register ICommutePlannerExchange as a singleton
    services.AddSingleton<ICommutePlannerExchange, T>();
    services.AddSingleton<T>();

    // Register EventCollaborationService, assuming it depends on CommutePlannerExchange
    services.AddHostedService<EventCollaborationService>();

    return services;
  }
}
