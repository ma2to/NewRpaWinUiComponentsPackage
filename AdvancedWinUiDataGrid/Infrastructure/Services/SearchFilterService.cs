using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.API;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Core.ValueObjects;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Infrastructure.Services;

/// <summary>
/// INTERNAL: Search & Filter service implementation
/// PERFORMANCE: Optimized for large datasets with regex and complex filter support
/// </summary>
internal sealed class SearchFilterService : IDisposable
{
    private readonly ILogger _logger;
    private bool _disposed;

    public SearchFilterService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogInformation("SEARCH_FILTER: Service initialized");
    }

    /// <summary>
    /// ENTERPRISE: Execute advanced search with regex support
    /// PERFORMANCE: Optimized for large datasets with early termination
    /// </summary>
    public async Task<Result<IReadOnlyList<SearchResult>>> SearchAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> dataset,
        AdvancedSearchCriteria criteria)
    {
        try
        {
            _logger.LogInformation("SEARCH: Starting search with pattern '{Pattern}', regex: {UseRegex}",
                criteria.SearchText, criteria.UseRegex);

            var results = new List<SearchResult>();
            var regex = criteria.UseRegex ? new Regex(criteria.SearchText,
                criteria.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase) : null;

            var columnsToSearch = criteria.TargetColumns ?? GetAllColumnNames(dataset);

            for (int rowIndex = 0; rowIndex < dataset.Count; rowIndex++)
            {
                var row = dataset[rowIndex];

                foreach (var columnName in columnsToSearch)
                {
                    if (!row.ContainsKey(columnName)) continue;

                    var cellValue = row[columnName];
                    var searchMatches = await SearchInValue(cellValue, criteria.SearchText, regex, criteria.CaseSensitive);

                    foreach (var match in searchMatches)
                    {
                        results.Add(SearchResult.Create(rowIndex, columnName, cellValue, match.Text));

                        if (criteria.MaxMatches.HasValue && results.Count >= criteria.MaxMatches.Value)
                        {
                            _logger.LogInformation("SEARCH: Reached maximum matches limit ({MaxMatches})", criteria.MaxMatches.Value);
                            return Result<IReadOnlyList<SearchResult>>.Success(results);
                        }
                    }
                }
            }

            _logger.LogInformation("SEARCH: Completed with {MatchCount} matches found", results.Count);
            return Result<IReadOnlyList<SearchResult>>.Success(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SEARCH: Error during search operation");
            return Result<IReadOnlyList<SearchResult>>.Failure("Search operation failed", ex);
        }
    }

    /// <summary>
    /// ENTERPRISE: Apply complex filters with business logic combinations
    /// PERFORMANCE: Optimized filter evaluation with short-circuiting
    /// </summary>
    public async Task<Result<FilterResult>> ApplyFiltersAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> dataset,
        IReadOnlyList<FilterDefinition> filters)
    {
        try
        {
            _logger.LogInformation("FILTER: Applying {FilterCount} filters to {RowCount} rows", filters.Count, dataset.Count);

            var startTime = DateTime.UtcNow;
            var matchingIndices = new List<int>();

            for (int rowIndex = 0; rowIndex < dataset.Count; rowIndex++)
            {
                var row = dataset[rowIndex];
                bool rowMatches = await EvaluateFiltersForRow(row, filters);

                if (rowMatches)
                {
                    matchingIndices.Add(rowIndex);
                }
            }

            var processingTime = DateTime.UtcNow - startTime;
            var result = FilterResult.Create(dataset.Count, matchingIndices.Count, processingTime, matchingIndices);

            _logger.LogInformation("FILTER: Completed in {ProcessingTime}ms, {MatchingRows}/{TotalRows} rows match",
                processingTime.TotalMilliseconds, matchingIndices.Count, dataset.Count);

            return Result<FilterResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FILTER: Error during filter operation");
            return Result<FilterResult>.Failure("Filter operation failed", ex);
        }
    }

    private async Task<bool> EvaluateFiltersForRow(IReadOnlyDictionary<string, object?> row, IReadOnlyList<FilterDefinition> filters)
    {
        if (!filters.Any()) return true;

        bool result = true;
        FilterLogicOperator currentLogic = FilterLogicOperator.And;

        foreach (var filter in filters)
        {
            bool filterResult = await EvaluateSingleFilter(row, filter);

            if (currentLogic == FilterLogicOperator.And)
            {
                result = result && filterResult;
            }
            else
            {
                result = result || filterResult;
            }

            currentLogic = filter.LogicOperator;
        }

        return result;
    }

    private async Task<bool> EvaluateSingleFilter(IReadOnlyDictionary<string, object?> row, FilterDefinition filter)
    {
        if (string.IsNullOrEmpty(filter.ColumnName) || !row.ContainsKey(filter.ColumnName))
            return true;

        var cellValue = row[filter.ColumnName];

        return filter.Operator switch
        {
            FilterOperator.Equals => Equals(cellValue, filter.Value),
            FilterOperator.NotEquals => !Equals(cellValue, filter.Value),
            FilterOperator.Contains => cellValue?.ToString()?.Contains(filter.Value?.ToString() ?? "") == true,
            FilterOperator.NotContains => cellValue?.ToString()?.Contains(filter.Value?.ToString() ?? "") == false,
            FilterOperator.StartsWith => cellValue?.ToString()?.StartsWith(filter.Value?.ToString() ?? "") == true,
            FilterOperator.EndsWith => cellValue?.ToString()?.EndsWith(filter.Value?.ToString() ?? "") == true,
            FilterOperator.GreaterThan => CompareValues(cellValue, filter.Value) > 0,
            FilterOperator.LessThan => CompareValues(cellValue, filter.Value) < 0,
            FilterOperator.GreaterThanOrEqual => CompareValues(cellValue, filter.Value) >= 0,
            FilterOperator.LessThanOrEqual => CompareValues(cellValue, filter.Value) <= 0,
            FilterOperator.IsNull => cellValue == null,
            FilterOperator.IsNotNull => cellValue != null,
            FilterOperator.IsEmpty => string.IsNullOrEmpty(cellValue?.ToString()),
            FilterOperator.IsNotEmpty => !string.IsNullOrEmpty(cellValue?.ToString()),
            FilterOperator.Regex => EvaluateRegexFilter(cellValue, filter.Value?.ToString()),
            _ => true
        };
    }

    private bool EvaluateRegexFilter(object? cellValue, string? pattern)
    {
        if (string.IsNullOrEmpty(pattern) || cellValue == null) return false;

        try
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(cellValue.ToString() ?? "");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("FILTER: Invalid regex pattern '{Pattern}': {Error}", pattern, ex.Message);
            return false;
        }
    }

    private int CompareValues(object? value1, object? value2)
    {
        if (value1 == null && value2 == null) return 0;
        if (value1 == null) return -1;
        if (value2 == null) return 1;

        // Try numeric comparison first
        if (decimal.TryParse(value1.ToString(), out var num1) && decimal.TryParse(value2.ToString(), out var num2))
        {
            return num1.CompareTo(num2);
        }

        // Try date comparison
        if (DateTime.TryParse(value1.ToString(), out var date1) && DateTime.TryParse(value2.ToString(), out var date2))
        {
            return date1.CompareTo(date2);
        }

        // Fall back to string comparison
        return string.Compare(value1.ToString(), value2.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private async Task<IEnumerable<(string Text, int Start, int Length)>> SearchInValue(
        object? cellValue,
        string searchText,
        Regex? regex,
        bool caseSensitive)
    {
        if (cellValue == null || string.IsNullOrEmpty(searchText))
            return Enumerable.Empty<(string, int, int)>();

        var valueText = cellValue.ToString() ?? "";

        if (regex != null)
        {
            var matches = regex.Matches(valueText);
            return matches.Cast<Match>().Select(m => (m.Value, m.Index, m.Length));
        }
        else
        {
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            var index = valueText.IndexOf(searchText, comparison);

            if (index >= 0)
            {
                return new[] { (searchText, index, searchText.Length) };
            }
        }

        return Enumerable.Empty<(string, int, int)>();
    }

    private string[] GetAllColumnNames(IReadOnlyList<IReadOnlyDictionary<string, object?>> dataset)
    {
        if (!dataset.Any()) return Array.Empty<string>();
        return dataset.First().Keys.ToArray();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _logger.LogInformation("SEARCH_FILTER: Service disposed");
        _disposed = true;
    }
}