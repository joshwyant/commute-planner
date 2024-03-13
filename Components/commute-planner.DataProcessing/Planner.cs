namespace commute_planner.DataProcessing;

public class Planner
{
  public async Task<PlannerRoute> PlanRouteAsync(int routeId)
  {
    return new PlannerRoute()
    {
      Created = DateTime.UnixEpoch
    };
  }
}

public class PlannerRoute
{
  public DateTime Created { get; set; }
}
