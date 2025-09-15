using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Core.Commands;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Handlers;
using TaskManagement.Core.Interfaces;
using TaskManagement.Core.Queries;
using TaskManagement.Core.Validation;

namespace TaskManagement.Core.Extensions;

/// <summary>
/// Extension methods for registering Core layer services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Core layer services including handlers and validators
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        // Register Command Handlers
        services.AddScoped<ICommandHandler<CreateTaskCommand, CreateTaskResult>, CreateTaskHandler>();
        services.AddScoped<ICommandHandler<UpdateTaskCommand, UpdateTaskResult>, UpdateTaskHandler>();
        services.AddScoped<ICommandHandler<ToggleTaskCompletionCommand, ToggleTaskCompletionResult>, ToggleTaskCompletionHandler>();
        services.AddScoped<ICommandHandler<DeleteTaskCommand, DeleteTaskResult>, DeleteTaskHandler>();

        // Register Query Handlers
        services.AddScoped<IQueryHandler<GetTasksQuery, GetTasksResult>, GetTasksHandler>();
        services.AddScoped<IQueryHandler<GetTaskStatisticsQuery, GetTaskStatisticsResult>, GetTaskStatisticsHandler>();
        services.AddScoped<IQueryHandler<GetUsersQuery, GetUsersResult>, GetUsersHandler>();

        // Register Validators
        services.AddScoped<IValidator<UserTask>, TaskValidator>();

        return services;
    }
}
