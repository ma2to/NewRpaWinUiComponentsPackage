using Microsoft.Extensions.Logging;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Interfaces;

/// <summary>
/// 🔗 CONFIGURATION CONTRACT: Interface for logger configuration
/// FUNCTIONAL: Immutable configuration with validation
/// SOLID: Interface segregation for configuration concerns
/// </summary>
public interface ILoggerConfiguration
{
    #region Core Properties

    string LogDirectory { get; }
    string BaseFileName { get; }
    LogLevel MinLogLevel { get; }

    #endregion

    #region File Management

    int? MaxFileSizeMB { get; }
    int MaxLogFiles { get; }
    bool EnableAutoRotation { get; }

    #endregion

    #region Advanced Features

    bool EnableStructuredLogging { get; }
    bool EnableBackgroundLogging { get; }
    bool EnablePerformanceMonitoring { get; }
    string DateFormat { get; }

    #endregion

    #region Validation

    /// <summary>
    /// FUNCTIONAL: Validate configuration integrity
    /// </summary>
    bool IsValid();

    /// <summary>
    /// FUNCTIONAL: Get validation errors
    /// </summary>
    IReadOnlyList<string> GetValidationErrors();

    #endregion

    #region Transformation

    /// <summary>
    /// FUNCTIONAL: Create new configuration with updated directory
    /// </summary>
    ILoggerConfiguration WithDirectory(string newDirectory);

    /// <summary>
    /// FUNCTIONAL: Create new configuration with updated file name
    /// </summary>
    ILoggerConfiguration WithFileName(string newFileName);

    /// <summary>
    /// FUNCTIONAL: Create new configuration with updated log level
    /// </summary>
    ILoggerConfiguration WithLogLevel(LogLevel newLevel);

    #endregion
}