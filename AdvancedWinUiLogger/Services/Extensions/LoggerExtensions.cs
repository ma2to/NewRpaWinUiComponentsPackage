using Microsoft.Extensions.Logging;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Services.Extensions;

/// <summary>
/// 🔧 EXTENSION METHODS: Convenient logging extensions with null safety
/// FUNCTIONAL: Pure extension methods for enhanced logging experience
/// CONSISTENT: Unified logging patterns across the application
/// </summary>
public static class LoggerExtensions
{
    #region Information Logging

    /// <summary>
    /// FUNCTIONAL: Logs informational message with null safety
    /// USAGE: logger?.Info("🔧 Operation started with {Count} items", count)
    /// </summary>
    public static void Info(this ILogger? logger, string message, params object[] args)
    {
        logger?.LogInformation(message, args);
    }

    /// <summary>
    /// FUNCTIONAL: Logs informational message with structured data
    /// </summary>
    public static void InfoStructured<T>(this ILogger? logger, string message, T data)
    {
        logger?.LogInformation(message + " {Data}", data);
    }

    #endregion

    #region Warning Logging

    /// <summary>
    /// FUNCTIONAL: Logs warning message with null safety
    /// USAGE: logger?.Warning("⚠️ Performance threshold exceeded: {Value}ms", duration)
    /// </summary>
    public static void Warning(this ILogger? logger, string message, params object[] args)
    {
        logger?.LogWarning(message, args);
    }

    /// <summary>
    /// FUNCTIONAL: Logs warning with exception
    /// </summary>
    public static void Warning(this ILogger? logger, Exception exception, string message, params object[] args)
    {
        logger?.LogWarning(exception, message, args);
    }

    #endregion

    #region Error Logging

    /// <summary>
    /// FUNCTIONAL: Logs error message with null safety
    /// USAGE: logger?.Error("❌ Operation failed: {Error}", errorMessage)
    /// </summary>
    public static void Error(this ILogger? logger, string message, params object[] args)
    {
        logger?.LogError(message, args);
    }

    /// <summary>
    /// FUNCTIONAL: Logs error message with exception and null safety
    /// USAGE: logger?.Error(exception, "🚨 Critical error in {Operation}", operationName)
    /// </summary>
    public static void Error(this ILogger? logger, Exception exception, string message, params object[] args)
    {
        logger?.LogError(exception, message, args);
    }

    #endregion

    #region Debug Logging

    /// <summary>
    /// FUNCTIONAL: Logs debug message (only in debug builds by default)
    /// </summary>
    public static void Debug(this ILogger? logger, string message, params object[] args)
    {
        logger?.LogDebug(message, args);
    }

    /// <summary>
    /// FUNCTIONAL: Logs debug message with structured data
    /// </summary>
    public static void DebugStructured<T>(this ILogger? logger, string message, T data)
    {
        logger?.LogDebug(message + " {Data}", data);
    }

    #endregion

    #region Trace Logging

    /// <summary>
    /// FUNCTIONAL: Logs trace message for detailed diagnostics
    /// </summary>
    public static void Trace(this ILogger? logger, string message, params object[] args)
    {
        logger?.LogTrace(message, args);
    }

    /// <summary>
    /// FUNCTIONAL: Logs method entry for tracing
    /// </summary>
    public static void TraceMethodEntry(this ILogger? logger, string methodName, params object[] parameters)
    {
        logger?.LogTrace("→ Entering {MethodName} with parameters: {Parameters}", methodName, parameters);
    }

    /// <summary>
    /// FUNCTIONAL: Logs method exit for tracing
    /// </summary>
    public static void TraceMethodExit(this ILogger? logger, string methodName, object? result = null)
    {
        if (result != null)
        {
            logger?.LogTrace("← Exiting {MethodName} with result: {Result}", methodName, result);
        }
        else
        {
            logger?.LogTrace("← Exiting {MethodName}", methodName);
        }
    }

    #endregion

    #region Critical Logging

    /// <summary>
    /// FUNCTIONAL: Logs critical error that requires immediate attention
    /// </summary>
    public static void Critical(this ILogger? logger, string message, params object[] args)
    {
        logger?.LogCritical(message, args);
    }

    /// <summary>
    /// FUNCTIONAL: Logs critical error with exception
    /// </summary>
    public static void Critical(this ILogger? logger, Exception exception, string message, params object[] args)
    {
        logger?.LogCritical(exception, message, args);
    }

    #endregion

    #region Performance Logging

    /// <summary>
    /// FUNCTIONAL: Logs performance metrics
    /// </summary>
    public static void Performance(this ILogger? logger, string operation, TimeSpan duration, params object[] additionalData)
    {
        logger?.LogInformation("⏱️ Performance: {Operation} completed in {Duration}ms {AdditionalData}",
            operation, duration.TotalMilliseconds, additionalData);
    }

    /// <summary>
    /// FUNCTIONAL: Logs performance warning when threshold exceeded
    /// </summary>
    public static void PerformanceWarning(this ILogger? logger, string operation, TimeSpan duration, TimeSpan threshold)
    {
        logger?.LogWarning("⚠️ Performance Warning: {Operation} took {Duration}ms (threshold: {Threshold}ms)",
            operation, duration.TotalMilliseconds, threshold.TotalMilliseconds);
    }

    #endregion

    #region Structured Logging Helpers

    /// <summary>
    /// FUNCTIONAL: Logs with custom event ID
    /// </summary>
    public static void LogWithEventId(this ILogger? logger, LogLevel level, int eventId, string message, params object[] args)
    {
        logger?.Log(level, new EventId(eventId), message, args);
    }

    /// <summary>
    /// FUNCTIONAL: Logs operation start/end with automatic timing
    /// </summary>
    public static IDisposable? LogOperation(this ILogger? logger, string operationName)
    {
        return logger != null ? new LoggedOperation(logger, operationName) : null;
    }

    #endregion

    #region Conditional Logging

    /// <summary>
    /// FUNCTIONAL: Logs only if condition is true
    /// </summary>
    public static void LogIf(this ILogger? logger, bool condition, LogLevel level, string message, params object[] args)
    {
        if (condition)
        {
            logger?.Log(level, message, args);
        }
    }

    /// <summary>
    /// FUNCTIONAL: Logs only if logger is enabled for the level
    /// </summary>
    public static void LogIfEnabled(this ILogger? logger, LogLevel level, Func<string> messageFactory)
    {
        if (logger?.IsEnabled(level) == true)
        {
            logger.Log(level, messageFactory());
        }
    }

    #endregion
}

#region Helper Classes

/// <summary>
/// INTERNAL: Helper class for automatic operation timing
/// </summary>
internal sealed class LoggedOperation : IDisposable
{
    private readonly ILogger _logger;
    private readonly string _operationName;
    private readonly DateTime _startTime;
    private bool _disposed;

    public LoggedOperation(ILogger logger, string operationName)
    {
        _logger = logger;
        _operationName = operationName;
        _startTime = DateTime.Now;

        _logger.Info("🚀 Starting operation: {OperationName}", _operationName);
    }

    public void Dispose()
    {
        if (_disposed) return;

        var duration = DateTime.Now - _startTime;
        _logger.Performance(_operationName, duration);
        _disposed = true;
    }
}

#endregion