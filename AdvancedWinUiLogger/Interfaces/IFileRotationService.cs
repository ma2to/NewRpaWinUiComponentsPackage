using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Domain;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Results;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Core.Functional;

namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Interfaces;

/// <summary>
/// 🔗 SERVICE CONTRACT: File rotation operations interface
/// FUNCTIONAL: Pure functions for file rotation logic
/// SOLID: Single responsibility for rotation concerns
/// </summary>
public interface IFileRotationService
{
    #region Rotation Operations

    /// <summary>
    /// FUNCTIONAL: Check if file needs rotation based on size
    /// </summary>
    bool ShouldRotateBySize(string filePath, long maxSizeBytes);

    /// <summary>
    /// FUNCTIONAL: Check if file needs rotation based on age
    /// </summary>
    bool ShouldRotateByAge(string filePath, TimeSpan maxAge);

    /// <summary>
    /// FUNCTIONAL: Perform file rotation
    /// </summary>
    Task<Result<RotationResult>> RotateFileAsync(string currentFilePath, string baseFileName, string directory);

    /// <summary>
    /// FUNCTIONAL: Archive current file and create new one
    /// </summary>
    Task<Result<RotationResult>> ArchiveAndCreateNewAsync(string currentFile, string newFile);

    #endregion

    #region Cleanup Operations

    /// <summary>
    /// FUNCTIONAL: Clean up old rotated files
    /// </summary>
    Task<Result<CleanupResult>> CleanupOldFilesAsync(string directory, string baseFileName, int maxFiles);

    /// <summary>
    /// FUNCTIONAL: Clean up files older than specified age
    /// </summary>
    Task<Result<CleanupResult>> CleanupByAgeAsync(string directory, TimeSpan maxAge);

    /// <summary>
    /// FUNCTIONAL: Get list of rotated files for cleanup
    /// </summary>
    Task<Result<IReadOnlyList<LogFileInfo>>> GetRotatedFilesAsync(string directory, string baseFileName);

    #endregion

    #region Utilities

    /// <summary>
    /// FUNCTIONAL: Generate next rotation file name
    /// </summary>
    string GenerateRotationFileName(string baseFileName, string directory, DateTime timestamp);

    /// <summary>
    /// FUNCTIONAL: Get file sequence number for rotation
    /// </summary>
    int GetNextSequenceNumber(string directory, string baseFileName);

    /// <summary>
    /// FUNCTIONAL: Calculate total size of log files
    /// </summary>
    Task<Result<long>> CalculateTotalSizeAsync(string directory, string baseFileName);

    #endregion
}