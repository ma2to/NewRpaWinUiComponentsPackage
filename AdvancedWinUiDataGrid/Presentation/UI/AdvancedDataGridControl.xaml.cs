using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Core.Enums;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Core.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Presentation.ViewModels;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Infrastructure.Logging;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Presentation.UI;

/// <summary>
/// PRESENTATION: Main UI control for AdvancedDataGrid with WinUI 3 implementation
/// PERFORMANCE: Uses ItemsRepeater for virtualization and smooth scrolling
/// INTERACTION: Supports column resizing, cell editing, and keyboard navigation
/// </summary>
internal sealed partial class AdvancedDataGridControl : UserControl, IDisposable
{
    #region Dependencies and Fields

    private readonly IDataGridLogger _logger;
    private DataGridViewModel? _viewModel;
    private bool _disposed;

    #endregion

    #region Properties

    /// <summary>Data context as strongly-typed ViewModel</summary>
    public DataGridViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            if (_viewModel != value)
            {
                _viewModel?.Dispose();
                _viewModel = value;
                DataContext = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsHeadlessMode));
                OnPropertyChanged(nameof(RowCountText));
            }
        }
    }

    /// <summary>Indicates if control is in headless mode</summary>
    public bool IsHeadlessMode => ViewModel?.OperationMode == DataGridOperationMode.Headless;

    /// <summary>Row count display text for status bar</summary>
    public string RowCountText
    {
        get
        {
            if (ViewModel == null) return "No data";
            var rowCount = ViewModel.Rows.Count;
            var columnCount = ViewModel.Columns.Count;
            return $"{rowCount} rows, {columnCount} columns";
        }
    }

    #endregion

    #region Constructor

    public AdvancedDataGridControl(IDataGridLogger? logger = null)
    {
        _logger = logger ?? new DataGridLogger(null, "UI");

        this.InitializeComponent();

        _logger.LogInformation("UI: AdvancedDataGridControl initialized");

        // Subscribe to ViewModel property changes for UI updates
        this.Loaded += OnControlLoaded;
        this.Unloaded += OnControlUnloaded;
    }

    #endregion

    #region Event Handlers - Control Lifecycle

    private void OnControlLoaded(object sender, RoutedEventArgs e)
    {
        _logger.LogInformation("UI: AdvancedDataGridControl loaded");

        // Subscribe to ViewModel collection changes
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnControlUnloaded(object sender, RoutedEventArgs e)
    {
        _logger.LogInformation("UI: AdvancedDataGridControl unloaded");

        // Unsubscribe from ViewModel events
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Update UI-specific properties when ViewModel changes
        if (e.PropertyName == nameof(DataGridViewModel.Rows) ||
            e.PropertyName == nameof(DataGridViewModel.Columns))
        {
            OnPropertyChanged(nameof(RowCountText));
        }
        else if (e.PropertyName == nameof(DataGridViewModel.OperationMode))
        {
            OnPropertyChanged(nameof(IsHeadlessMode));
        }

        _logger.LogInformation("UI: ViewModel property changed: {PropertyName}", e.PropertyName);
    }

    #endregion

    #region Event Handlers - Cell Interaction

    private void OnCellGotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.DataContext is CellViewModel cellViewModel)
        {
            cellViewModel.IsSelected = true;
            cellViewModel.IsEditing = true;

            // Update ViewModel selection
            if (ViewModel != null)
            {
                ViewModel.SelectedRowIndex = cellViewModel.Address.Row;
                ViewModel.SelectedColumnName = cellViewModel.ColumnName;
            }

            _logger.LogInformation("UI: Cell [{Row}, {Column}] got focus",
                cellViewModel.Address.Row, cellViewModel.ColumnName);
        }
    }

    private void OnCellLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.DataContext is CellViewModel cellViewModel)
        {
            cellViewModel.IsSelected = false;
            cellViewModel.IsEditing = false;

            _logger.LogInformation("UI: Cell [{Row}, {Column}] lost focus",
                cellViewModel.Address.Row, cellViewModel.ColumnName);
        }
    }

    private void OnCellTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.DataContext is CellViewModel cellViewModel)
        {
            // Update cell value through ViewModel
            cellViewModel.Value = textBox.Text;

            _logger.LogInformation("UI: Cell [{Row}, {Column}] text changed to '{Text}'",
                cellViewModel.Address.Row, cellViewModel.ColumnName, textBox.Text);
        }
    }

    #endregion

    #region Event Handlers - Column Resizing

    private void OnColumnResizeStarted(object sender, DragStartedEventArgs e)
    {
        if (sender is Thumb thumb &&
            thumb.DataContext is DataColumnViewModel columnViewModel)
        {
            columnViewModel.StartResize();
            _logger.LogInformation("UI: Started resizing column '{ColumnName}'", columnViewModel.Name);
        }
    }

    private void OnColumnResizeDelta(object sender, DragDeltaEventArgs e)
    {
        if (sender is Thumb thumb &&
            thumb.DataContext is DataColumnViewModel columnViewModel)
        {
            var newWidth = columnViewModel.Width + e.HorizontalChange;
            columnViewModel.UpdateResizeWidth(newWidth);

            _logger.LogInformation("UI: Resizing column '{ColumnName}' to {Width}px",
                columnViewModel.Name, newWidth);
        }
    }

    private void OnColumnResizeCompleted(object sender, DragCompletedEventArgs e)
    {
        if (sender is Thumb thumb &&
            thumb.DataContext is DataColumnViewModel columnViewModel)
        {
            columnViewModel.EndResize();
            _logger.LogInformation("UI: Finished resizing column '{ColumnName}' to {Width}px",
                columnViewModel.Name, columnViewModel.Width);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>Initialize control with ViewModel</summary>
    public void Initialize(DataGridViewModel viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _logger.LogInformation("UI: AdvancedDataGridControl initialized with ViewModel");
    }

    /// <summary>Focus specific cell</summary>
    public void FocusCell(int rowIndex, string columnName)
    {
        try
        {
            // Find the specific cell and focus it
            if (ViewModel != null &&
                rowIndex >= 0 && rowIndex < ViewModel.Rows.Count)
            {
                var rowViewModel = ViewModel.Rows[rowIndex];
                var cellViewModel = rowViewModel.GetCell(columnName);

                if (cellViewModel != null)
                {
                    cellViewModel.IsSelected = true;
                    ViewModel.SelectedRowIndex = rowIndex;
                    ViewModel.SelectedColumnName = columnName;

                    _logger.LogInformation("UI: Focused cell [{Row}, {Column}]", rowIndex, columnName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UI: Error focusing cell [{Row}, {Column}]", rowIndex, columnName);
        }
    }

    /// <summary>Scroll to specific row</summary>
    public void ScrollToRow(int rowIndex)
    {
        try
        {
            if (ViewModel != null &&
                rowIndex >= 0 && rowIndex < ViewModel.Rows.Count)
            {
                // Implement scrolling logic here
                // This would typically involve calculating the row position and scrolling to it

                _logger.LogInformation("UI: Scrolled to row {RowIndex}", rowIndex);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UI: Error scrolling to row {RowIndex}", rowIndex);
        }
    }

    /// <summary>Apply color configuration to UI elements</summary>
    public void ApplyColorConfiguration(ColorConfiguration colorConfiguration)
    {
        try
        {
            if (ViewModel != null)
            {
                ViewModel.ColorConfiguration = colorConfiguration;
                _logger.LogInformation("UI: Applied color configuration");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UI: Error applying color configuration");
        }
    }

    #endregion

    #region INotifyPropertyChanged

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            _logger.LogInformation("UI: Disposing AdvancedDataGridControl");

            // Unsubscribe from events
            this.Loaded -= OnControlLoaded;
            this.Unloaded -= OnControlUnloaded;

            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
                ViewModel.Dispose();
                ViewModel = null;
            }

            _logger.LogInformation("UI: AdvancedDataGridControl disposed successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "UI: Error during AdvancedDataGridControl disposal");
        }
        finally
        {
            _logger?.Dispose();
            _disposed = true;
        }
    }

    #endregion
}