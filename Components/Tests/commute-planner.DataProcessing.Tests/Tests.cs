namespace commute_planner.DataProcessing;

public class Tests
{
  [SetUp]
  public void Setup()
  {
  }

  [Test]
  public async Task TestPlanner()
  {
    var planner = new Planner();

    var routeId = 1;
    var route = await planner.PlanRouteAsync(routeId);

    Assert.That(route, Is.Not.Null);
    Assert.That(route.Created,
      Is.GreaterThanOrEqualTo(DateTime.Now.AddMinutes(-5)));
  }
}