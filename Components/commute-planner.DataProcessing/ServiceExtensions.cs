using commute_planner.EventCollaboration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace commute_planner.DataProcessing;

public static class ServiceExtensions
{
  public static IHostApplicationBuilder AddDataProcessingServices(this IHostApplicationBuilder builder, string? connectionStringName = null)
  {
    builder.Services.AddHostedService<DataProcessingService>();
    builder.AddEventCollaborationServices<DataProcessingExchange>(connectionStringName);
    return builder;
  }
}