using commute_planner.EventCollaboration;
using Microsoft.Extensions.DependencyInjection;

namespace commute_planner.DataProcessing;

public static class ServiceExtensions
{
  public static IServiceCollection AddDataProcessingServices(this IServiceCollection services)
  {
    services.AddHostedService<DataProcessingService>();
    services.AddEventCollaborationServices<DataProcessingExchange>();
    return services;
  }
}