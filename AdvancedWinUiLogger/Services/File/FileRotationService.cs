using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Domain;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Core.Functional;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Core.Constants;
using Microsoft.Extensions.Logging;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Services.File;

/// <summary>
/// 🔄 ROTATION SERVICE: Pure functional file rotation and cleanup operations
/// FUNCTIONAL APPROACH: Stateless service with pure functions
/// THREAD-SAFE: No shared state, safe for concurrent operations
/// </summary>
internal sealed class FileRotationService : IFileRotationService
{
    private readonly ILogger? _logger;

    #region Constructor

    public FileRotationService(ILogger? logger = null)
    {
        _logger = logger;
    }

    #endregion

    #region Rotation Operations

    public bool ShouldRotateBySize(string filePath, long maxSizeBytes)
    {
        return Result<bool>.Try(() =>
        {
            if (!File.Exists(filePath) || maxSizeBytes <= 0)
                return false;

            var fileInfo = new FileInfo(filePath);
            return fileInfo.Length >= maxSizeBytes;
        }).ValueOr(false);
    }

    public bool ShouldRotateByAge(string filePath, TimeSpan maxAge)
    {
        return Result<bool>.Try(() =>
        {
            if (!File.Exists(filePath) || maxAge <= TimeSpan.Zero)
                return false;

            var fileInfo = new FileInfo(filePath);
            return DateTime.Now - fileInfo.CreationTime >= maxAge;
        }).ValueOr(false);
    }

    public async Task<Result<RotationResult>> RotateFileAsync(string currentFilePath, string baseFileName, string directory)
    {
        return await Result<RotationResult>.TryAsync(async () =>
        {
            _logger?.LogInformation("Starting file rotation: {CurrentFile}", currentFilePath);

            var timestamp = DateTime.Now;
            string? archivedFilePath = null;
            long archivedFileSize = 0;

            // FUNCTIONAL: Archive current file if it exists
            if (File.Exists(currentFilePath))
            {
                archivedFilePath = GenerateArchiveFileName(currentFilePath, timestamp);
                archivedFileSize = new FileInfo(currentFilePath).Length;

                // FUNCTIONAL: Handle existing archive file
                if (File.Exists(archivedFilePath))
                {
                    File.Delete(archivedFilePath);
                }

                File.Move(currentFilePath, archivedFilePath);
                _logger?.LogInformation("Archived file: {OriginalFile} -> {ArchivedFile}",
                    Path.GetFileName(currentFilePath), Path.GetFileName(archivedFilePath));
            }

            // FUNCTIONAL: Create new log file
            var newFilePath = GenerateRotationFileName(baseFileName, directory, timestamp);
            await CreateNewLogFileAsync(newFilePath);

            _logger?.LogInformation("Created new log file: {NewFile}", Path.GetFileName(newFilePath));

            return RotationResult.Success(archivedFilePath, newFilePath, archivedFileSize);
        });
    }

    public async Task<Result<RotationResult>> ArchiveAndCreateNewAsync(string currentFile, string newFile)
    {
        return await Result<RotationResult>.TryAsync(async () =>
        {
            var timestamp = DateTime.Now;
            string? archivedFile = null;
            long archivedSize = 0;

            // FUNCTIONAL: Archive current file
            if (File.Exists(currentFile))
            {
                archivedFile = GenerateArchiveFileName(currentFile, timestamp);
                archivedSize = new FileInfo(currentFile).Length;

                if (File.Exists(archivedFile))
                {
                    File.Delete(archivedFile);
                }

                File.Move(currentFile, archivedFile);
            }

            // FUNCTIONAL: Create new file
            await CreateNewLogFileAsync(newFile);

            return RotationResult.Success(archivedFile, newFile, archivedSize);
        });
    }

    #endregion

    #region Cleanup Operations

    public async Task<Result<CleanupResult>> CleanupOldFilesAsync(string directory, string baseFileName, int maxFiles)
    {
        return await Result<CleanupResult>.TryAsync(async () =>
        {
            await Task.Yield();

            _logger?.LogInformation("Starting cleanup: directory={Directory}, baseFileName={BaseFileName}, maxFiles={MaxFiles}",
                directory, baseFileName, maxFiles);

            if (!Directory.Exists(directory) || maxFiles <= 0)
            {
                return CleanupResult.Success(0, 0);
            }

            // FUNCTIONAL: Get files sorted by creation time (oldest first)
            var pattern = $"{baseFileName}*{LoggerConstants.LogFileExtension}";
            var files = Directory.GetFiles(directory, pattern)
                .Select(f => new FileInfo(f))
                .OrderBy(f => f.CreationTime)
                .ToList();

            // FUNCTIONAL: Keep only the most recent files
            var filesToDelete = files.Take(Math.Max(0, files.Count - maxFiles)).ToList();

            var deletedFiles = new List<string>();
            long bytesFreed = 0;

            foreach (var fileInfo in filesToDelete)
            {
                try
                {
                    bytesFreed += fileInfo.Length;
                    deletedFiles.Add(fileInfo.Name);
                    File.Delete(fileInfo.FullName);

                    _logger?.LogInformation("Deleted old log file: {FileName} ({Size} bytes)",
                        fileInfo.Name, fileInfo.Length);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to delete log file: {FileName}", fileInfo.Name);
                }
            }

            return CleanupResult.Success(deletedFiles.Count, bytesFreed, deletedFiles.AsReadOnly());
        });
    }

    public async Task<Result<CleanupResult>> CleanupByAgeAsync(string directory, TimeSpan maxAge)
    {
        return await Result<CleanupResult>.TryAsync(async () =>
        {
            await Task.Yield();

            _logger?.LogInformation("Starting age-based cleanup: directory={Directory}, maxAge={MaxAge}",
                directory, maxAge);

            if (!Directory.Exists(directory) || maxAge <= TimeSpan.Zero)
            {
                return CleanupResult.Success(0, 0);
            }

            var cutoffDate = DateTime.Now - maxAge;
            var files = Directory.GetFiles(directory, LoggerConstants.LogFilePattern)
                .Select(f => new FileInfo(f))
                .Where(f => f.CreationTime < cutoffDate)
                .ToList();

            var deletedFiles = new List<string>();
            long bytesFreed = 0;

            foreach (var fileInfo in files)
            {
                try
                {
                    bytesFreed += fileInfo.Length;
                    deletedFiles.Add(fileInfo.Name);
                    File.Delete(fileInfo.FullName);

                    _logger?.LogInformation("Deleted old log file: {FileName} (age: {Age} days)",
                        fileInfo.Name, (DateTime.Now - fileInfo.CreationTime).TotalDays);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to delete log file: {FileName}", fileInfo.Name);
                }
            }

            return CleanupResult.Success(deletedFiles.Count, bytesFreed, deletedFiles.AsReadOnly());
        });
    }

    public async Task<Result<IReadOnlyList<LogFileInfo>>> GetRotatedFilesAsync(string directory, string baseFileName)
    {
        return await Result<IReadOnlyList<LogFileInfo>>.TryAsync(async () =>
        {
            await Task.Yield();

            if (!Directory.Exists(directory))
            {
                return Array.Empty<LogFileInfo>().ToList().AsReadOnly();
            }

            var pattern = $"{baseFileName}*{LoggerConstants.LogFileExtension}";
            return Directory.GetFiles(directory, pattern)
                .Select(file => LogFileInfo.FromFileInfo(new FileInfo(file)))
                .OrderByDescending(f => f.ModifiedTime)
                .ToList()
                .AsReadOnly();
        });
    }

    #endregion

    #region Utilities

    public string GenerateRotationFileName(string baseFileName, string directory, DateTime timestamp)
    {
        var fileName = $"{baseFileName}_{timestamp.ToString(LoggerConstants.RotationTimestampFormat)}{LoggerConstants.LogFileExtension}";
        return Path.Combine(directory, fileName);
    }

    public int GetNextSequenceNumber(string directory, string baseFileName)
    {
        return Result<int>.Try(() =>
        {
            if (!Directory.Exists(directory))
                return 1;

            var pattern = $"{baseFileName}_*{LoggerConstants.LogFileExtension}";
            var files = Directory.GetFiles(directory, pattern);

            return files.Length + 1;
        }).ValueOr(1);
    }

    public async Task<Result<long>> CalculateTotalSizeAsync(string directory, string baseFileName)
    {
        return await Result<long>.TryAsync(async () =>
        {
            await Task.Yield();

            if (!Directory.Exists(directory))
                return 0L;

            var pattern = $"{baseFileName}*{LoggerConstants.LogFileExtension}";
            return Directory.GetFiles(directory, pattern)
                .Select(file => new FileInfo(file).Length)
                .Sum();
        });
    }

    #endregion

    #region Private Methods

    private string GenerateArchiveFileName(string originalFilePath, DateTime timestamp)
    {
        var directory = Path.GetDirectoryName(originalFilePath) ?? string.Empty;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFilePath);
        var archiveFileName = $"{fileNameWithoutExtension}_{timestamp.ToString(LoggerConstants.RotationTimestampFormat)}{LoggerConstants.ArchiveFileExtension}";

        return Path.Combine(directory, archiveFileName);
    }

    private async Task CreateNewLogFileAsync(string filePath)
    {
        var header = $"# Log file created: {DateTime.Now.ToString(LoggerConstants.DefaultDateFormat)}{Environment.NewLine}";
        await File.WriteAllTextAsync(filePath, header, LoggerConstants.DefaultEncoding);
    }

    #endregion
}