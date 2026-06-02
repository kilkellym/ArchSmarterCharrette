# Project: [Project Name] — Revit Add-in

## Overview

This is a C# Revit add-in built with Visual Studio using the ArchSmarter add-in template. It targets Revit 2025 by default (net8.0-windows) and supports Revit 2020 through 2026 via multi-configuration builds.

Replace this section with a brief description of what this specific add-in does.

## Project Structure

```
[ProjectName]/
├── App.cs                  # IExternalApplication - ribbon setup on Revit startup
├── Command1.cs             # IExternalCommand - entry point for a ribbon button
├── Helpers/
│   ├── ButtonDataClass.cs  # Wraps PushButtonData creation including icons and availability
│   ├── CommandAvailability.cs  # Controls when ribbon buttons are enabled
│   ├── GlobalUsing.cs      # Global using statements for the entire project
│   └── Utils.cs            # Ribbon panel creation helpers
├── UI/
│   ├── MyWindow.xaml       # WPF window markup
│   ├── MyWindow.xaml.cs    # Code-behind - minimal logic, wires up ViewModel
│   └── MyWindowViewModel.cs  # Business logic, data binding, INotifyPropertyChanged
├── Properties/
│   └── Resources.resx      # Embedded button icon resources
├── Resources/              # Source PNG files for button icons (16x16 and 32x32)
└── RegisterAddin.addin     # Revit add-in manifest, copied to Addins folder on build
```

## Build System

This project uses a multi-configuration build system. Each configuration targets a specific Revit version.

- Configurations follow the pattern `Debug R25`, `Release R25`, `Debug R26`, etc.
- R20 through R24 target `net48` (.NET Framework 4.8)
- R25 and R26 target `net8.0-windows`
- The active Revit version is derived from the configuration name at build time via `$(RevitVersion)`
- The NuGet package `Revit_All_Main_Versions_API_x64` provides Revit API references. Do not reference Revit DLLs directly from the install path.
- Default target is R25 (`net8.0-windows`) unless otherwise specified

On post-build, the DLL and .addin manifest are copied to:
`%AppData%\Autodesk\REVIT\Addins\[RevitVersion]\[ProjectName]\`

To debug, set the build configuration to the target Revit version and launch Revit via F5. The project is pre-configured to start `Revit.exe` with `/language ENG`.

## C# Conventions

- PascalCase for classes, methods, and properties
- camelCase for local variables and method parameters
- Prefix private fields with underscore: `_myField`
- Use `var` only when the type is obvious from the right side
- One class per file, named to match the class
- Global usings are declared in `Helpers/GlobalUsing.cs` - do not add redundant using statements in individual files

## Revit API Rules

These are critical. Violations cause runtime failures or silent data loss.

- **All model modifications require a transaction.** Always use:
  `using (Transaction t = new Transaction(doc, "Description")) { t.Start(); /* changes */ t.Commit(); }`
- Use `TransactionGroup` when multiple transactions need to appear as a single undo step
- Never modify the model outside a transaction
- `FilteredElementCollector` requires a category or class filter. Always add one early in the chain
- Internal Revit units are decimal feet. Convert using `UnitUtils.ConvertToInternalUnits()` and `UnitUtils.ConvertFromInternalUnits()`
- Use `BuiltInCategory` and `BuiltInParameter` enums instead of string matching
- Always null-check Parameters before reading: `param?.AsString()`
- Do not store `Element` references across transactions. Store `ElementId` values instead
- `Element.Name` is a property, not a method
- `Parameter.AsString()` returns the raw stored value; `Parameter.AsValueString()` returns the formatted display string
- Use `ElementId.InvalidElementId` not `ElementId.Invalid`
- Access selection via `uidoc.Selection` (namespace `Autodesk.Revit.UI.Selection`)

## WPF and MVVM Pattern

The project uses a variant of MVVM where the ViewModel holds business logic and the code-behind is kept minimal.

- Windows live in the `UI/` folder
- Each window has a corresponding ViewModel class (e.g. `MyWindow.xaml.cs` + `MyWindowViewModel.cs`)
- The ViewModel is instantiated in the code-behind constructor and set as `DataContext`
- `doc` and `uidoc` are passed into the window constructor and forwarded to the ViewModel
- Keep Revit API calls out of ViewModels where possible. Collect data before opening the window and pass it in
- ViewModels implement `INotifyPropertyChanged` using `[CallerMemberName]`
- Close the WPF window before starting any Revit transaction
- Show WPF windows with `.ShowDialog()` from within the `Execute()` method of a command

Example of showing a window from a command:
```csharp
MyWindow window = new MyWindow(doc, uidoc);
window.ShowDialog();
```

## Ribbon and Command Structure

- `App.cs` sets up the ribbon tab and panel in `OnStartup()` using `Helper.Utils.CreateRibbonPanel()`
- Each command class implements `IExternalCommand` and also contains a static `GetButtonData()` method
- Button icons are embedded resources stored in `Properties/Resources.resx` and accessed via `Properties.Resources`
- `CommandAvailability` controls whether buttons are enabled. Modify it to restrict commands to specific contexts (e.g. active document required)
- The `.addin` manifest is named `RegisterAddin.addin` in the project and renamed to `[ProjectName].addin` on copy

## Common Task Recipes

### Add a new command

1. Copy `Command1.cs` and rename the file and class (e.g. `RenameCommand.cs` / `RenameCommand`)
2. Update `buttonInternalName`, `buttonTitle`, and tooltip inside `GetButtonData()`
3. Choose a button icon from `Properties.Resources` or add a new PNG to `Resources/` and embed it
4. In `App.cs`, call `YourCommand.GetButtonData()` and pass the result to `panel.AddItem()`

### Add a new WPF window

1. Copy the three files in `UI/` and rename them (e.g. `SettingsWindow.xaml`, `SettingsWindow.xaml.cs`, `SettingsWindowViewModel.cs`)
2. Update the namespace and class names in all three files
3. Add the properties and bindings you need to the ViewModel
4. Instantiate and show the window from your command: `new SettingsWindow(doc, uidoc).ShowDialog()`

### Add a helper utility

1. Add a new static class to the `Helpers/` folder (e.g. `WallUtils.cs`)
2. Use the namespace `[ProjectName].Helper`
3. Reference it from commands or ViewModels as `Helper.WallUtils.MethodName()`

### Collect elements

```csharp
var walls = new FilteredElementCollector(doc)
    .OfClass(typeof(Wall))
    .WhereElementIsNotElementType()
    .Cast<Wall>()
    .ToList();
```

### Modify element parameters

```csharp
using (Transaction t = new Transaction(doc, "Update Parameter"))
{
    t.Start();
    Parameter param = element.LookupParameter("My Parameter");
    if (param != null && !param.IsReadOnly)
        param.Set("New Value");
    t.Commit();
}
```
