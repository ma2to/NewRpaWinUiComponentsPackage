using Microsoft.Extensions.Logging;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Configuration;

/// <summary>
/// 🔧 PUBLIC CONFIGURATION: Clean API options for logger setup
/// IMMUTABLE: Record type for configuration safety
/// FUNCTIONAL: Factory methods for common scenarios
/// </summary>
public sealed record LoggerOptions
{
    /// <summary>Base directory for log files</summary>
    public required string LogDirectory { get; init; }

    /// <summary>Base file name for logs</summary>
    public required string BaseFileName { get; init; }

    /// <summary>Maximum log file size in bytes</summary>
    public long MaxFileSizeBytes { get; init; } = 100 * 1024 * 1024; // 100 MB

    /// <summary>Maximum number of log files to keep</summary>
    public int MaxFileCount { get; init; } = 10;

    /// <summary>Enable automatic file rotation</summary>
    public bool EnableAutoRotation { get; init; } = true;

    /// <summary>Enable background logging</summary>
    public bool EnableBackgroundLogging { get; init; } = true;

    /// <summary>Enable performance monitoring</summary>
    public bool EnablePerformanceMonitoring { get; init; } = false;

    /// <summary>Date format for log entries</summary>
    public string DateFormat { get; init; } = "yyyy-MM-dd HH:mm:ss.fff";

    /// <summary>Minimum log level to write</summary>
    public LogLevel MinLogLevel { get; init; } = LogLevel.Information;

    /// <summary>
    /// FUNCTIONAL: Default factory method
    /// </summary>
    public static LoggerOptions Create(string logDirectory, string baseFileName) => new()
    {
        LogDirectory = logDirectory,
        BaseFileName = baseFileName
    };

    /// <summary>
    /// FUNCTIONAL: Debug configuration factory
    /// </summary>
    public static LoggerOptions Debug(string logDirectory, string baseFileName) => new()
    {
        LogDirectory = logDirectory,
        BaseFileName = baseFileName,
        EnablePerformanceMonitoring = true,
        MaxFileSizeBytes = 50 * 1024 * 1024, // 50 MB
        MinLogLevel = LogLevel.Debug
    };

    /// <summary>
    /// FUNCTIONAL: Production configuration factory
    /// </summary>
    public static LoggerOptions Production(string logDirectory, string baseFileName) => new()
    {
        LogDirectory = logDirectory,
        BaseFileName = baseFileName,
        EnablePerformanceMonitoring = false,
        MaxFileSizeBytes = 200 * 1024 * 1024, // 200 MB
        MaxFileCount = 30,
        MinLogLevel = LogLevel.Information
    };

    /// <summary>
    /// FUNCTIONAL: High performance configuration factory
    /// </summary>
    public static LoggerOptions HighPerformance(string logDirectory, string baseFileName) => new()
    {
        LogDirectory = logDirectory,
        BaseFileName = baseFileName,
        EnableBackgroundLogging = true,
        MaxFileSizeBytes = 500 * 1024 * 1024, // 500 MB
        MaxFileCount = 50,
        MinLogLevel = LogLevel.Warning
    };

    /// <summary>
    /// FUNCTIONAL: Validate configuration
    /// </summary>
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(LogDirectory) &&
        !string.IsNullOrWhiteSpace(BaseFileName) &&
        MaxFileSizeBytes > 0 &&
        MaxFileCount > 0;

    /// <summary>
    /// FUNCTIONAL: Convert to legacy MB format
    /// </summary>
    public int MaxFileSizeMB => (int)(MaxFileSizeBytes / (1024 * 1024));
}