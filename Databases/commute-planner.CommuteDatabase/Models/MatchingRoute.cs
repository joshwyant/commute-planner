using System.ComponentModel.DataAnnotations;

namespace commute_planner.CommuteDatabase.Models;

public class MatchingRoute
{
  [Key]
  public int MatchingRouteId { get; init; }
  
  [StringLength(100)]
  public required string Name { get; init; }
  
  public required DrivingRoute DrivingRoute { get; init; }
  public int DrivingRouteId { get; init; }
  public required TransitRoute TransitRoute { get; init; }
  public int TransitRouteId { get; init; }
}
