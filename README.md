# ArchSmarter Charrette

An open-source Revit add-in that turns any Revit view into a photorealistic architectural rendering using Google's Gemini AI — and now generates animated videos from those renderings using Google's Veo models. Export your active view, pick a style, and get a rendered image or video back in seconds — all without leaving Revit.

Your API key stays on your machine — it is stored locally in `%AppData%\ArchSmarter\` and is never logged, transmitted to any intermediary, or stored anywhere else. Charrette calls the Google AI APIs directly, with no intermediary server and no telemetry.

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

### Video Generation

- **AI-powered video from renderings** — Turn any rendered image into an animated architectural video using Google's Veo models
- **Camera motion presets** — Choose from preset camera movements to control how the camera moves through the scene
- **Mood, weather, and scene activity controls** — Fine-tune the atmosphere of your video with preset options for mood, weather, and scene activity
- **Multiple Veo models** — Select from Veo 3.1, 3.1 Fast, 3.0, or 2.0 depending on quality and speed needs
- **Resolution and duration** — Generate video up to 4K resolution, 4–8 seconds long, at 16:9 aspect ratio
- **Video gallery** — Browse generated videos within the session

## Requirements

- Autodesk Revit 2025, 2026, or 2027
- A [Google Gemini API key](https://aistudio.google.com/apikey) with access to image generation models
- Windows 10/11

## Installation

1. Download the latest release (or build from source — see below)
2. Copy `ReRender.addin` to `%AppData%\Autodesk\REVIT\Addins\2025\`
3. Copy `ReRender.dll` and its dependencies to `%AppData%\Autodesk\REVIT\Addins\2025\ReRender\`
4. Launch Revit. The **ArchSmarter** tab will appear in the ribbon with the **ReRender** and **Video** buttons
5. Click **Settings** to enter your Gemini API key (used for both rendering and video generation)

## Building from Source

Open the solution in Visual Studio 2022+ and select the build configuration for your Revit version:

```
dotnet build -c "Debug R25"
```

The post-build step automatically copies the output to the Revit Addins folder. Make sure Revit is closed when building, or the copy will fail because Revit locks the DLL.

Available configurations: `Debug R25`, `Debug R26`, and `Debug R27` (and corresponding `Release` configs). R25/R26 target `net8.0-windows`; R27 targets `net10.0-windows`.

## How It Works

### Rendering Pipeline

```
Revit View ──> PNG Export ──> Gemini API ──> Rendered Image ──> Disk
```

1. **View export** — `ViewExporter` uses the Revit API's `Document.ExportImage()` to write the active view to a temporary PNG at 300 DPI / 2048px
2. **Prompt assembly** — `PromptBuilder` combines a geometry-adherence preamble with the user's style selections and custom directions into a single text prompt
3. **API call** — `GeminiClient` sends the base64-encoded image and prompt to the Gemini REST API (`generateContent` endpoint), requesting both text and image response modalities
4. **Result** — The response is parsed for an `inlineData` image part, decoded from base64, and written to the output folder as a timestamped PNG

### Video Pipeline

```
Rendered Image ──> Veo API ──> Poll for completion ──> MP4 ──> Disk
```

1. **Source image** — The user selects a previously rendered image (or any image file) as the starting frame
2. **Prompt assembly** — Camera motion, mood, weather, and scene activity presets are combined into a video generation prompt
3. **API call** — The standalone VideoTool process calls the Google GenAI API using a Veo model, submitting the image and prompt
4. **Polling** — The API returns an operation ID; the tool polls for completion (up to 10 minutes)
5. **Result** — The finished MP4 is saved to the output folder

The video feature runs as a separate standalone process (`ArchSmarterCharrette.VideoTool`) to avoid .NET version conflicts between Revit's runtime and the Google GenAI SDK (which requires .NET 10). The Revit add-in launches the VideoTool via `VideoToolLauncher`, passing the source image path and API key as arguments.

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

The solution has two projects:

### ArchSmarterCharrette (Revit Add-in)

```
ArchSmarterCharrette/
├── App.cs                          # Ribbon tab and panel setup
├── RenderCommand.cs                # IExternalCommand — exports view, opens render window
├── VideoCommand.cs                 # IExternalCommand — launches the video tool
├── SettingsCommand.cs              # IExternalCommand — opens settings window
│
├── Data/
│   ├── GeminiClient.cs             # REST API client for Gemini image generation
│   ├── GeminiVideoClient.cs        # Launches the standalone VideoTool process
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

### ArchSmarterCharrette.VideoTool (Standalone WPF App, .NET 10)

Runs as a separate process to use the `Google.GenAI` SDK without conflicting with Revit's .NET runtime.

```
ArchSmarterCharrette.VideoTool/
├── Data/
│   ├── VideoArgs.cs                    # Parses command-line args from the Revit add-in
│   ├── CameraMotionPreset.cs           # Model: camera movement options
│   ├── MoodPreset.cs                   # Model: mood/atmosphere options
│   ├── WeatherPreset.cs                # Model: weather options
│   ├── SceneActivityPreset.cs          # Model: scene activity options
│   ├── VideoSettings.cs                # Model: video generation settings
│   ├── VideoSettingsManager.cs         # Loads/saves video settings JSON
│   └── VideoGalleryItem.cs             # Wraps generated video path with thumbnail
│
└── UI/
    ├── VideoWindow.xaml / .cs          # Main video generation window
    ├── VideoWindowViewModel.cs         # Video window logic and state
    └── VideoSettingsWindow.xaml / .cs   # Video settings (model, resolution, duration)
```

### Key Design Decisions

- **MVVM pattern** — ViewModels hold business logic; code-behind handles only WPF-specific wiring (PasswordBox sync, context menu navigation, dialog ownership)
- **No Revit API in the render window** — The view is exported to a temp PNG before the window opens. The render window works entirely with file paths and REST calls, which is why it can be non-modal
- **Static session history** — `SessionHistory` is a static class so rendered image paths persist across window open/close cycles within a single Revit session. It resets when Revit restarts (assembly unload)
- **Settings auto-save** — Every property change in the settings ViewModel writes to disk immediately via `RenderSettingsManager`
- **Editable prompt libraries** — Prompt phrases are data, not code. Users can create, edit, and share library JSON files without rebuilding
- **Separate video process** — The video tool runs as a standalone .NET 10 executable because the `Google.GenAI` SDK requires a newer runtime than Revit provides. The add-in launches it via `VideoToolLauncher`, passing the source image and API key as command-line arguments

## Configuration Files

All configuration is stored under `%AppData%\ArchSmarter\`:

| File | Purpose |
|---|---|
| `ReRender\ReRender.json` | Render settings (API key, model, output folder, library path) |
| `ReRender\ReRender_Presets.json` | Saved render presets |
| `ReRender\PromptLibraries/*.json` | Prompt library files |
| `ArchSmarterCharrette.json` | Shared settings including the Gemini API key (used by the video tool) |

Default rendered images are saved to `%UserProfile%\Pictures\ReRender\` (configurable in Settings). Generated videos are saved alongside rendered images.

## License

MIT
