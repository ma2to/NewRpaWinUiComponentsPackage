namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Core.Enums;

/// <summary>
/// CORE: Defines types of special columns with specialized behavior
/// ENTERPRISE: Support for common enterprise grid features
/// </summary>
internal enum SpecialColumnType
{
    /// <summary>Regular data column with text input</summary>
    None = 0,

    /// <summary>Checkbox column for boolean values</summary>
    CheckBox = 1,

    /// <summary>Delete row action column with confirmation</summary>
    DeleteRow = 2,

    /// <summary>Validation alerts display column showing error messages</summary>
    ValidAlerts = 3,

    /// <summary>Row number display column for navigation</summary>
    RowNumber = 4
}