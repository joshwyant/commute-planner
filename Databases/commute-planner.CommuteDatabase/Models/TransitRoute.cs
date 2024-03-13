using System.ComponentModel.DataAnnotations;

namespace commute_planner.CommuteDatabase.Models;

public class TransitRoute
{
  [Key]
  public int TransitRouteId { get; init; }
  
  [StringLength(100)]
  public required string Name { get; init; }
  
  [StringLength(1000)]
  public required string Description { get; init; }
  
  [StringLength(8)]
  public required string OperatorId { get; init; }
  
  [StringLength(12)]
  public required string LineId { get; init; }
  
  [StringLength(12)]
  public required string FromStopId { get; init; }
  
  [StringLength(12)]
  public required string ToStopId { get; init; }
}
