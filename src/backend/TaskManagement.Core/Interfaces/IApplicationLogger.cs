namespace TaskManagement.Core.Interfaces;

public interface IApplicationLogger<T>
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(Exception exception, string message, params object[] args);
    void LogError(string message, params object[] args);
    void LogDebug(string message, params object[] args);
    
    // Structured logging with properties
    void LogInformation(string message, object properties);
    void LogWarning(string message, object properties);
    void LogError(Exception exception, string message, object properties);
    
    // Performance tracking
    IDisposable BeginScope(string scopeName, object? properties = null);
    void LogPerformance(string operation, TimeSpan duration, object? properties = null);
}
