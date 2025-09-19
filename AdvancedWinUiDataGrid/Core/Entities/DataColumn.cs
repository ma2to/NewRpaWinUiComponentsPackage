using System;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Core.Enums;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Core.Entities;

/// <summary>
/// DOMAIN ENTITY: Represents a column definition in the data grid
/// SINGLE RESPONSIBILITY: Column metadata and configuration management
/// </summary>
internal sealed class DataColumn
{
    private string _name;
    private double _width;

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Column name cannot be empty", nameof(value));

            var oldName = _name;
            _name = value;
            NameChanged?.Invoke(this, new ColumnNameChangedEventArgs(oldName, value));
        }
    }

    public string OriginalName { get; }

    public double Width
    {
        get => _width;
        set
        {
            if (value < MinWidth) value = MinWidth;
            if (MaxWidth.HasValue && value > MaxWidth.Value) value = MaxWidth.Value;

            if (Math.Abs(_width - value) < 0.01) return;

            var oldWidth = _width;
            _width = value;
            WidthChanged?.Invoke(this, new ColumnWidthChangedEventArgs(oldWidth, value));
        }
    }

    public double MinWidth { get; set; } = 50;
    public double? MaxWidth { get; set; }
    public Type DataType { get; set; } = typeof(string);
    public SpecialColumnType SpecialType { get; set; } = SpecialColumnType.None;
    public int DisplayOrder { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsResizable { get; set; } = true;
    public bool IsSortable { get; set; } = true;

    public bool IsSpecialColumn => SpecialType != SpecialColumnType.None;

    public event EventHandler<ColumnNameChangedEventArgs>? NameChanged;
    public event EventHandler<ColumnWidthChangedEventArgs>? WidthChanged;

    public DataColumn(string name, double initialWidth = 100)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Column name cannot be empty", nameof(name));

        _name = name;
        OriginalName = name;
        _width = Math.Max(initialWidth, MinWidth);
    }

    /// <summary>
    /// ENTERPRISE: Create column with automatic name collision resolution
    /// </summary>
    public static DataColumn CreateWithUniqueNameResolution(string desiredName, Func<string, bool> nameExistsChecker, double initialWidth = 100)
    {
        var uniqueName = GenerateUniqueName(desiredName, nameExistsChecker);
        return new DataColumn(uniqueName, initialWidth);
    }

    /// <summary>
    /// ENTERPRISE: Generate unique column name with _1, _2, etc. suffix
    /// </summary>
    private static string GenerateUniqueName(string baseName, Func<string, bool> nameExists)
    {
        if (!nameExists(baseName))
            return baseName;

        var counter = 1;
        string candidateName;

        do
        {
            candidateName = $"{baseName}_{counter}";
            counter++;
        } while (nameExists(candidateName));

        return candidateName;
    }

    /// <summary>
    /// ENTERPRISE: Reset width to auto-fit content
    /// </summary>
    public void AutoFitWidth(double calculatedWidth)
    {
        Width = Math.Max(calculatedWidth, MinWidth);
    }

    /// <summary>
    /// CONFIGURATION: Configure as special column type
    /// </summary>
    public void ConfigureAsSpecialColumn(SpecialColumnType specialType, double? specialWidth = null)
    {
        SpecialType = specialType;

        if (specialWidth.HasValue)
        {
            Width = specialWidth.Value;
        }
        else
        {
            // Set default widths for special columns
            Width = specialType switch
            {
                SpecialColumnType.CheckBox => 50,
                SpecialColumnType.DeleteRow => 80,
                SpecialColumnType.ValidAlerts => 200,
                SpecialColumnType.RowNumber => 60,
                _ => Width
            };
        }

        // Special columns have specific behavior
        switch (specialType)
        {
            case SpecialColumnType.CheckBox:
                DataType = typeof(bool);
                break;
            case SpecialColumnType.DeleteRow:
                IsReadOnly = true;
                IsSortable = false;
                break;
            case SpecialColumnType.ValidAlerts:
                IsReadOnly = true;
                IsSortable = false;
                DataType = typeof(string);
                break;
            case SpecialColumnType.RowNumber:
                IsReadOnly = true;
                IsSortable = false;
                IsResizable = false;
                DataType = typeof(int);
                break;
        }
    }

    public override string ToString()
    {
        var special = IsSpecialColumn ? $" ({SpecialType})" : "";
        return $"Column[{Name}]: {Width}px{special}";
    }
}

/// <summary>
/// EVENT ARGS: Column name change notification
/// </summary>
internal sealed class ColumnNameChangedEventArgs : EventArgs
{
    public string OldName { get; }
    public string NewName { get; }

    public ColumnNameChangedEventArgs(string oldName, string newName)
    {
        OldName = oldName;
        NewName = newName;
    }
}

/// <summary>
/// EVENT ARGS: Column width change notification
/// </summary>
internal sealed class ColumnWidthChangedEventArgs : EventArgs
{
    public double OldWidth { get; }
    public double NewWidth { get; }

    public ColumnWidthChangedEventArgs(double oldWidth, double newWidth)
    {
        OldWidth = oldWidth;
        NewWidth = newWidth;
    }
}