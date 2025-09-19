using System;
using System.Collections.Generic;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Core.ValueObjects;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Core.Interfaces;

/// <summary>
/// INTERFACE: Complex validation rule for cross-row and cross-column validation
/// ENTERPRISE: Supports complex business rules across entire dataset
/// </summary>
internal interface IComplexValidationRule : IValidationRule
{
    /// <summary>Validation function for entire dataset</summary>
    Func<IReadOnlyList<IReadOnlyDictionary<string, object?>>, ValidationResult> Validator { get; }
}

/// <summary>
/// INTERFACE: Cross-row validation rule for multiple row validation
/// ENTERPRISE: Supports uniqueness, totals, and cross-row business rules
/// </summary>
internal interface ICrossRowValidationRule : IValidationRule
{
    /// <summary>Validation function for multiple rows</summary>
    Func<IReadOnlyList<IReadOnlyDictionary<string, object?>>, IReadOnlyList<ValidationResult>> Validator { get; }
}

/// <summary>
/// INTERFACE: Cross-column validation rule for same row validation
/// ENTERPRISE: Supports validation across multiple columns in same row
/// </summary>
internal interface ICrossColumnValidationRule : IValidationRule
{
    /// <summary>Dependent columns that trigger this rule</summary>
    string[] DependentColumns { get; }

    /// <summary>Validation function for row data</summary>
    Func<IReadOnlyDictionary<string, object?>, ValidationResult> Validator { get; }
}

/// <summary>
/// INTERFACE: Conditional validation rule - validates only if condition is met
/// ENTERPRISE: Supports conditional business logic validation
/// </summary>
internal interface IConditionalValidationRule : IValidationRule
{
    /// <summary>Column name to validate</summary>
    string ColumnName { get; }

    /// <summary>Condition that must be true to trigger validation</summary>
    Func<IReadOnlyDictionary<string, object?>, bool> Condition { get; }

    /// <summary>Validation rule to apply if condition is met</summary>
    ISingleCellValidationRule ValidationRule { get; }
}

/// <summary>
/// INTERFACE: Single cell validation rule
/// ENTERPRISE: Basic validation for individual cell values
/// </summary>
internal interface ISingleCellValidationRule : IValidationRule
{
    /// <summary>Column name to validate</summary>
    string ColumnName { get; }

    /// <summary>Validation function for cell value</summary>
    Func<object?, bool> Validator { get; }
}