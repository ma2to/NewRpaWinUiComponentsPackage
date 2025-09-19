namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Results;

/// <summary>
/// 📊 OPERATION RESULT: Log cleanup operation result
/// IMMUTABLE: Contains cleanup operation details
/// FUNCTIONAL: Factory methods and calculations
/// </summary>
public sealed record CleanupResult
{
    public bool IsSuccess { get; init; }
    public int FilesDeleted { get; init; }
    public long BytesFreed { get; init; }
    public DateTime CleanupTime { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyList<string> DeletedFiles { get; init; } = Array.Empty<string>();

    private CleanupResult()
    {
        CleanupTime = DateTime.Now;
    }

    /// <summary>
    /// FUNCTIONAL: Create successful cleanup result
    /// </summary>
    public static CleanupResult Success(int filesDeleted, long bytesFreed, IReadOnlyList<string>? deletedFiles = null) => new()
    {
        IsSuccess = true,
        FilesDeleted = filesDeleted,
        BytesFreed = bytesFreed,
        CleanupTime = DateTime.Now,
        DeletedFiles = deletedFiles ?? Array.Empty<string>()
    };

    /// <summary>
    /// FUNCTIONAL: Create failed cleanup result
    /// </summary>
    public static CleanupResult Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        CleanupTime = DateTime.Now
    };

    /// <summary>
    /// FUNCTIONAL: Get freed space in MB
    /// </summary>
    public double BytesFreedMB => BytesFreed / (1024.0 * 1024.0);

    /// <summary>
    /// FUNCTIONAL: Check if any files were cleaned
    /// </summary>
    public bool HasCleanedFiles => FilesDeleted > 0;

    /// <summary>
    /// FUNCTIONAL: Get cleanup summary
    /// </summary>
    public string GetSummary() => IsSuccess
        ? $"Cleanup successful: {FilesDeleted} files deleted, {BytesFreedMB:F2} MB freed"
        : $"Cleanup failed: {ErrorMessage}";

    /// <summary>
    /// FUNCTIONAL: Get average file size deleted
    /// </summary>
    public double AverageFileSizeMB => HasCleanedFiles ? BytesFreedMB / FilesDeleted : 0;
}