using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Infrastructure.Persistence;

public class TaskDbContext : DbContext, ITaskDbContext
{
	public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
	{
	}

	public DbSet<User> Users { get; set; } = null!;
	public DbSet<UserTask> Tasks { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Only seed in development environment - check environment variable
		if (ShouldSeedData())
		{
			var seeder = new DataSeeder(modelBuilder);
			seeder.SeedUsers();
			seeder.SeedTasks();
		}

		// Configure relationships
		modelBuilder.Entity<UserTask>()
			.HasOne<User>()
			.WithMany(u => u.Tasks)
			.HasForeignKey(t => t.UserId)
			.IsRequired();

		// Configure indexes for performance
		// Primary index for filtering tasks by user - most common query pattern
		modelBuilder.Entity<UserTask>()
			.HasIndex(t => t.UserId);
	}

	private static bool ShouldSeedData()
	{
		// Don't seed during tests - detect by checking if we're in a test context
		var stackTrace = Environment.StackTrace;
		if (stackTrace.Contains("Test", StringComparison.OrdinalIgnoreCase) ||
		    stackTrace.Contains("xunit", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		// Only seed in Development environment
		var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
		return string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);
	}
}
