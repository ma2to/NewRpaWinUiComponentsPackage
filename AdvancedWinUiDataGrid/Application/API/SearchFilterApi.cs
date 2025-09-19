using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.API;

/// <summary>
/// PUBLIC API: Filter operators for data filtering
/// </summary>
public enum FilterOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Contains,
    NotContains,
    StartsWith,
    EndsWith,
    IsNull,
    IsNotNull,
    IsEmpty,
    IsNotEmpty,
    Regex
}

/// <summary>
/// PUBLIC API: Logic operators for filter combinations
/// </summary>
public enum FilterLogicOperator
{
    And,
    Or
}

/// <summary>
/// PUBLIC API: Search scope definition
/// </summary>
public enum SearchScope
{
    AllData,
    VisibleData,
    SelectedData
}

/// <summary>
/// PUBLIC API: Filter definition for complex filtering
/// </summary>
public sealed record FilterDefinition
{
    public string? ColumnName { get; init; }
    public FilterOperator Operator { get; init; }
    public object? Value { get; init; }
    public object? SecondValue { get; init; }
    public FilterLogicOperator LogicOperator { get; init; } = FilterLogicOperator.And;
    public string? FilterName { get; init; }

    public static FilterDefinition Equals(string columnName, object value) =>
        new() { ColumnName = columnName, Operator = FilterOperator.Equals, Value = value };

    public static FilterDefinition Contains(string columnName, string value) =>
        new() { ColumnName = columnName, Operator = FilterOperator.Contains, Value = value };

    public static FilterDefinition GreaterThan(string columnName, object value) =>
        new() { ColumnName = columnName, Operator = FilterOperator.GreaterThan, Value = value };

    public static FilterDefinition Regex(string columnName, string pattern) =>
        new() { ColumnName = columnName, Operator = FilterOperator.Regex, Value = pattern };
}

/// <summary>
/// PUBLIC API: Advanced search criteria
/// </summary>
public sealed record AdvancedSearchCriteria
{
    public string SearchText { get; init; } = string.Empty;
    public string[]? TargetColumns { get; init; }
    public bool UseRegex { get; init; }
    public bool CaseSensitive { get; init; }
    public SearchScope Scope { get; init; } = SearchScope.AllData;
    public int? MaxMatches { get; init; }
    public TimeSpan? Timeout { get; init; } = TimeSpan.FromSeconds(5);
}

/// <summary>
/// PUBLIC API: Search result information
/// </summary>
public sealed record SearchResult
{
    public int RowIndex { get; init; }
    public string ColumnName { get; init; } = string.Empty;
    public object? Value { get; init; }
    public string? MatchedText { get; init; }
    public int MatchStartIndex { get; init; }
    public int MatchLength { get; init; }

    public static SearchResult Create(int rowIndex, string columnName, object? value, string? matchedText = null) =>
        new()
        {
            RowIndex = rowIndex,
            ColumnName = columnName,
            Value = value,
            MatchedText = matchedText
        };
}

/// <summary>
/// PUBLIC API: Filter result with statistics
/// </summary>
public sealed record FilterResult
{
    public int TotalRowsProcessed { get; init; }
    public int MatchingRows { get; init; }
    public int FilteredOutRows { get; init; }
    public TimeSpan ProcessingTime { get; init; }
    public IReadOnlyList<int> MatchingRowIndices { get; init; } = Array.Empty<int>();

    public static FilterResult Create(int total, int matching, TimeSpan processingTime, IReadOnlyList<int>? matchingIndices = null) =>
        new()
        {
            TotalRowsProcessed = total,
            MatchingRows = matching,
            FilteredOutRows = total - matching,
            ProcessingTime = processingTime,
            MatchingRowIndices = matchingIndices ?? Array.Empty<int>()
        };
}