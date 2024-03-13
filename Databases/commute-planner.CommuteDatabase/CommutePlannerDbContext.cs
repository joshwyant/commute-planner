using System.ComponentModel.DataAnnotations;
using commute_planner.CommuteDatabase.Models;
using Microsoft.EntityFrameworkCore;

namespace commute_planner.CommuteDatabase;

public class CommutePlannerDbContext(DbContextOptions options)
  : DbContext(options)
{
  // ReSharper disable PropertyCanBeMadeInitOnly.Global
  public DbSet<DrivingRoute> DrivingRoutes { get; set; } = null!;
  public DbSet<TransitRoute> TransitRoutes { get; set; } = null!;
  public DbSet<MatchingRoute> MatchingRoutes { get; set; } = null!;
  public DbSet<TripData> TripData { get; set; } = null!;
  // ReSharper restore PropertyCanBeMadeInitOnly.Global
}
