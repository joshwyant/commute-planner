using commute_planner.CommuteDatabase;
using Microsoft.AspNetCore.Mvc;

namespace commute_planner.ApiService;

public class ApiController(CommutePlannerDbContext dbContext) : Controller
{
  protected CommutePlannerDbContext DbContext { get; } = dbContext;
  
  // GET
  public IActionResult RoutePairs()
  {
    return Json(DbContext.MatchingRoutes);
  }
}