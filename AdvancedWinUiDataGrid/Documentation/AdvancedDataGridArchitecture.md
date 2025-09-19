# AdvancedDataGrid Architecture Documentation

## 🏗️ Clean Architecture Overview

Komponenta AdvancedDataGrid je implementovaná podľa **Clean Architecture** princípov s dôrazom na **Single Responsibility Principle** a **Single Using Statement** pre vývojárov.

## 📁 Final Architecture Structure

```
AdvancedWinUiDataGrid/
├── 📁 Application/                    # Application Layer - Use Cases & APIs
│   ├── 📁 API/                        # Public API Definitions
│   │   ├── SearchFilterApi.cs         # Search & Filter types
│   │   ├── DataImportExportApi.cs     # Import/Export types
│   │   ├── SortApi.cs                 # Sort configuration types
│   │   ├── KeyboardShortcutsApi.cs    # Keyboard shortcut types
│   │   ├── PerformanceApi.cs          # Performance & virtualization types
│   │   └── AutoRowHeightApi.cs        # Auto row height types
│   ├── 📁 Interfaces/                 # Service contracts
│   └── 📁 UseCases/                   # Business logic operations
├── 📁 Core/                           # Domain Layer - Business Logic
│   ├── 📁 Entities/                   # Domain entities
│   ├── 📁 ValueObjects/               # Immutable value objects
│   ├── 📁 Enums/                      # Domain enumerations
│   ├── 📁 Interfaces/                 # Core abstractions
│   │   ├── IValidationRules.cs        # Public validation interfaces
│   │   ├── IDataGridLogger.cs         # Logging interface
│   │   └── IComplexValidationRule.cs  # Complex validation interface
│   └── 📁 Constants/                  # Domain constants
├── 📁 Infrastructure/                 # Infrastructure Layer - Services
│   ├── 📁 Logging/                    # Logging implementations
│   ├── 📁 Persistence/                # Data storage implementations
│   └── 📁 Services/                   # Service implementations
│       ├── ValidationService.cs       # Validation logic
│       ├── ValidationRuleImplementations.cs # Internal rule implementations
│       └── SearchFilterService.cs     # Search & filter logic
├── 📁 Presentation/                   # Presentation Layer - UI
│   ├── 📁 UI/                         # UserControl implementations
│   ├── 📁 ViewModels/                 # MVVM ViewModels
│   ├── 📁 Converters/                 # Value converters
│   └── 📁 Themes/                     # UI themes and styles
├── 📁 Tests/                          # Unit & Integration tests
├── 📁 Documentation/                  # Architecture documentation
├── AdvancedDataGrid.cs                # 🎯 SINGLE PUBLIC API ENTRY POINT
└── *.cs.old                          # Archived old implementations
```

## 🎯 Single Using Statement Architecture

### ✅ Developer Experience
```csharp
// SINGLE USING STATEMENT for entire component
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;

// All functionality available through one class
var dataGrid = new AdvancedDataGrid(logger, DataGridOperationMode.UI);
```

### 📊 Public API Surface

**AdvancedDataGrid.cs** - Single point of entry with:
- ✅ **Validation API** - 8-type validation system
- ✅ **Data Management API** - Dictionary & DataTable import/export
- ✅ **Copy/Paste API** - Excel-compatible tab-delimited format
- ✅ **Search & Filter API** - Advanced search with regex
- ✅ **Sort API** - Multi-column sorting
- ✅ **Configuration Properties** - All settings in one place
- ✅ **Data Access** - Read-only data access methods

## 🔧 API Structure Design

### Application/API Layer
Všetky **public typy** potrebné pre API sú organizované v `Application/API/`:

```csharp
// Search & Filter
FilterOperator, FilterDefinition, SearchResult, FilterResult

// Import & Export
ImportMode, ImportOptions, ImportResult, CopyPasteResult

// Sorting
SortDirection, SortConfiguration, SortResult

// Performance
VirtualizationConfiguration, PerformanceStatistics, DataPage

// Auto Row Height
AutoRowHeightConfiguration, TextMeasurementResult

// Keyboard
KeyboardShortcut, KeyboardShortcutConfiguration
```

### Core/Interfaces Layer
**Public validation interfaces** v `Core/Interfaces/`:

```csharp
// Validation interfaces accessible to external developers
IValidationRule, ISingleCellValidationRule, ICrossColumnValidationRule
IConditionalValidationRule, IComplexValidationRule
```

### Infrastructure/Services Layer
**Internal implementations** v `Infrastructure/Services/`:

```csharp
// Internal service implementations
ValidationService, SearchFilterService, ValidationRuleImplementations
```

## 🚀 Key Architectural Principles

### 1. **Single Entry Point**
- ✅ **AdvancedDataGrid.cs** - jediný public API súbor
- ✅ **Single using statement** - `using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;`
- ✅ **No internal namespace pollution** - žiadne internal typy v IntelliSense

### 2. **Clean Architecture Layers**
- ✅ **Application/API** - Public types for external use
- ✅ **Core** - Business logic and interfaces
- ✅ **Infrastructure** - Service implementations
- ✅ **Presentation** - UI components

### 3. **SOLID Principles**
- ✅ **Single Responsibility** - každá trieda má jednu zodpovednosť
- ✅ **Open/Closed** - rozšíriteľné cez interfaces
- ✅ **Liskov Substitution** - interfaces sú substituovateľné
- ✅ **Interface Segregation** - špecializované interfaces
- ✅ **Dependency Inversion** - závislosti cez abstractions

### 4. **API Design**
- ✅ **Standard .NET types** - int, string, bool, DateTime, Dictionary, DataTable
- ✅ **No custom complex types** - v API argumentoch
- ✅ **IntelliSense friendly** - všetko dostupné cez AdvancedDataGrid
- ✅ **Async/await patterns** - moderné asynchronné programovanie

## 📈 Implemented Features

### ✅ Validation System (8 Types)
```csharp
// Single cell validation
await dataGrid.AddSingleCellValidationAsync("Age", value => value is int age && age >= 0, "Age must be positive");

// Cross-column validation
await dataGrid.AddCrossColumnValidationAsync(new[] { "FirstName", "LastName" },
    row => (row.ContainsKey("FirstName") && row.ContainsKey("LastName"), null), "Both names required");

// Conditional validation
await dataGrid.AddConditionalValidationAsync("Salary",
    row => row["Department"]?.ToString() == "Sales",
    value => value is decimal salary && salary > 30000, "Sales salary must be > 30k");
```

### ✅ Data Import/Export (Dictionary & DataTable)
```csharp
// Dictionary import
var data = new[] { new Dictionary<string, object?> { ["Name"] = "John", ["Age"] = 30 } };
await dataGrid.ImportFromDictionaryAsync(data);

// DataTable import
await dataGrid.ImportFromDataTableAsync(dataTable);

// Excel-compatible copy/paste (tab-delimited)
var copyResult = await dataGrid.CopyToClipboardAsync();
await dataGrid.PasteFromClipboardAsync(clipboardData);
```

### ✅ Search & Filter
```csharp
// Advanced search with regex
var searchResults = await dataGrid.SearchAsync(new AdvancedSearchCriteria
{
    SearchText = "John.*Developer",
    UseRegex = true
});

// Business logic filters
var filterResults = await dataGrid.ApplyFiltersAsync(new[]
{
    FilterDefinition.GreaterThan("Age", 25),
    FilterDefinition.Contains("Department", "Engineering")
});
```

### ✅ Multi-Column Sorting
```csharp
// Single column sort
await dataGrid.SortByColumnAsync("Name", SortDirection.Ascending);

// Toggle column sort (for UI clicks)
await dataGrid.ToggleColumnSortAsync("Salary");

// Clear all sorting
dataGrid.ClearAllSorts();
```

### ✅ Configuration
```csharp
// Virtualization for large datasets
dataGrid.VirtualizationConfiguration = VirtualizationConfiguration.MassiveDataset;

// Sort configuration
dataGrid.SortConfiguration = SortConfiguration.Default;

// Auto row height for multiline text
dataGrid.AutoRowHeightConfiguration = AutoRowHeightConfiguration.Spacious;

// Keyboard shortcuts
dataGrid.KeyboardShortcutConfiguration = KeyboardShortcutConfiguration.CreateDefault();
```

## 🔍 Development Workflow

### For External Developers:
1. **Single using statement**: `using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;`
2. **IntelliSense discovery**: Everything cez `AdvancedDataGrid` class
3. **Standard .NET types**: Žiadne custom typy v API
4. **Async/await patterns**: Moderné asynchronné volania

### For Internal Development:
1. **Clean separation**: API types v `Application/API`
2. **Business logic**: V `Core` layer
3. **Implementations**: V `Infrastructure` layer
4. **UI components**: V `Presentation` layer
5. **No God classes**: Každá trieda má jednu zodpovednosť

## 📊 Performance Characteristics

- ✅ **Virtualization**: Podpora pre 10M+ rows
- ✅ **Memory management**: Inteligentné cachovanie
- ✅ **Background processing**: Non-blocking operácie
- ✅ **Progress reporting**: Real-time feedback
- ✅ **Timeout protection**: 2-second default pre validation

## 🎯 Migration Benefits

### Before (Old Architecture):
- ❌ Multiple using statements needed
- ❌ Internal namespaces in IntelliSense
- ❌ God classes with mixed responsibilities
- ❌ Complex type dependencies
- ❌ Namespace pollution

### After (New Architecture):
- ✅ Single using statement: `using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;`
- ✅ Clean IntelliSense experience
- ✅ SOLID principles throughout
- ✅ Standard .NET types only
- ✅ Clean namespace structure

---

*AdvancedDataGrid v2.0 - Clean Architecture Implementation*
*Single Using Statement • Standard .NET Types • SOLID Principles*