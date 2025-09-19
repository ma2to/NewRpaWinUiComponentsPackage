using System;
using System.Collections.Generic;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Core.ValueObjects;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Core.Interfaces;

/// <summary>
/// PUBLIC API: Base interface for all validation rules
/// ENTERPRISE: Enables external validation rule implementations
/// </summary>
public interface IValidationRule
{
    /// <summary>Unique name for the validation rule</summary>
    string? RuleName { get; }

    /// <summary>Error message when validation fails</summary>
    string ErrorMessage { get; }

    /// <summary>Severity level of validation failure</summary>
    ValidationSeverity Severity { get; }

    /// <summary>Priority for rule execution (lower = higher priority)</summary>
    int? Priority { get; }

    /// <summary>Timeout for rule execution</summary>
    TimeSpan? Timeout { get; }

    /// <summary>Effective timeout with fallback to default</summary>
    TimeSpan EffectiveTimeout { get; }
}

/// <summary>
/// PUBLIC API: Single cell validation rule
/// ENTERPRISE: External developers can implement custom single cell validation
/// </summary>
public interface ISingleCellValidationRule : IValidationRule
{
    /// <summary>Column name to validate</summary>
    string ColumnName { get; }

    /// <summary>Validation function for single cell value</summary>
    Func<object?, bool> Validator { get; }
}

/// <summary>
/// PUBLIC API: Cross-column validation rule (same row)
/// ENTERPRISE: External developers can implement custom cross-column validation
/// </summary>
public interface ICrossColumnValidationRule : IValidationRule
{
    /// <summary>Columns that this rule depends on</summary>
    string[] DependentColumns { get; }

    /// <summary>Validation function for row data</summary>
    Func<IReadOnlyDictionary<string, object?>, ValidationResult> Validator { get; }
}

/// <summary>
/// PUBLIC API: Cross-row validation rule
/// ENTERPRISE: External developers can implement custom cross-row validation
/// </summary>
public interface ICrossRowValidationRule : IValidationRule
{
    /// <summary>Validation function for entire dataset</summary>
    Func<IReadOnlyList<IReadOnlyDictionary<string, object?>>, IReadOnlyList<ValidationResult>> Validator { get; }
}

/// <summary>
/// PUBLIC API: Conditional validation rule
/// ENTERPRISE: External developers can implement custom conditional validation
/// </summary>
public interface IConditionalValidationRule : IValidationRule
{
    /// <summary>Column name to validate when condition is met</summary>
    string ColumnName { get; }

    /// <summary>Condition that determines if validation should run</summary>
    Func<IReadOnlyDictionary<string, object?>, bool> Condition { get; }

    /// <summary>Validation rule to apply when condition is true</summary>
    ISingleCellValidationRule ValidationRule { get; }
}

/// <summary>
/// PUBLIC API: Complex validation rule (cross-row & cross-column)
/// ENTERPRISE: External developers can implement custom complex validation
/// </summary>
public interface IComplexValidationRule : IValidationRule
{
    /// <summary>Validation function for entire dataset with complex logic</summary>
    Func<IReadOnlyList<IReadOnlyDictionary<string, object?>>, ValidationResult> Validator { get; }
}