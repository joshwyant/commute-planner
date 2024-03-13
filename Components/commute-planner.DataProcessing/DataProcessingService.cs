using commute_planner.CommuteDatabase;
using Microsoft.Extensions.Hosting;

namespace commute_planner.DataProcessing;

public class DataProcessingService(CommutePlannerDbContext db) : IHostedService
{
  public Task StartAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
}