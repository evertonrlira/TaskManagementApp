using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Core.Common;

/// <summary>
/// Base class for command handlers with common functionality
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
/// <typeparam name="TResult">The result type</typeparam>
public abstract class BaseCommandHandler<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : class, ICommand
{
    protected readonly ILogger<BaseCommandHandler<TCommand, TResult>> Logger;

    protected BaseCommandHandler(ILogger<BaseCommandHandler<TCommand, TResult>> logger)
    {
        Logger = logger;
    }

    public async Task<TResult> HandleAsync(TCommand command)
    {
        var commandType = typeof(TCommand).Name;
        var stopwatch = Stopwatch.StartNew();
        
        Logger.LogInformation("Executing command {CommandType}", commandType);

        try
        {
            var result = await ExecuteAsync(command);
            
            stopwatch.Stop();
            Logger.LogInformation("Command {CommandType} completed in {Duration}ms", 
                commandType, stopwatch.ElapsedMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.LogError(ex, "Failed to execute command {CommandType} after {Duration}ms", 
                commandType, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Execute the specific command logic
    /// </summary>
    protected abstract Task<TResult> ExecuteAsync(TCommand command);
}

/// <summary>
/// Base class for query handlers with common functionality
/// </summary>
/// <typeparam name="TQuery">The query type</typeparam>
/// <typeparam name="TResult">The result type</typeparam>
public abstract class BaseQueryHandler<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    where TQuery : class, IQuery
{
    protected readonly ILogger<BaseQueryHandler<TQuery, TResult>> Logger;

    protected BaseQueryHandler(ILogger<BaseQueryHandler<TQuery, TResult>> logger)
    {
        Logger = logger;
    }

    public async Task<TResult> HandleAsync(TQuery query)
    {
        var queryType = typeof(TQuery).Name;
        var stopwatch = Stopwatch.StartNew();
        
        Logger.LogInformation("Executing query {QueryType}", queryType);

        try
        {
            var result = await ExecuteAsync(query);
            
            stopwatch.Stop();
            Logger.LogInformation("Query {QueryType} completed in {Duration}ms", 
                queryType, stopwatch.ElapsedMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.LogError(ex, "Failed to execute query {QueryType} after {Duration}ms", 
                queryType, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Execute the specific query logic
    /// </summary>
    protected abstract Task<TResult> ExecuteAsync(TQuery query);
}
