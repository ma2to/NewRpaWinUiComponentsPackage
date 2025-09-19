using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Domain;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Core.Functional;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Interfaces;

/// <summary>
/// 🔗 CORE CONTRACT: Primary interface for logger core operations
/// FUNCTIONAL: Result-based error handling with monadic composition
/// CLEAN ARCHITECTURE: Abstraction between application and infrastructure layers
/// </summary>
public interface ILoggerCore : IDisposable
{
    #region Properties

    /// <summary>Current log directory path</summary>
    string? LogDirectory { get; }

    /// <summary>Current log file path</summary>
    string? CurrentLogFile { get; }

    /// <summary>Is Logger initialized and ready for operations</summary>
    bool IsInitialized { get; }

    /// <summary>Total size of all log files in MB</summary>
    double TotalLogSizeMB { get; }

    #endregion

    #region Initialization

    /// <summary>
    /// FUNCTIONAL: Initialize logger with configuration
    /// </summary>
    Task<Result<bool>> InitializeAsync(LoggerConfiguration config);

    #endregion

    #region File Management

    /// <summary>
    /// FUNCTIONAL: Set log directory and create if needed
    /// </summary>
    Task<Result<bool>> SetLogDirectoryAsync(string directory);

    /// <summary>
    /// FUNCTIONAL: Rotate log files (archive current, start new)
    /// </summary>
    Task<Result<RotationResult>> RotateLogsAsync();

    /// <summary>
    /// FUNCTIONAL: Clean up old log files based on retention policy
    /// </summary>
    Task<Result<CleanupResult>> CleanupOldLogsAsync(int maxAgeInDays = 30);

    /// <summary>
    /// FUNCTIONAL: Get current log file size
    /// </summary>
    Task<Result<long>> GetCurrentLogSizeAsync();

    /// <summary>
    /// FUNCTIONAL: Get list of all log files in directory
    /// </summary>
    Task<Result<IReadOnlyList<LogFileInfo>>> GetLogFilesAsync();

    /// <summary>
    /// FUNCTIONAL: Get current log file path
    /// </summary>
    Task<Result<string>> GetCurrentLogFileAsync();

    #endregion

    #region Logging Operations

    /// <summary>
    /// FUNCTIONAL: Write log entry to current file
    /// </summary>
    Task<Result<bool>> WriteLogEntryAsync(LogEntry entry);

    /// <summary>
    /// FUNCTIONAL: Check if log level is enabled
    /// </summary>
    bool IsLogLevelEnabled(LogLevel level);

    /// <summary>
    /// FUNCTIONAL: Flush pending log entries
    /// </summary>
    Task<Result<bool>> FlushAsync();

    #endregion
}