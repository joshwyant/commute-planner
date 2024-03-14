using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace commute_planner.CommuteDatabase.Models;

public class DrivingRoute
{
  [Key]
  public int DrivingRouteId { get; init; }
  
  [StringLength(100)]
  public required string Name { get; init; }
  
  [StringLength(1000)]
  public required string Description { get; init; }
  
  [StringLength(300)]
  public required string FromAddress { get; init; }
  
  [StringLength(300)]
  public required string ToAddress { get; init; }
}