using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Services.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Services.File;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Core.Functional;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Core.Constants;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Services.Extensions;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.API;

/// <summary>
/// 🎯 CORE API: Primary implementation for logger creation and management
/// CLEAN ARCHITECTURE: Application layer coordinating domain and infrastructure
/// FUNCTIONAL: Monadic error handling with composable operations
/// </summary>
public static class LoggerAPI
{
    /// <summary>
    /// 🚀 ENTERPRISE FILE LOGGER: Create professional file logger with automatic rotation
    ///
    /// FEATURES:
    /// ✅ FILE-ONLY LOGGING: Pure file-based logging without UI components
    /// ✅ AUTOMATIC ROTATION: Size-based file rotation with configurable limits
    /// ✅ THREAD-SAFE: Concurrent logging support with proper synchronization
    /// ✅ FAIL-SAFE: Graceful error handling with external logger fallback
    /// ✅ CLEAN API: Simple interface hiding complex implementation details
    ///
    /// USAGE EXAMPLES:
    ///
    /// // Basic file logger with rotation
    /// var logger = LoggerAPI.CreateFileLogger(
    ///     externalLogger: appLogger,
    ///     logDirectory: @"C:\MyApp\Logs",
    ///     baseFileName: "MyApp",
    ///     maxFileSizeMB: 50);
    ///
    /// // Simple development logger
    /// var devLogger = LoggerAPI.CreateFileLogger(
    ///     externalLogger: null,
    ///     logDirectory: Path.GetTempPath(),
    ///     baseFileName: "DevTest",
    ///     maxFileSizeMB: 5);
    ///
    /// // High-volume production logger
    /// var prodLogger = LoggerAPI.CreateFileLogger(
    ///     externalLogger: systemLogger,
    ///     logDirectory: @"D:\ProductionLogs",
    ///     baseFileName: "Production",
    ///     maxFileSizeMB: 200);
    /// </summary>
    /// <param name="externalLogger">Optional external logger for internal operations. Can be null.</param>
    /// <param name="logDirectory">Directory for log files. Will be created if doesn't exist.</param>
    /// <param name="baseFileName">Base name for log files without extension.</param>
    /// <param name="maxFileSizeMB">Maximum file size in MB before rotation. null = no rotation.</param>
    /// <returns>ILogger implementation for file logging with rotation support</returns>
    /// <exception cref="ArgumentException">Invalid directory or filename</exception>
    /// <exception cref="DirectoryNotFoundException">Cannot create log directory</exception>
    /// <exception cref="UnauthorizedAccessException">Insufficient permissions</exception>
    public static ILogger CreateFileLogger(
        ILogger? externalLogger,
        string logDirectory,
        string baseFileName,
        int? maxFileSizeMB)
    {
        return CreateFileLoggerInternal(externalLogger, logDirectory, baseFileName, maxFileSizeMB)
            .ValueOrThrow(() => new InvalidOperationException("Failed to create file logger"));
    }

    /// <summary>
    /// 🔧 CONFIGURATION-BASED API: Create file logger using LoggerOptions
    ///
    /// Modern approach with configuration object for better extensibility.
    /// Provides better IntelliSense support and type safety.
    ///
    /// USAGE:
    /// var options = LoggerOptions.Create(@"C:\Logs", "MyApp");
    /// var logger = LoggerAPI.CreateFileLogger(options);
    ///
    /// // Or with predefined configurations:
    /// var debugLogger = LoggerAPI.CreateFileLogger(LoggerOptions.Debug(@"C:\Logs", "Debug"));
    /// var prodLogger = LoggerAPI.CreateFileLogger(LoggerOptions.Production(@"C:\Logs", "Prod"));
    /// </summary>
    /// <param name="options">Logger configuration options</param>
    /// <returns>ILogger implementation configured according to options</returns>
    /// <exception cref="ArgumentNullException">Options cannot be null</exception>
    /// <exception cref="ArgumentException">Invalid configuration</exception>
    public static ILogger CreateFileLogger(LoggerOptions options)
    {
        return CreateFileLogger(
            externalLogger: null,
            logDirectory: options.EnsureNotNull(nameof(options)).LogDirectory,
            baseFileName: options.BaseFileName,
            maxFileSizeMB: options.MaxFileSizeMB);
    }

    /// <summary>
    /// 🔧 ENHANCED CONFIGURATION API: Create file logger with external logger and options
    ///
    /// Combines configuration convenience with external logger support.
    /// Best for complex scenarios requiring audit trails and chained logging.
    ///
    /// USAGE:
    /// var mainLogger = serviceProvider.GetService<ILogger<MyApp>>();
    /// var options = LoggerOptions.Production(@"C:\Logs", "MyApp");
    /// var fileLogger = LoggerAPI.CreateFileLogger(mainLogger, options);
    /// </summary>
    /// <param name="externalLogger">Optional external logger for audit trail</param>
    /// <param name="options">Complete logger configuration</param>
    /// <returns>ILogger implementation with external logging support</returns>
    /// <exception cref="ArgumentNullException">Options cannot be null</exception>
    public static ILogger CreateFileLogger(ILogger? externalLogger, LoggerOptions options)
    {
        return CreateFileLogger(
            externalLogger: externalLogger,
            logDirectory: options.EnsureNotNull(nameof(options)).LogDirectory,
            baseFileName: options.BaseFileName,
            maxFileSizeMB: options.MaxFileSizeMB);
    }

    /// <summary>
    /// 🎯 RESULT-BASED API: Create file logger with explicit error handling
    ///
    /// Returns Result<ILogger> for functional error handling patterns.
    /// Use when you need explicit control over error scenarios.
    ///
    /// USAGE:
    /// var result = LoggerAPI.CreateFileLoggerSafe(appLogger, @"C:\Logs", "MyApp", 50);
    /// if (result.IsSuccess)
    /// {
    ///     var logger = result.Value;
    ///     logger.LogInformation("Logger created successfully");
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Logger creation failed: {result.ErrorMessage}");
    /// }
    /// </summary>
    /// <param name="externalLogger">Optional external logger</param>
    /// <param name="logDirectory">Log directory path</param>
    /// <param name="baseFileName">Base file name</param>
    /// <param name="maxFileSizeMB">Max file size in MB</param>
    /// <returns>Result containing ILogger or error information</returns>
    public static LoggerResult<ILogger> CreateFileLoggerSafe(
        ILogger? externalLogger,
        string logDirectory,
        string baseFileName,
        int? maxFileSizeMB)
    {
        return CreateFileLoggerInternal(externalLogger, logDirectory, baseFileName, maxFileSizeMB)
            .Map(logger => (ILogger)logger)
            .ToLoggerResult();
    }

    #region Private Implementation

    /// <summary>
    /// INTERNAL: Core implementation for file logger creation
    /// FUNCTIONAL: Composable error handling with Result monad
    /// </summary>
    private static Result<IFileLoggerService> CreateFileLoggerInternal(
        ILogger? externalLogger,
        string logDirectory,
        string baseFileName,
        int? maxFileSizeMB)
    {
        return Result<IFileLoggerService>.Try(() =>
        {
            // FUNCTIONAL: Validate input parameters
            ValidateCreateLoggerParameters(logDirectory, baseFileName, maxFileSizeMB);

            externalLogger?.Info("📁 Creating file logger: Directory={Directory}, BaseFileName={BaseFileName}, MaxSizeMB={MaxSize}",
                logDirectory, baseFileName, maxFileSizeMB?.ToString() ?? "unlimited");

            // FUNCTIONAL: Create configuration
            var configuration = CreateLoggerConfiguration(logDirectory, baseFileName, maxFileSizeMB);

            // FUNCTIONAL: Create services with dependency injection
            var rotationService = new FileRotationService(externalLogger);
            var loggerCore = new LoggerCore(externalLogger, rotationService);
            var fileLoggerService = new FileLoggerService(loggerCore, externalLogger);

            // FUNCTIONAL: Initialize service
            var initResult = fileLoggerService.InitializeAsync(configuration).RunSync();
            if (initResult.IsFailure)
            {
                throw new InvalidOperationException($"Logger initialization failed: {initResult.ErrorMessage}");
            }

            externalLogger?.Info("✅ File logger created successfully");
            return fileLoggerService;
        });
    }

    /// <summary>
    /// FUNCTIONAL: Validate input parameters
    /// </summary>
    private static void ValidateCreateLoggerParameters(string logDirectory, string baseFileName, int? maxFileSizeMB)
    {
        logDirectory.EnsureNotWhiteSpace(nameof(logDirectory));
        baseFileName.EnsureNotWhiteSpace(nameof(baseFileName));

        if (maxFileSizeMB.HasValue && maxFileSizeMB.Value <= 0)
        {
            throw new ArgumentException("MaxFileSizeMB must be greater than 0 if specified", nameof(maxFileSizeMB));
        }
    }

    /// <summary>
    /// FUNCTIONAL: Create internal configuration from parameters
    /// </summary>
    private static LoggerConfiguration CreateLoggerConfiguration(string logDirectory, string baseFileName, int? maxFileSizeMB)
    {
        return new LoggerConfiguration
        {
            LogDirectory = logDirectory.Trim(),
            BaseFileName = baseFileName.Trim(),
            MaxFileSizeMB = maxFileSizeMB,
            MaxLogFiles = LoggerConstants.DefaultMaxLogFiles,
            EnableAutoRotation = maxFileSizeMB.HasValue,
            EnableRealTimeViewing = false,
            MinLogLevel = LogLevel.Information,
            EnableStructuredLogging = true,
            EnableBackgroundLogging = true,
            BufferSize = LoggerConstants.DefaultBufferSize,
            FlushInterval = TimeSpan.FromSeconds(LoggerConstants.DefaultFlushIntervalSeconds),
            EnablePerformanceMonitoring = false,
            DateFormat = LoggerConstants.DefaultDateFormat
        };
    }

    #endregion
}

#region Extension Methods

/// <summary>
/// FUNCTIONAL: Extension methods for Result to LoggerResult conversion
/// </summary>
internal static class ResultExtensions
{
    internal static LoggerResult<T> ToLoggerResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? LoggerResult<T>.Success(result.Value)
            : LoggerResult<T>.Failure(result.ErrorMessage);
    }
}

#endregion