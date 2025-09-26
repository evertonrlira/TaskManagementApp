using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces;

/// <summary>
/// Abstraction for the database context to allow Core to depend on interfaces
/// </summary>
public interface ITaskDbContext
{
	DbSet<User> Users { get; }
	DbSet<UserTask> Tasks { get; }
	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
