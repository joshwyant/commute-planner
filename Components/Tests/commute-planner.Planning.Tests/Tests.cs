namespace commute_planner.Planning.Tests;

public class Tests
{
  [SetUp]
  public void Setup()
  {
  }

  [Test]
  public void TestPlanner()
  {
    var planner = new Planner();

    var routeId = 1;
    var route = planner.PlanRoute(routeId);

    Assert.That(route, Is.Not.Null);
    Assert.That(route.Created,
      Is.GreaterThanOrEqualTo(DateTime.Now.AddMinutes(-5)));
  }
}