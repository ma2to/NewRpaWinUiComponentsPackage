namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger.Models.Domain;

/// <summary>
/// 📊 DOMAIN VALUE OBJECT: Log file information
/// IMMUTABLE: Represents file metadata and statistics
/// FUNCTIONAL: Pure functions for calculations
/// </summary>
public sealed record LogFileInfo
{
    public string FilePath { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public DateTime CreatedTime { get; init; }
    public DateTime ModifiedTime { get; init; }
    public bool IsActive { get; init; }
    public int LineCount { get; init; }

    /// <summary>
    /// FUNCTIONAL: Calculate file size in MB
    /// </summary>
    public double SizeMB => SizeBytes / (1024.0 * 1024.0);

    /// <summary>
    /// FUNCTIONAL: Get file name without path
    /// </summary>
    public string FileName => Path.GetFileName(FilePath);

    /// <summary>
    /// FUNCTIONAL: Check if file is older than specified days
    /// </summary>
    public bool IsOlderThan(int days) => CreatedTime < DateTime.Now.AddDays(-days);

    /// <summary>
    /// FUNCTIONAL: Check if file exceeds size limit
    /// </summary>
    public bool ExceedsSize(long maxSizeBytes) => SizeBytes > maxSizeBytes;

    /// <summary>
    /// FUNCTIONAL: Create from FileInfo
    /// </summary>
    public static LogFileInfo FromFileInfo(FileInfo fileInfo, bool isActive = false) =>
        new()
        {
            FilePath = fileInfo.FullName,
            SizeBytes = fileInfo.Length,
            CreatedTime = fileInfo.CreationTime,
            ModifiedTime = fileInfo.LastWriteTime,
            IsActive = isActive,
            LineCount = 0 // TODO: Calculate if needed
        };

    /// <summary>
    /// FUNCTIONAL: Create collection from directory
    /// </summary>
    public static IReadOnlyList<LogFileInfo> FromDirectory(string directory, string pattern = "*.log") =>
        Directory.Exists(directory)
            ? Directory.GetFiles(directory, pattern)
                .Select(file => FromFileInfo(new FileInfo(file)))
                .OrderByDescending(f => f.ModifiedTime)
                .ToList()
                .AsReadOnly()
            : Array.Empty<LogFileInfo>().ToList().AsReadOnly();
}