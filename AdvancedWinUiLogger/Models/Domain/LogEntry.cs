using Microsoft.Extensions.Logging;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Domain;

/// <summary>
/// 📊 DOMAIN ENTITY: Log entry representation
/// IMMUTABLE: Value object with rich domain behavior
/// FUNCTIONAL: Pure functions for transformations
/// </summary>
public sealed record LogEntry
{
    public DateTime Timestamp { get; init; }
    public LogLevel Level { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Exception { get; init; }
    public string? Category { get; init; }
    public string? Source { get; init; }
    public EventId EventId { get; init; }

    public LogEntry() { }

    public LogEntry(DateTime timestamp, LogLevel level, string message, string? exception = null)
    {
        Timestamp = timestamp;
        Level = level;
        Message = message;
        Exception = exception;
        EventId = new EventId(0);
    }

    /// <summary>
    /// FUNCTIONAL: Create log entry from logger parameters
    /// </summary>
    public static LogEntry Create<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        System.Exception? exception,
        Func<TState, System.Exception?, string> formatter) =>
        new()
        {
            Timestamp = DateTime.Now,
            Level = logLevel,
            EventId = eventId,
            Message = formatter(state, exception),
            Exception = exception?.ToString(),
            Category = typeof(TState).Name
        };

    /// <summary>
    /// FUNCTIONAL: Format entry as string for file output
    /// </summary>
    public string ToFileFormat(string dateFormat = "yyyy-MM-dd HH:mm:ss.fff") =>
        $"[{Timestamp.ToString(dateFormat)}] [{Level.ToString().ToUpperInvariant()}] {Message}" +
        (Exception != null ? Environment.NewLine + Exception : "") +
        Environment.NewLine;

    /// <summary>
    /// FUNCTIONAL: Check if entry meets minimum log level
    /// </summary>
    public bool MeetsLevel(LogLevel minimumLevel) => Level >= minimumLevel;

    /// <summary>
    /// FUNCTIONAL: Transform entry with new message
    /// </summary>
    public LogEntry WithMessage(string newMessage) => this with { Message = newMessage };

    /// <summary>
    /// FUNCTIONAL: Transform entry with additional context
    /// </summary>
    public LogEntry WithSource(string source) => this with { Source = source };
}