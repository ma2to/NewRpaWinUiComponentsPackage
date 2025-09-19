using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Domain;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Core.Functional;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Core.Constants;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Services.Extensions;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Services.Core;

/// <summary>
/// ⚙️ CORE SERVICE: Primary business logic for logger operations
/// HYBRID ARCHITECTURE: Functional approach with OOP when beneficial
/// CLEAN ARCHITECTURE: Application service coordinating domain and infrastructure
/// </summary>
internal sealed class LoggerCore : ILoggerCore
{
    private readonly ILogger? _externalLogger;
    private readonly IFileRotationService _rotationService;
    private LoggerConfiguration? _configuration;
    private string? _logDirectory;
    private string? _currentLogFile;
    private bool _isInitialized;
    private bool _disposed;

    #region Constructor

    public LoggerCore(ILogger? externalLogger, IFileRotationService rotationService)
    {
        _externalLogger = externalLogger;
        _rotationService = rotationService.EnsureNotNull(nameof(rotationService));

        _externalLogger?.LogInformation("LoggerCore created with external logger: {HasLogger}",
            externalLogger != null);
    }

    #endregion

    #region Properties

    public string? LogDirectory => _logDirectory;
    public string? CurrentLogFile => _currentLogFile;
    public bool IsInitialized => _isInitialized;

    public double TotalLogSizeMB =>
        CalculateTotalLogSize()
            .Map(bytes => bytes / (1024.0 * 1024.0))
            .ValueOr(0.0);

    #endregion

    #region Initialization

    public async Task<Result<bool>> InitializeAsync(LoggerConfiguration config)
    {
        try
        {
            _externalLogger?.LogInformation("Initializing LoggerCore with directory: {Directory}",
                config.LogDirectory);

            // FUNCTIONAL: Validate configuration
            var validationResult = ValidateConfiguration(config);
            if (validationResult.IsFailure)
            {
                return Result<bool>.Failure(validationResult.ErrorMessage);
            }

            _configuration = config;

            // FUNCTIONAL: Initialize directory and file
            return await SetLogDirectoryAsync(config.LogDirectory)
                .BindAsync(async _ => await CreateInitialLogFile(config.BaseFileName))
                .MapAsync(async _ =>
                {
                    _isInitialized = true;
                    _externalLogger?.LogInformation("LoggerCore initialization completed successfully");
                    return true;
                });
        }
        catch (Exception ex)
        {
            _externalLogger?.LogError(ex, "LoggerCore initialization failed");
            return Result<bool>.Failure($"Initialization failed: {ex.Message}", ex);
        }
    }

    #endregion

    #region File Management

    public async Task<Result<bool>> SetLogDirectoryAsync(string directory)
    {
        return await Result<string>.Try(() => directory.EnsureNotWhiteSpace(nameof(directory)))
            .BindAsync(async dir => await CreateDirectoryIfNotExists(dir))
            .Tap(dir =>
            {
                _logDirectory = dir;
                _externalLogger?.LogInformation("Log directory set to: {Directory}", dir);
            })
            .Map(_ => true);
    }

    public async Task<Result<RotationResult>> RotateLogsAsync()
    {
        if (!_isInitialized || _configuration == null)
        {
            return Result<RotationResult>.Failure("LoggerCore not initialized");
        }

        return await _rotationService.RotateFileAsync(
            _currentLogFile ?? string.Empty,
            _configuration.BaseFileName,
            _logDirectory ?? string.Empty)
            .Tap(result =>
            {
                if (result.IsSuccess)
                {
                    _currentLogFile = result.NewFilePath;
                    _externalLogger?.LogInformation("Log rotation completed: {Summary}",
                        result.GetSummary());
                }
            });
    }

    public async Task<Result<CleanupResult>> CleanupOldLogsAsync(int maxAgeInDays = 30)
    {
        if (!_isInitialized || _configuration == null || _logDirectory == null)
        {
            return Result<CleanupResult>.Failure("LoggerCore not initialized");
        }

        return await _rotationService.CleanupByAgeAsync(_logDirectory, TimeSpan.FromDays(maxAgeInDays))
            .Tap(result =>
            {
                if (result.IsSuccess)
                {
                    _externalLogger?.LogInformation("Cleanup completed: {Summary}",
                        result.GetSummary());
                }
            });
    }

    public async Task<Result<long>> GetCurrentLogSizeAsync()
    {
        return await Result<string>.Try(() => _currentLogFile.EnsureNotNull("Current log file"))
            .Map(filePath => File.Exists(filePath) ? new FileInfo(filePath).Length : 0L)
            .ToAsync();
    }

    public async Task<Result<IReadOnlyList<LogFileInfo>>> GetLogFilesAsync()
    {
        if (_logDirectory == null)
        {
            return Result<IReadOnlyList<LogFileInfo>>.Failure("Log directory not set");
        }

        return await Result<IReadOnlyList<LogFileInfo>>.TryAsync(async () =>
        {
            await Task.Yield();
            return LogFileInfo.FromDirectory(_logDirectory, LoggerConstants.LogFilePattern);
        });
    }

    public async Task<Result<string>> GetCurrentLogFileAsync()
    {
        return await Result<string>.Try(() =>
            _currentLogFile.EnsureNotNull("Current log file not available"))
            .ToAsync();
    }

    #endregion

    #region Logging Operations

    public async Task<Result<bool>> WriteLogEntryAsync(LogEntry entry)
    {
        if (!_isInitialized || _currentLogFile == null)
        {
            return Result<bool>.Failure("Logger not initialized");
        }

        return await Result<bool>.TryAsync(async () =>
        {
            // FUNCTIONAL: Check if rotation needed before writing
            if (_configuration?.EnableAutoRotation == true && ShouldRotateFile())
            {
                var rotationResult = await RotateLogsAsync();
                if (rotationResult.IsFailure)
                {
                    _externalLogger?.LogWarning("Rotation failed, continuing with current file: {Error}",
                        rotationResult.ErrorMessage);
                }
            }

            // FUNCTIONAL: Write entry to file
            var formattedEntry = entry.ToFileFormat(_configuration?.DateFormat ?? LoggerConstants.DefaultDateFormat);
            await File.AppendAllTextAsync(_currentLogFile, formattedEntry, LoggerConstants.DefaultEncoding);

            return true;
        });
    }

    public bool IsLogLevelEnabled(LogLevel level)
    {
        return level >= (_configuration?.MinLogLevel ?? LogLevel.Information);
    }

    public async Task<Result<bool>> FlushAsync()
    {
        // File operations are auto-flushed in this implementation
        await Task.CompletedTask;
        return Result<bool>.Success(true);
    }

    #endregion

    #region Private Methods

    private Result<LoggerConfiguration> ValidateConfiguration(LoggerConfiguration config)
    {
        return config.IsValid()
            ? Result<LoggerConfiguration>.Success(config)
            : Result<LoggerConfiguration>.Failure(LoggerConstants.InvalidConfigurationError);
    }

    private async Task<Result<string>> CreateDirectoryIfNotExists(string directory)
    {
        return await Result<string>.TryAsync(async () =>
        {
            await Task.Yield();

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _externalLogger?.LogInformation("Created log directory: {Directory}", directory);
            }

            return directory;
        });
    }

    private async Task<Result<string>> CreateInitialLogFile(string baseFileName)
    {
        return await Result<string>.TryAsync(async () =>
        {
            await Task.Yield();

            var fileName = $"{baseFileName}_{DateTime.Now.ToString(LoggerConstants.RotationTimestampFormat)}{LoggerConstants.LogFileExtension}";
            var filePath = Path.Combine(_logDirectory!, fileName);

            if (!File.Exists(filePath))
            {
                var header = $"# Log file created: {DateTime.Now.ToString(LoggerConstants.DefaultDateFormat)}{Environment.NewLine}";
                await File.WriteAllTextAsync(filePath, header, LoggerConstants.DefaultEncoding);
            }

            _currentLogFile = filePath;
            _externalLogger?.LogInformation("Initial log file created: {FilePath}", filePath);

            return filePath;
        });
    }

    private bool ShouldRotateFile()
    {
        if (_currentLogFile == null || !File.Exists(_currentLogFile) || _configuration?.MaxFileSizeBytes == null)
        {
            return false;
        }

        return _rotationService.ShouldRotateBySize(_currentLogFile, _configuration.MaxFileSizeBytes.Value);
    }

    private Result<long> CalculateTotalLogSize()
    {
        if (_logDirectory == null)
        {
            return Result<long>.Success(0L);
        }

        return Result<long>.Try(() =>
        {
            return Directory.Exists(_logDirectory)
                ? Directory.GetFiles(_logDirectory, LoggerConstants.LogFilePattern)
                    .Select(file => new FileInfo(file).Length)
                    .Sum()
                : 0L;
        });
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            _externalLogger?.LogInformation("Disposing LoggerCore");
            _isInitialized = false;
            _disposed = true;
        }
        catch (Exception ex)
        {
            _externalLogger?.LogError(ex, "Error during LoggerCore disposal");
        }
    }

    #endregion
}