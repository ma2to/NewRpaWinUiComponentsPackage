using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.API;

/// <summary>
/// PUBLIC API: Import mode options
/// </summary>
public enum ImportMode
{
    Replace,
    Append,
    Insert,
    Merge
}

/// <summary>
/// PUBLIC API: Import options for Dictionary and DataTable
/// </summary>
public sealed record ImportOptions
{
    public ImportMode Mode { get; init; } = ImportMode.Replace;
    public int StartRowIndex { get; init; } = 0;
    public bool ValidateBeforeImport { get; init; } = true;
    public bool CreateMissingColumns { get; init; } = true;
    public Dictionary<string, string>? ColumnMapping { get; init; }
    public IProgress<ImportProgress>? Progress { get; init; }

    public static ImportOptions Default => new();
}

/// <summary>
/// PUBLIC API: Import progress information
/// </summary>
public sealed record ImportProgress
{
    public int ProcessedRows { get; init; }
    public int TotalRows { get; init; }
    public double CompletionPercentage => TotalRows > 0 ? (double)ProcessedRows / TotalRows * 100 : 0;
    public TimeSpan ElapsedTime { get; init; }
    public string CurrentOperation { get; init; } = string.Empty;

    public static ImportProgress Create(int processed, int total, TimeSpan elapsed, string operation = "") =>
        new()
        {
            ProcessedRows = processed,
            TotalRows = total,
            ElapsedTime = elapsed,
            CurrentOperation = operation
        };
}

/// <summary>
/// PUBLIC API: Import result with statistics
/// </summary>
public sealed record ImportResult
{
    public bool Success { get; init; }
    public int ImportedRows { get; init; }
    public int SkippedRows { get; init; }
    public int TotalRows { get; init; }
    public TimeSpan ImportTime { get; init; }
    public IReadOnlyList<string> ErrorMessages { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> WarningMessages { get; init; } = Array.Empty<string>();

    public static ImportResult Success(int importedRows, int totalRows, TimeSpan importTime) =>
        new()
        {
            Success = true,
            ImportedRows = importedRows,
            TotalRows = totalRows,
            ImportTime = importTime
        };

    public static ImportResult Failure(IReadOnlyList<string> errors, TimeSpan importTime) =>
        new()
        {
            Success = false,
            ErrorMessages = errors,
            ImportTime = importTime
        };
}

/// <summary>
/// PUBLIC API: Copy/Paste operation result
/// </summary>
public sealed record CopyPasteResult
{
    public bool Success { get; init; }
    public int ProcessedRows { get; init; }
    public string? ClipboardData { get; init; }
    public string? ErrorMessage { get; init; }

    public static CopyPasteResult Success(int processedRows, string? clipboardData = null) =>
        new() { Success = true, ProcessedRows = processedRows, ClipboardData = clipboardData };

    public static CopyPasteResult Failure(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}