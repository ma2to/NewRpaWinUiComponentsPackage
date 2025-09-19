using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Domain;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Services.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Services.File;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Core.Functional;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Services.Extensions;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.API;

/// <summary>
/// 🚀 COMPONENT API: Advanced logger component with extended functionality
/// HYBRID ARCHITECTURE: OOP component wrapper with functional operations
/// ENTERPRISE FEATURES: Configuration management, lifecycle control, and monitoring
/// </summary>
public sealed class LoggerAPIComponent : IDisposable
{
    private readonly ILogger? _externalLogger;
    private readonly ILoggerCore _loggerCore;
    private readonly IFileRotationService _rotationService;
    private bool _isInitialized;
    private bool _disposed;

    #region Constructor

    private LoggerAPIComponent(ILogger? externalLogger)
    {
        _externalLogger = externalLogger;
        _rotationService = new FileRotationService(externalLogger);
        _loggerCore = new LoggerCore(externalLogger, _rotationService);

        _externalLogger?.Info("🔧 LoggerAPIComponent created");
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// 🏭 FACTORY: Create file-based logger component
    ///
    /// Creates a component-based logger with advanced file management capabilities.
    /// Suitable for applications requiring programmatic logger control.
    ///
    /// FEATURES:
    /// - Programmatic configuration management
    /// - Runtime log directory changes
    /// - Manual rotation control
    /// - File monitoring and statistics
    /// - Cleanup operations
    ///
    /// USAGE:
    /// var component = LoggerAPIComponent.CreateFileLogger(
    ///     logDirectory: @"C:\Logs",
    ///     baseFileName: "MyApp",
    ///     maxFileSizeMB: 50,
    ///     logger: appLogger);
    ///
    /// await component.InitializeAsync();
    /// // Use component methods for advanced operations
    /// await component.RotateLogsAsync();
    /// var files = await component.GetLogFilesAsync();
    /// </summary>
    /// <param name="logDirectory">Directory for log files</param>
    /// <param name="baseFileName">Base name for log files</param>
    /// <param name="maxFileSizeMB">Maximum file size before rotation</param>
    /// <param name="maxBackupFiles">Maximum number of backup files to keep</param>
    /// <param name="logger">Optional external logger for component operations</param>
    /// <returns>Configured logger component</returns>
    public static LoggerAPIComponent CreateFileLogger(
        string logDirectory,
        string baseFileName = "application",
        int maxFileSizeMB = 10,
        int maxBackupFiles = 5,
        ILogger? logger = null)
    {
        var component = new LoggerAPIComponent(logger);

        // Create configuration and initialize asynchronously
        var options = LoggerOptions.Create(logDirectory, baseFileName) with
        {
            MaxFileSizeBytes = maxFileSizeMB * 1024 * 1024,
            MaxFileCount = maxBackupFiles
        };

        var config = LoggerConfiguration.FromOptions(options);

        // Initialize immediately for factory pattern
        var initResult = component._loggerCore.InitializeAsync(config).RunSync();
        if (initResult.IsSuccess)
        {
            component._isInitialized = true;
            logger?.Info("✅ LoggerAPIComponent initialized successfully");
        }
        else
        {
            logger?.Error("❌ LoggerAPIComponent initialization failed: {Error}", initResult.ErrorMessage);
            throw new InvalidOperationException($"Component initialization failed: {initResult.ErrorMessage}");
        }

        return component;
    }

    /// <summary>
    /// 🏭 FACTORY: Create headless logger for background operations
    ///
    /// Creates a lightweight logger component for background services
    /// and automated operations without UI requirements.
    ///
    /// USAGE:
    /// var headless = LoggerAPIComponent.CreateHeadless(backgroundLogger);
    /// await headless.SetLogDirectoryAsync(@"C:\BackgroundLogs");
    /// await headless.RotateLogsAsync();
    /// </summary>
    /// <param name="logger">Optional external logger</param>
    /// <returns>Headless logger component</returns>
    public static LoggerAPIComponent CreateHeadless(ILogger? logger = null)
    {
        return new LoggerAPIComponent(logger);
    }

    /// <summary>
    /// 🏭 FACTORY: Create from configuration options
    ///
    /// Creates component using LoggerOptions for consistency with LoggerAPI.
    ///
    /// USAGE:
    /// var options = LoggerOptions.Production(@"C:\Logs", "MyApp");
    /// var component = LoggerAPIComponent.FromOptions(options, appLogger);
    /// </summary>
    /// <param name="options">Logger configuration options</param>
    /// <param name="logger">Optional external logger</param>
    /// <returns>Configured logger component</returns>
    public static LoggerAPIComponent FromOptions(LoggerOptions options, ILogger? logger = null)
    {
        return CreateFileLogger(
            logDirectory: options.EnsureNotNull(nameof(options)).LogDirectory,
            baseFileName: options.BaseFileName,
            maxFileSizeMB: options.MaxFileSizeMB,
            maxBackupFiles: options.MaxFileCount,
            logger: logger);
    }

    #endregion

    #region Configuration Management

    /// <summary>
    /// 🔧 CONFIGURATION: Initialize component with custom configuration
    ///
    /// Allows for advanced configuration scenarios with full control
    /// over all logger parameters.
    ///
    /// USAGE:
    /// var config = LoggerConfiguration.Minimal(@"C:\Logs", "MyApp");
    /// var result = await component.InitializeAsync(config);
    /// if (result.IsSuccess) { /* Ready to use */ }
    /// </summary>
    /// <param name="configuration">Complete logger configuration</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<LoggerResult<bool>> InitializeAsync(LoggerConfiguration configuration)
    {
        try
        {
            _externalLogger?.Info("🔧 Initializing component with custom configuration");

            var result = await _loggerCore.InitializeAsync(configuration);
            _isInitialized = result.IsSuccess;

            if (_isInitialized)
            {
                _externalLogger?.Info("✅ Component initialization completed successfully");
                return LoggerResult<bool>.Success(true);
            }
            else
            {
                _externalLogger?.Error("❌ Component initialization failed: {Error}", result.ErrorMessage);
                return LoggerResult<bool>.Failure(result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _externalLogger?.Error(ex, "🚨 Critical error during component initialization");
            return LoggerResult<bool>.Failure($"Initialization failed: {ex.Message}");
        }
    }

    #endregion

    #region File Operations

    /// <summary>
    /// 📁 OPERATION: Set or change log directory
    ///
    /// Dynamically change the log directory during runtime.
    /// Useful for applications with changing storage requirements.
    ///
    /// USAGE:
    /// var result = await component.SetLogDirectoryAsync(@"D:\NewLogs");
    /// if (result.IsSuccess) { /* Directory changed successfully */ }
    /// </summary>
    /// <param name="logDirectory">New log directory path</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<LoggerResult<bool>> SetLogDirectoryAsync(string logDirectory)
    {
        try
        {
            var result = await _loggerCore.SetLogDirectoryAsync(logDirectory);
            return result.IsSuccess
                ? LoggerResult<bool>.Success(true)
                : LoggerResult<bool>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _externalLogger?.Error(ex, "🚨 Error setting log directory: {Directory}", logDirectory);
            return LoggerResult<bool>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// 🔄 OPERATION: Manual log rotation
    ///
    /// Force log rotation regardless of file size or age.
    /// Useful for scheduled maintenance or application restarts.
    ///
    /// USAGE:
    /// var result = await component.RotateLogsAsync();
    /// if (result.IsSuccess)
    /// {
    ///     var summary = result.Value.GetSummary();
    ///     Console.WriteLine($"Rotation completed: {summary}");
    /// }
    /// </summary>
    /// <returns>Result containing rotation details</returns>
    public async Task<LoggerResult<RotationResult>> RotateLogsAsync()
    {
        try
        {
            var result = await _loggerCore.RotateLogsAsync();
            return result.IsSuccess
                ? LoggerResult<RotationResult>.Success(result.Value)
                : LoggerResult<RotationResult>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _externalLogger?.Error(ex, "🚨 Error rotating logs");
            return LoggerResult<RotationResult>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// 🧹 OPERATION: Clean up old log files
    ///
    /// Remove old log files based on age criteria.
    /// Helps manage disk space in long-running applications.
    ///
    /// USAGE:
    /// var result = await component.CleanupOldLogsAsync(30); // Remove files older than 30 days
    /// if (result.IsSuccess)
    /// {
    ///     Console.WriteLine($"Cleaned up {result.Value.FilesDeleted} files, freed {result.Value.BytesFreedMB:F2} MB");
    /// }
    /// </summary>
    /// <param name="maxAgeInDays">Maximum age in days for log files</param>
    /// <returns>Result containing cleanup statistics</returns>
    public async Task<LoggerResult<CleanupResult>> CleanupOldLogsAsync(int maxAgeInDays = 30)
    {
        try
        {
            var result = await _loggerCore.CleanupOldLogsAsync(maxAgeInDays);
            return result.IsSuccess
                ? LoggerResult<CleanupResult>.Success(result.Value)
                : LoggerResult<CleanupResult>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _externalLogger?.Error(ex, "🚨 Error cleaning up logs");
            return LoggerResult<CleanupResult>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// 📊 QUERY: Get current log file path
    ///
    /// Retrieve the path to the currently active log file.
    /// Useful for external log monitoring tools.
    ///
    /// USAGE:
    /// var result = await component.GetCurrentLogFileAsync();
    /// if (result.IsSuccess)
    /// {
    ///     var currentFile = result.Value;
    ///     // Process current log file
    /// }
    /// </summary>
    /// <returns>Result containing current log file path</returns>
    public async Task<LoggerResult<string>> GetCurrentLogFileAsync()
    {
        try
        {
            var result = await _loggerCore.GetCurrentLogFileAsync();
            return result.IsSuccess
                ? LoggerResult<string>.Success(result.Value)
                : LoggerResult<string>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _externalLogger?.Error(ex, "🚨 Error getting current log file");
            return LoggerResult<string>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// 📊 QUERY: Get all log files information
    ///
    /// Retrieve detailed information about all log files in the directory.
    /// Includes file sizes, dates, and active status.
    ///
    /// USAGE:
    /// var result = await component.GetLogFilesAsync();
    /// if (result.IsSuccess)
    /// {
    ///     foreach (var fileInfo in result.Value)
    ///     {
    ///         Console.WriteLine($"{fileInfo.FileName}: {fileInfo.SizeMB:F2} MB, {fileInfo.CreatedTime}");
    ///     }
    /// }
    /// </summary>
    /// <returns>Result containing list of log file information</returns>
    public async Task<LoggerResult<IReadOnlyList<LogFileInfo>>> GetLogFilesAsync()
    {
        try
        {
            var result = await _loggerCore.GetLogFilesAsync();
            return result.IsSuccess
                ? LoggerResult<IReadOnlyList<LogFileInfo>>.Success(result.Value)
                : LoggerResult<IReadOnlyList<LogFileInfo>>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _externalLogger?.Error(ex, "🚨 Error getting log files");
            return LoggerResult<IReadOnlyList<LogFileInfo>>.Failure(ex.Message);
        }
    }

    #endregion

    #region Properties

    /// <summary>Check if component has been initialized</summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>Current log directory path</summary>
    public string? LogDirectory => _loggerCore.LogDirectory;

    /// <summary>Total size of all log files in MB</summary>
    public double TotalLogSizeMB => _loggerCore.TotalLogSizeMB;

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            _loggerCore?.Dispose();
            _externalLogger?.Info("🔧 LoggerAPIComponent disposed successfully");
        }
        catch (Exception ex)
        {
            _externalLogger?.Error(ex, "🚨 Error during component disposal");
        }
        finally
        {
            _disposed = true;
        }
    }

    #endregion
}