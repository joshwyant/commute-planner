using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace commute_planner.CommuteDatabase;

public class CommutePlannerDbContext(DbContextOptions options)
  : DbContext(options)
{
  public DbSet<DrivingRoute> DrivingRoutes { get; set; } = null!;
  public DbSet<TransitRoute> TransitRoutes { get; set; } = null!;
  public DbSet<MatchingRoute> MatchingRoutes { get; set; } = null!;
}

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

public class MatchingRoute
{
  [Key]
  public int MatchingRouteId { get; init; }
  
  [StringLength(100)]
  public required string Name { get; init; }
  
  public required DrivingRoute DrivingRoute { get; init; }
  public required TransitRoute TransitRoute { get; init; }
}