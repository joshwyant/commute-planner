using commute_planner.CommuteDatabase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace commute_planner.ApiService;

[ApiController]
[Route("[controller]")]
public class ApiController(CommutePlannerDbContext dbContext) : Controller
{
  protected CommutePlannerDbContext DbContext { get; } = dbContext;
  
  [HttpGet("RoutePairs")]
  public async Task<IActionResult> RoutePairsAsync()
  {
    return Json(DbContext.MatchingRoutes
      .Include(r => r.DrivingRoute)
      .Include(r => r.TransitRoute));
  }

  public async Task<IActionResult> PlanRoute(int routeId)
  {
    return Problem("Not implemented");
  }
  
}