using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Domain;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Core.Functional;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Core.Constants;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Services.Extensions;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Services.File;

/// <summary>
/// 📁 FILE SERVICE: Microsoft.Extensions.Logging.ILogger implementation for file operations
/// HYBRID ARCHITECTURE: OOP for ILogger interface, functional for file operations
/// THREAD-SAFE: Concurrent logging support with proper synchronization
/// </summary>
internal sealed class FileLoggerService : IFileLoggerService
{
    private readonly ILoggerCore _loggerCore;
    private readonly ILogger? _externalLogger;
    private readonly object _lockObject = new();
    private LoggerConfiguration? _configuration;
    private bool _isInitialized;
    private bool _disposed;

    #region Constructor

    public FileLoggerService(ILoggerCore loggerCore, ILogger? externalLogger = null)
    {
        _loggerCore = loggerCore.EnsureNotNull(nameof(loggerCore));
        _externalLogger = externalLogger;

        _externalLogger?.LogInformation("FileLoggerService created");
    }

    #endregion

    #region Properties

    public string LogDirectory => _loggerCore.LogDirectory ?? string.Empty;
    public string? CurrentLogFile => _loggerCore.CurrentLogFile;
    public bool IsInitialized => _isInitialized && _loggerCore.IsInitialized;

    #endregion

    #region ILogger Implementation

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        // Scopes not supported in file-only implementation
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return IsInitialized && _loggerCore.IsLogLevelEnabled(logLevel);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel) || _disposed)
        {
            return;
        }

        // FUNCTIONAL: Create log entry and write asynchronously
        LogEntry.Create(logLevel, eventId, state, exception, formatter)
            .Pipe(entry => WriteLogEntrySync(entry))
            .OnFailure(error => _externalLogger?.LogError("Failed to write log entry: {Error}", error));
    }

    #endregion

    #region IFileLoggerService Implementation

    public async Task<Result<bool>> InitializeAsync(LoggerConfiguration configuration)
    {
        try
        {
            _externalLogger?.LogInformation("Initializing FileLoggerService");

            return await _loggerCore.InitializeAsync(configuration)
                .Tap(result =>
                {
                    if (result.IsSuccess)
                    {
                        _configuration = configuration;
                        _isInitialized = true;
                        _externalLogger?.LogInformation("FileLoggerService initialized successfully");
                    }
                    else
                    {
                        _externalLogger?.LogError("FileLoggerService initialization failed: {Error}", result.ErrorMessage);
                    }
                });
        }
        catch (Exception ex)
        {
            _externalLogger?.LogError(ex, "Critical error during FileLoggerService initialization");
            return Result<bool>.Failure($"Initialization failed: {ex.Message}", ex);
        }
    }

    public async Task<Result<RotationResult>> RotateAsync()
    {
        if (!IsInitialized)
        {
            return Result<RotationResult>.Failure("Service not initialized");
        }

        return await _loggerCore.RotateLogsAsync()
            .Tap(result =>
            {
                if (result.IsSuccess)
                {
                    _externalLogger?.LogInformation("Manual rotation completed: {Summary}",
                        result.GetSummary());
                }
            });
    }

    public async Task<Result<long>> GetFileSizeAsync()
    {
        return await _loggerCore.GetCurrentLogSizeAsync();
    }

    public bool ShouldRotate()
    {
        if (!IsInitialized || _configuration?.MaxFileSizeBytes == null || CurrentLogFile == null)
        {
            return false;
        }

        return File.Exists(CurrentLogFile) &&
               new FileInfo(CurrentLogFile).Length >= _configuration.MaxFileSizeBytes;
    }

    public async Task<Result<bool>> FlushAsync()
    {
        return await _loggerCore.FlushAsync();
    }

    #endregion

    #region Configuration Methods

    public Result<bool> SetLogLevel(LogLevel logLevel)
    {
        if (_configuration == null)
        {
            return Result<bool>.Failure("Service not initialized");
        }

        return Result<bool>.Try(() =>
        {
            _configuration = _configuration with { MinLogLevel = logLevel };
            _externalLogger?.LogInformation("Log level updated to: {LogLevel}", logLevel);
            return true;
        });
    }

    public Result<bool> SetFileSizeLimit(long maxSizeBytes)
    {
        if (_configuration == null)
        {
            return Result<bool>.Failure("Service not initialized");
        }

        return Result<bool>.Try(() =>
        {
            if (maxSizeBytes < LoggerConstants.MinFileSizeBytes || maxSizeBytes > LoggerConstants.MaxFileSizeBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(maxSizeBytes),
                    $"File size must be between {LoggerConstants.MinFileSizeBytes} and {LoggerConstants.MaxFileSizeBytes} bytes");
            }

            _configuration = _configuration with { MaxFileSizeMB = (int)(maxSizeBytes / (1024 * 1024)) };
            _externalLogger?.LogInformation("File size limit updated to: {SizeBytes} bytes ({SizeMB} MB)",
                maxSizeBytes, maxSizeBytes / (1024.0 * 1024.0));
            return true;
        });
    }

    #endregion

    #region Private Methods

    private Result<bool> WriteLogEntrySync(LogEntry entry)
    {
        try
        {
            lock (_lockObject)
            {
                // HYBRID: Use async method but wait for completion in sync context
                var writeTask = _loggerCore.WriteLogEntryAsync(entry);
                return writeTask.RunSync();
            }
        }
        catch (Exception ex)
        {
            _externalLogger?.LogError(ex, "Synchronous log write failed");
            return Result<bool>.Failure($"Write failed: {ex.Message}", ex);
        }
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            lock (_lockObject)
            {
                _externalLogger?.LogInformation("Disposing FileLoggerService");
                _loggerCore?.Dispose();
                _isInitialized = false;
                _disposed = true;
            }
        }
        catch (Exception ex)
        {
            _externalLogger?.LogError(ex, "Error during FileLoggerService disposal");
        }
    }

    #endregion
}