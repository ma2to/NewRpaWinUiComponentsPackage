using Microsoft.Extensions.Logging;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Configuration;

/// <summary>
/// 🔧 INTERNAL CONFIGURATION: Extended configuration for internal use
/// IMMUTABLE: Complete configuration with all options
/// FUNCTIONAL: Builder pattern with fluent API
/// </summary>
internal sealed record LoggerConfiguration
{
    public required string LogDirectory { get; init; }
    public string BaseFileName { get; init; } = "application";
    public int? MaxFileSizeMB { get; init; } = 10;
    public int MaxLogFiles { get; init; } = 10;
    public bool EnableAutoRotation { get; init; } = true;
    public bool EnableRealTimeViewing { get; init; } = false;
    public LogLevel MinLogLevel { get; init; } = LogLevel.Information;
    public bool EnableStructuredLogging { get; init; } = true;
    public bool EnableBackgroundLogging { get; init; } = true;
    public int BufferSize { get; init; } = 1000;
    public TimeSpan FlushInterval { get; init; } = TimeSpan.FromSeconds(5);
    public bool EnablePerformanceMonitoring { get; init; } = false;
    public string DateFormat { get; init; } = "yyyy-MM-dd HH:mm:ss.fff";

    /// <summary>
    /// FUNCTIONAL: Create from public options
    /// </summary>
    public static LoggerConfiguration FromOptions(LoggerOptions options) => new()
    {
        LogDirectory = options.LogDirectory,
        BaseFileName = options.BaseFileName,
        MaxFileSizeMB = options.MaxFileSizeMB,
        MaxLogFiles = options.MaxFileCount,
        EnableAutoRotation = options.EnableAutoRotation,
        MinLogLevel = options.MinLogLevel,
        EnableBackgroundLogging = options.EnableBackgroundLogging,
        EnablePerformanceMonitoring = options.EnablePerformanceMonitoring,
        DateFormat = options.DateFormat,
        EnableRealTimeViewing = false,
        EnableStructuredLogging = true,
        BufferSize = 1000,
        FlushInterval = TimeSpan.FromSeconds(5)
    };

    /// <summary>
    /// FUNCTIONAL: Create minimal configuration
    /// </summary>
    public static LoggerConfiguration Minimal(string logDirectory, string baseFileName) => new()
    {
        LogDirectory = logDirectory,
        BaseFileName = baseFileName,
        MaxFileSizeMB = 10,
        MaxLogFiles = 5,
        EnableAutoRotation = true,
        EnableRealTimeViewing = false,
        MinLogLevel = LogLevel.Information
    };

    /// <summary>
    /// FUNCTIONAL: Validate internal configuration
    /// </summary>
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(LogDirectory) &&
        !string.IsNullOrWhiteSpace(BaseFileName) &&
        (MaxFileSizeMB == null || MaxFileSizeMB > 0) &&
        MaxLogFiles > 0 &&
        BufferSize > 0 &&
        FlushInterval > TimeSpan.Zero;

    /// <summary>
    /// FUNCTIONAL: Get max file size in bytes
    /// </summary>
    public long? MaxFileSizeBytes => MaxFileSizeMB * 1024 * 1024;
}