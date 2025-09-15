using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Infrastructure.Logging;

public class ApplicationLogger<T> : IApplicationLogger<T>
{
    private readonly ILogger<T> _logger;

    public ApplicationLogger(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        _logger.LogError(exception, message, args);
    }

    public void LogError(string message, params object[] args)
    {
        _logger.LogError(message, args);
    }

    public void LogDebug(string message, params object[] args)
    {
        _logger.LogDebug(message, args);
    }

    public void LogInformation(string message, object properties)
    {
        using (_logger.BeginScope(properties))
        {
            _logger.LogInformation(message);
        }
    }

    public void LogWarning(string message, object properties)
    {
        using (_logger.BeginScope(properties))
        {
            _logger.LogWarning(message);
        }
    }

    public void LogError(Exception exception, string message, object properties)
    {
        using (_logger.BeginScope(properties))
        {
            _logger.LogError(exception, message);
        }
    }

    public IDisposable BeginScope(string scopeName, object? properties = null)
    {
        var scopeData = new { ScopeName = scopeName, Properties = properties };
        return _logger.BeginScope(scopeData) ?? throw new InvalidOperationException("Failed to create logger scope");
    }

    public void LogPerformance(string operation, TimeSpan duration, object? properties = null)
    {
        var perfData = new { Operation = operation, DurationMs = duration.TotalMilliseconds, Properties = properties };

        using (_logger.BeginScope(perfData))
        {
            _logger.LogInformation("Performance: {Operation} completed in {DurationMs}ms", 
                operation, duration.TotalMilliseconds);
        }
    }
}
