using System.ComponentModel.DataAnnotations;

namespace commute_planner.CommuteDatabase.Models;

public class TripData
{
  [Key]
  public int TripDataId { get; init; }

  public DateTime Created { get; init; }
  
  public MatchingRoute Route { get; init; }
  public int MatchingRouteId { get; init; }
  
  public int DrivingTimeInSeconds { get; init; }
  
  public int TransitTimeInSeconds { get; init; }
}
