using System;
using System.Collections.Generic;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Core.ValueObjects;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Core.Enums;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Core.Interfaces;

/// <summary>
/// CORE: Base interface for all validation rules
/// POLYMORPHISM: Enables unified handling of different validation types
/// </summary>
internal interface IValidationRule
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
/// TYPE 1: Single cell validation rule
/// </summary>
internal interface ISingleCellValidationRule : IValidationRule
{
    /// <summary>Column name to validate</summary>
    string ColumnName { get; }

    /// <summary>Validation function for single cell value</summary>
    Func<object?, bool> Validator { get; }
}

/// <summary>
/// TYPE 2&3: Cross-column validation rule (same row)
/// </summary>
internal interface ICrossColumnValidationRule : IValidationRule
{
    /// <summary>Columns that this rule depends on</summary>
    string[] DependentColumns { get; }

    /// <summary>Validation function for row data</summary>
    Func<IReadOnlyDictionary<string, object?>, ValidationResult> Validator { get; }
}

/// <summary>
/// TYPE 4: Cross-row validation rule
/// </summary>
internal interface ICrossRowValidationRule : IValidationRule
{
    /// <summary>Validation function for entire dataset</summary>
    Func<IReadOnlyList<IReadOnlyDictionary<string, object?>>, IReadOnlyList<ValidationResult>> Validator { get; }
}

/// <summary>
/// TYPE 6: Conditional validation rule
/// </summary>
internal interface IConditionalValidationRule : IValidationRule
{
    /// <summary>Column name to validate when condition is met</summary>
    string ColumnName { get; }

    /// <summary>Condition that determines if validation should run</summary>
    Func<IReadOnlyDictionary<string, object?>, bool> Condition { get; }

    /// <summary>Validation rule to apply when condition is true</summary>
    ISingleCellValidationRule ValidationRule { get; }
}

/// <summary>
/// TYPE 5&7: Complex validation rule (cross-row & cross-column)
/// </summary>
internal interface IComplexValidationRule : IValidationRule
{
    /// <summary>Validation function for entire dataset with complex logic</summary>
    Func<IReadOnlyList<IReadOnlyDictionary<string, object?>>, ValidationResult> Validator { get; }
}