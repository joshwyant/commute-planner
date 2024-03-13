namespace commute_planner.Planning;

public class Planner
{
  public PlannerRoute PlanRoute(int routeId)
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
