using commute_planner.EventCollaboration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace commute_planner.DataCollection;

public static class ServiceExtensions
{
  public static IHostApplicationBuilder AddDataCollectionServices(this IHostApplicationBuilder builder, string? connectionStringName = null)
  {
    builder.Services.AddHostedService<DataCollectionService>();
    builder.AddEventCollaborationServices<DataCollectionExchange>(connectionStringName);
    return builder;
  }
}