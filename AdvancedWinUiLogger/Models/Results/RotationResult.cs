namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Results;

/// <summary>
/// 📊 OPERATION RESULT: Log file rotation operation result
/// IMMUTABLE: Contains rotation operation details
/// FUNCTIONAL: Factory methods for different outcomes
/// </summary>
public sealed record RotationResult
{
    public bool IsSuccess { get; init; }
    public string? OldFilePath { get; init; }
    public string? NewFilePath { get; init; }
    public long RotatedFileSize { get; init; }
    public DateTime RotationTime { get; init; }
    public string? ErrorMessage { get; init; }

    private RotationResult()
    {
        RotationTime = DateTime.Now;
    }

    /// <summary>
    /// FUNCTIONAL: Create successful rotation result
    /// </summary>
    public static RotationResult Success(string? oldFilePath, string newFilePath, long rotatedFileSize) => new()
    {
        IsSuccess = true,
        OldFilePath = oldFilePath,
        NewFilePath = newFilePath,
        RotatedFileSize = rotatedFileSize,
        RotationTime = DateTime.Now
    };

    /// <summary>
    /// FUNCTIONAL: Create failed rotation result
    /// </summary>
    public static RotationResult Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        RotationTime = DateTime.Now
    };

    /// <summary>
    /// FUNCTIONAL: Get rotated file size in MB
    /// </summary>
    public double RotatedFileSizeMB => RotatedFileSize / (1024.0 * 1024.0);

    /// <summary>
    /// FUNCTIONAL: Check if rotation involved file archiving
    /// </summary>
    public bool HasArchivedFile => !string.IsNullOrEmpty(OldFilePath);

    /// <summary>
    /// FUNCTIONAL: Get summary message
    /// </summary>
    public string GetSummary() => IsSuccess
        ? $"Rotation successful: {(HasArchivedFile ? $"Archived {Path.GetFileName(OldFilePath)}, " : "")}Created {Path.GetFileName(NewFilePath)} ({RotatedFileSizeMB:F2} MB)"
        : $"Rotation failed: {ErrorMessage}";
}