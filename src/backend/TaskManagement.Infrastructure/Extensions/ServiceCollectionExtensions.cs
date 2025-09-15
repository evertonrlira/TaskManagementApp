using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Common;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering Infrastructure layer services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Infrastructure layer services including database and repositories
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Add DbContext - using InMemory database for development
        services.AddDbContext<TaskDbContext>(options =>
            options.UseInMemoryDatabase("TasksDb"));

        // Register Infrastructure implementations
        services.AddScoped<ITaskDbContext>(provider => provider.GetRequiredService<TaskDbContext>());
        services.AddScoped<IPaginationService, PaginationService>();

        return services;
    }

    /// <summary>
    /// Ensures the database is created and seeded with initial data
    /// </summary>
    /// <param name="services">The service provider</param>
    public static void EnsureDatabaseCreated(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
        context.Database.EnsureCreated();
    }
}
