using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Domain;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Core.Functional;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Interfaces;

/// <summary>
/// 🔗 SERVICE CONTRACT: File logger service interface
/// HYBRID: Combines ILogger interface with functional file operations
/// SOLID: Segregated interface for file-specific operations
/// </summary>
public interface IFileLoggerService : ILogger, IDisposable
{
    #region Properties

    /// <summary>Current log directory</summary>
    string LogDirectory { get; }

    /// <summary>Current log file path</summary>
    string? CurrentLogFile { get; }

    /// <summary>Is service initialized</summary>
    bool IsInitialized { get; }

    #endregion

    #region File Operations

    /// <summary>
    /// FUNCTIONAL: Initialize service with configuration
    /// </summary>
    Task<Result<bool>> InitializeAsync(LoggerConfiguration configuration);

    /// <summary>
    /// FUNCTIONAL: Force log rotation
    /// </summary>
    Task<Result<RotationResult>> RotateAsync();

    /// <summary>
    /// FUNCTIONAL: Get current file size
    /// </summary>
    Task<Result<long>> GetFileSizeAsync();

    /// <summary>
    /// FUNCTIONAL: Check if rotation is needed
    /// </summary>
    bool ShouldRotate();

    /// <summary>
    /// FUNCTIONAL: Flush all pending writes
    /// </summary>
    Task<Result<bool>> FlushAsync();

    #endregion

    #region Configuration

    /// <summary>
    /// FUNCTIONAL: Update log level
    /// </summary>
    Result<bool> SetLogLevel(LogLevel logLevel);

    /// <summary>
    /// FUNCTIONAL: Update file size limit
    /// </summary>
    Result<bool> SetFileSizeLimit(long maxSizeBytes);

    #endregion
}