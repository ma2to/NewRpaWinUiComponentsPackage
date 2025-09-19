using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Input;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.API;

/// <summary>
/// PUBLIC API: Keyboard shortcut definition
/// </summary>
public sealed record KeyboardShortcut
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string KeyCombination { get; init; } = string.Empty;
    public bool IsEnabled { get; init; } = true;

    public static KeyboardShortcut Create(string name, string keyCombo, string description) =>
        new() { Name = name, KeyCombination = keyCombo, Description = description };
}

/// <summary>
/// PUBLIC API: Keyboard shortcut configuration
/// </summary>
public sealed class KeyboardShortcutConfiguration
{
    private readonly Dictionary<string, KeyboardShortcut> _shortcuts = new();

    /// <summary>Enable Excel-like navigation shortcuts</summary>
    public bool EnableNavigationShortcuts { get; set; } = true;

    /// <summary>Enable editing shortcuts</summary>
    public bool EnableEditingShortcuts { get; set; } = true;

    /// <summary>Enable selection shortcuts</summary>
    public bool EnableSelectionShortcuts { get; set; } = true;

    /// <summary>Enable data operation shortcuts</summary>
    public bool EnableDataOperationShortcuts { get; set; } = true;

    /// <summary>All configured shortcuts</summary>
    public IReadOnlyDictionary<string, KeyboardShortcut> Shortcuts => _shortcuts.AsReadOnly();

    /// <summary>Add custom shortcut</summary>
    public KeyboardShortcutConfiguration AddShortcut(KeyboardShortcut shortcut)
    {
        _shortcuts[shortcut.Name] = shortcut;
        return this;
    }

    /// <summary>Create default configuration with Excel-like shortcuts</summary>
    public static KeyboardShortcutConfiguration CreateDefault()
    {
        var config = new KeyboardShortcutConfiguration();

        // Navigation shortcuts
        config.AddShortcut(KeyboardShortcut.Create("MoveUp", "Up", "Move selection up"));
        config.AddShortcut(KeyboardShortcut.Create("MoveDown", "Down", "Move selection down"));
        config.AddShortcut(KeyboardShortcut.Create("MoveLeft", "Left", "Move selection left"));
        config.AddShortcut(KeyboardShortcut.Create("MoveRight", "Right", "Move selection right"));
        config.AddShortcut(KeyboardShortcut.Create("PageUp", "PageUp", "Move page up"));
        config.AddShortcut(KeyboardShortcut.Create("PageDown", "PageDown", "Move page down"));

        // Editing shortcuts
        config.AddShortcut(KeyboardShortcut.Create("Edit", "F2", "Edit selected cell"));
        config.AddShortcut(KeyboardShortcut.Create("Confirm", "Enter", "Confirm edit"));
        config.AddShortcut(KeyboardShortcut.Create("Cancel", "Escape", "Cancel edit"));

        // Selection shortcuts
        config.AddShortcut(KeyboardShortcut.Create("SelectAll", "Ctrl+A", "Select all"));
        config.AddShortcut(KeyboardShortcut.Create("ExtendSelectionUp", "Shift+Up", "Extend selection up"));
        config.AddShortcut(KeyboardShortcut.Create("ExtendSelectionDown", "Shift+Down", "Extend selection down"));

        // Data operation shortcuts
        config.AddShortcut(KeyboardShortcut.Create("Copy", "Ctrl+C", "Copy selected data"));
        config.AddShortcut(KeyboardShortcut.Create("Paste", "Ctrl+V", "Paste data"));
        config.AddShortcut(KeyboardShortcut.Create("Delete", "Delete", "Delete selected rows"));

        return config;
    }
}

/// <summary>
/// PUBLIC API: Keyboard navigation result
/// </summary>
public sealed record KeyboardNavigationResult
{
    public bool Handled { get; init; }
    public string? Action { get; init; }
    public int? NewRowIndex { get; init; }
    public int? NewColumnIndex { get; init; }

    public static KeyboardNavigationResult Handled(string action, int? newRow = null, int? newColumn = null) =>
        new() { Handled = true, Action = action, NewRowIndex = newRow, NewColumnIndex = newColumn };

    public static KeyboardNavigationResult NotHandled => new() { Handled = false };
}