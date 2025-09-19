namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Core.Constants;

/// <summary>
/// 📊 CONSTANTS: Logger system constants and configuration values
/// IMMUTABLE: Compile-time constants for consistent behavior
/// SENIOR ARCHITECTURE: Centralized configuration values
/// </summary>
public static class LoggerConstants
{
    #region File Extensions

    public const string LogFileExtension = ".log";
    public const string ArchiveFileExtension = ".archive.log";
    public const string BackupFileExtension = ".backup.log";
    public const string TempFileExtension = ".tmp";

    #endregion

    #region Default Values

    public const int DefaultMaxFileSizeMB = 10;
    public const int DefaultMaxLogFiles = 10;
    public const int DefaultBufferSize = 1000;
    public const int DefaultFlushIntervalSeconds = 5;
    public const string DefaultDateFormat = "yyyy-MM-dd HH:mm:ss.fff";
    public const string DefaultBaseFileName = "application";

    #endregion

    #region Size Limits

    public const long MinFileSizeBytes = 1024; // 1 KB minimum
    public const long MaxFileSizeBytes = 1024L * 1024L * 1024L * 10L; // 10 GB maximum
    public const int MinLogFiles = 1;
    public const int MaxLogFiles = 1000;
    public const int MinBufferSize = 1;
    public const int MaxBufferSize = 100000;

    #endregion

    #region File Patterns

    public const string LogFilePattern = "*.log";
    public const string ArchiveFilePattern = "*.archive.log";
    public const string BackupFilePattern = "*.backup.log";
    public const string AllLogFilesPattern = "*.*";

    #endregion

    #region Rotation Settings

    public const int DefaultRetentionDays = 30;
    public const int MinRetentionDays = 1;
    public const int MaxRetentionDays = 365;
    public const string RotationTimestampFormat = "yyyyMMdd_HHmmss";

    #endregion

    #region Performance Settings

    public const int MinFlushIntervalMs = 100;
    public const int MaxFlushIntervalMs = 60000; // 1 minute
    public const int DefaultBatchSize = 100;
    public const int MaxBatchSize = 10000;
    public const int DefaultConcurrencyLevel = Environment.ProcessorCount;

    #endregion

    #region Error Messages

    public const string InvalidDirectoryError = "Log directory cannot be null or empty";
    public const string InvalidFileNameError = "Base file name cannot be null or empty";
    public const string DirectoryNotFoundError = "Cannot create or access log directory";
    public const string InsufficientPermissionsError = "Insufficient permissions for log directory";
    public const string FileSizeExceededError = "File size exceeded maximum limit";
    public const string InvalidConfigurationError = "Logger configuration is invalid";
    public const string InitializationFailedError = "Logger initialization failed";
    public const string RotationFailedError = "Log rotation failed";
    public const string CleanupFailedError = "Log cleanup failed";

    #endregion

    #region Log Level Names

    public static readonly IReadOnlyDictionary<Microsoft.Extensions.Logging.LogLevel, string> LogLevelNames =
        new Dictionary<Microsoft.Extensions.Logging.LogLevel, string>
        {
            { Microsoft.Extensions.Logging.LogLevel.Trace, "TRACE" },
            { Microsoft.Extensions.Logging.LogLevel.Debug, "DEBUG" },
            { Microsoft.Extensions.Logging.LogLevel.Information, "INFO" },
            { Microsoft.Extensions.Logging.LogLevel.Warning, "WARN" },
            { Microsoft.Extensions.Logging.LogLevel.Error, "ERROR" },
            { Microsoft.Extensions.Logging.LogLevel.Critical, "FATAL" },
            { Microsoft.Extensions.Logging.LogLevel.None, "NONE" }
        };

    #endregion

    #region Environment Detection

    public static readonly bool IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
    public static readonly bool IsUnix = Environment.OSVersion.Platform == PlatformID.Unix;
    public static readonly bool IsMacOS = Environment.OSVersion.Platform == PlatformID.MacOSX;

    #endregion

    #region Path Separators

    public static readonly char PathSeparator = Path.DirectorySeparatorChar;
    public static readonly char AltPathSeparator = Path.AltDirectorySeparatorChar;
    public static readonly string PathSeparatorString = PathSeparator.ToString();

    #endregion

    #region Encoding

    public static readonly System.Text.Encoding DefaultEncoding = System.Text.Encoding.UTF8;
    public const bool DefaultDetectEncodingFromByteOrderMarks = true;

    #endregion
}