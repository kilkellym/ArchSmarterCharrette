# ArchSmarter Charrette

An open-source Revit add-in that turns any Revit view into a photorealistic architectural rendering using Google's Gemini AI. Export your active view, pick a style, and get a rendered image back in seconds — all without leaving Revit.

Your API key stays on your machine. ReRender calls the Gemini REST API directly, without an intermediary server.

## Features

- **One-click rendering** — Export the active Revit view and send it to Gemini with a single button click
- **Style controls** — Choose from six categories (Style, Lighting, Material, Background, Entourage, Weather) to art-direct the output
- **Custom directions** — Add free-text instructions that get appended to the prompt
- **Prompt library system** — All style options are defined in editable JSON files, so you can create and share your own rendering vocabularies
- **Themes** — Bundle preset selections across categories into named themes for one-click style changes
- **Render presets** — Save and load your favorite setting combinations
- **Output settings** — Control image size (512 to 4K) and aspect ratio
- **Session gallery** — Side panel showing thumbnails of every image rendered in the current Revit session, with right-click to reuse settings from a previous render
- **Prompt preview** — Inspect the exact prompt text before sending it to the API
- **Non-modal window** — Keep working in Revit while the render window stays open
- **Configurable save location** — Choose where rendered images are saved
- **Dark-themed UI** — Custom WPF controls styled to match modern dark interfaces

## Requirements

- Autodesk Revit 2025 or 2026 (builds for 2020-2024 are defined but target .NET Framework 4.8)
- A [Google Gemini API key](https://aistudio.google.com/apikey) with access to image generation models
- Windows 10/11

## Installation

1. Download the latest release (or build from source — see below)
2. Copy `ReRender.addin` to `%AppData%\Autodesk\REVIT\Addins\2025\`
3. Copy `ReRender.dll` and its dependencies to `%AppData%\Autodesk\REVIT\Addins\2025\ReRender\`
4. Launch Revit. The **ArchSmarter** tab will appear in the ribbon with the **ReRender** button
5. Click **Settings** to enter your Gemini API key

## Building from Source

Open the solution in Visual Studio 2022+ and select the build configuration for your Revit version:

```
dotnet build -c "Debug R25"
```

The post-build step automatically copies the output to the Revit Addins folder. Make sure Revit is closed when building, or the copy will fail because Revit locks the DLL.

Available configurations: `Debug R20` through `Debug R26` (and corresponding `Release` configs). R25/R26 target `net8.0-windows`; R20-R24 target `net48`.

## How It Works

### Rendering Pipeline

```
Revit View ──> PNG Export ──> Gemini API ──> Rendered Image ──> Disk
```

1. **View export** — `ViewExporter` uses the Revit API's `Document.ExportImage()` to write the active view to a temporary PNG at 300 DPI / 2048px
2. **Prompt assembly** — `PromptBuilder` combines a geometry-adherence preamble with the user's style selections and custom directions into a single text prompt
3. **API call** — `GeminiClient` sends the base64-encoded image and prompt to the Gemini REST API (`generateContent` endpoint), requesting both text and image response modalities
4. **Result** — The response is parsed for an `inlineData` image part, decoded from base64, and written to the output folder as a timestamped PNG

### Prompt Construction

Every prompt starts with an adherence preamble that instructs the model to preserve the exact geometry, camera angle, and spatial composition of the source image. Style phrases are appended based on the user's dropdown selections. Only non-default selections contribute text — if you leave a dropdown on its default, it adds nothing to the prompt.

The prompt library is a JSON file containing `PromptPhrase` objects, each with a `Category`, `DisplayName` (what you see in the dropdown), and `Phrase` (the actual text sent to the API). This makes it easy to experiment with prompt engineering without touching code.

### Prompt Library Format

Prompt libraries are JSON files stored in `%AppData%\ArchSmarter\ReRender\PromptLibraries\`. The format supports both phrases and themes:

```json
{
  "Items": [
    { "Category": "Style", "DisplayName": "Default", "Phrase": "" },
    { "Category": "Style", "DisplayName": "Watercolor", "Phrase": "Render in a loose watercolor painting style with soft washes and visible brushstrokes." },
    { "Category": "Lighting", "DisplayName": "Default", "Phrase": "" },
    { "Category": "Lighting", "DisplayName": "Golden Hour", "Phrase": "Use warm golden hour lighting with long shadows and an amber sky." }
  ],
  "Themes": [
    {
      "Name": "Sunset Watercolor",
      "Selections": {
        "Style": "Watercolor",
        "Lighting": "Golden Hour"
      }
    }
  ]
}
```

**Categories:** Style, Lighting, Material, Background, Entourage, Weather. Each category should have a "Default" entry with an empty `Phrase`.

**Themes** map category names to `DisplayName` values. When a user selects a theme, the corresponding dropdowns are set automatically.

## Architecture

```
ReRender/
├── App.cs                          # Ribbon tab and panel setup
├── RenderCommand.cs                # IExternalCommand — exports view, opens render window
├── SettingsCommand.cs              # IExternalCommand — opens settings window
│
├── Data/
│   ├── GeminiClient.cs             # REST API client for Gemini image generation
│   ├── GeminiException.cs          # (in GeminiClient.cs) Error type with raw JSON
│   ├── PromptBuilder.cs            # Assembles prompt text from phrase selections
│   ├── PromptPhrase.cs             # Model: Category + DisplayName + Phrase
│   ├── PromptTheme.cs              # Model: named preset of category selections
│   ├── PromptLibrary.cs            # Model: Items[] + Themes[] for JSON deserialization
│   ├── PromptLibraryManager.cs     # Loads/saves prompt library JSON files
│   ├── RenderPreset.cs             # Model: saved user preset (selections + custom text)
│   ├── RenderPresetManager.cs      # Loads/saves user presets JSON
│   ├── RenderSettings.cs           # Model: all app settings (API key, model, paths)
│   ├── RenderSettingsManager.cs    # Loads/saves app settings JSON
│   ├── GalleryItem.cs              # Wraps rendered image path with thumbnail
│   └── SessionHistory.cs           # Static session state for gallery persistence
│
├── Helpers/
│   ├── ViewExporter.cs             # Exports active Revit view to temp PNG
│   ├── ButtonDataClass.cs          # Ribbon button helper
│   ├── CommandAvailability.cs      # Controls when ribbon buttons are enabled
│   ├── GlobalUsing.cs              # Project-wide using statements
│   └── Utils.cs                    # Ribbon panel creation
│
└── UI/
    ├── RenderWindow.xaml / .cs         # Main render window (WPF)
    ├── RenderWindowViewModel.cs        # Render window logic and state
    ├── SettingsWindow.xaml / .cs        # Settings window (API key, model, paths)
    ├── SettingsWindowViewModel.cs       # Settings window logic
    ├── SavePresetDialog.xaml / .cs      # Preset name input dialog
    └── PromptPreviewWindow.xaml / .cs   # Read-only prompt text viewer
```

### Key Design Decisions

- **MVVM pattern** — ViewModels hold business logic; code-behind handles only WPF-specific wiring (PasswordBox sync, context menu navigation, dialog ownership)
- **No Revit API in the render window** — The view is exported to a temp PNG before the window opens. The render window works entirely with file paths and REST calls, which is why it can be non-modal
- **Static session history** — `SessionHistory` is a static class so rendered image paths persist across window open/close cycles within a single Revit session. It resets when Revit restarts (assembly unload)
- **Settings auto-save** — Every property change in the settings ViewModel writes to disk immediately via `RenderSettingsManager`
- **Editable prompt libraries** — Prompt phrases are data, not code. Users can create, edit, and share library JSON files without rebuilding

## Configuration Files

All configuration is stored under `%AppData%\ArchSmarter\ReRender\`:

| File | Purpose |
|---|---|
| `ReRender.json` | App settings (API key, model, output folder, library path) |
| `ReRender_Presets.json` | Saved render presets |
| `PromptLibraries/*.json` | Prompt library files |

Default rendered images are saved to `%UserProfile%\Pictures\ReRender\` (configurable in Settings).

## License

MIT
