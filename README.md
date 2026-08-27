# ArchSmarter Charrette

An open-source Revit add-in that turns any Revit view into a rendered architectural image using Google's Gemini models — and animates those renderings into short videos using Google's Veo models. Export the active view, pick a style, and get an image or video back without leaving Revit.

Your API key stays on your machine. It is stored locally in `%AppData%\ArchSmarter\Charrette\` and is never logged or sent anywhere except Google's API. Charrette calls the Google AI APIs directly — no intermediary server, no telemetry.

Download the latest version here: https://github.com/kilkellym/ArchSmarterCharrette/releases

---

## Features

### Rendering

- **One-click rendering** — Export the active Revit view and send it to Gemini from a single button
- **Style controls** — Six art-direction categories (Style, Lighting, Material, Background, Entourage, Weather) drive the prompt
- **Custom directions** — Free-text instructions appended to the end of the prompt
- **Prompt libraries** — Every style option lives in an editable JSON file, so you can build and share your own rendering vocabularies
- **Themes** — Bundle a selection across categories into a named theme for one-click style changes; the selector shows "(modified)" if you drift from it
- **Presets** — Save and reload your own combinations of selections, custom directions, image size, and aspect ratio
- **Output settings** — Image size (512, 1K, 2K, 4K) and aspect ratio (1:1 through 21:9)
- **Session gallery** — A side panel of everything rendered this Revit session. Right-click any thumbnail to reuse its settings, touch it up, open it, or delete it
- **Touch up** — Send a finished render back to Gemini with a targeted edit ("make the sky overcast") under a prompt that tells the model to leave everything else alone
- **Prompt preview** — Inspect the exact prompt text before spending an API call
- **Non-modal window** — Keep working in Revit with the render window open. Click the ribbon button again to push a fresh view export into the open window
- **Configurable output folder** — Choose where rendered images land
- **Dark-themed WPF UI**

### Video

- **Image-to-video** — Turn a rendered image (or any image on disk) into a short animated clip
- **Camera motion presets** — Twelve movements: Static, Orbit Left/Right, Spiral Down, Zoom In/Out, Pan Left/Right, Tilt Up, Fly Through, Aerial Reveal, Dolly Forward
- **Custom motion text** — Free-text appended to the camera instruction
- **Model selection** — Veo 3.1, Veo 3.1 Fast, Veo 3.0, or Veo 2.0
- **Resolution and duration** — Up to 4K, 4–8 seconds, 16:9. The available options adapt to the model you pick (see [Video model constraints](#video-model-constraints))
- **Video gallery** — Generated clips are listed in a side panel with a thumbnail of their source image; click to play

---

## Requirements

- Autodesk Revit 2025, 2026, or 2027
- Windows 10 or 11
- A [Google Gemini API key](https://aistudio.google.com/apikey) with access to the image and video generation models — the same key is used for both
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) — required by the standalone video tool. The Revit add-in itself runs on the runtime Revit already ships.

API usage is billed to your own Google account. Video generation in particular is not cheap — check current Veo pricing before generating a batch.

---

## Installation

1. Download the latest release, or build from source (see [Building from source](#building-from-source))
2. Copy `ArchSmarter.Charrette.addin` to `%AppData%\Autodesk\REVIT\Addins\2025\` (or `2026` / `2027`)
3. Copy the add-in DLLs and `ArchSmarterCharrette.VideoTool.exe` into `%AppData%\Autodesk\REVIT\Addins\2025\ArchSmarterCharrette\`
4. Launch Revit. An **ArchSmarter** tab appears in the ribbon with a **Charrette** panel containing **Render Image**, **Render Video**, and **Settings**
5. Click **Settings** and paste in your Gemini API key

The manifest expects the assembly at `ArchSmarterCharrette\ArchSmarterCharrette.dll` relative to the Addins folder, so the subfolder name matters.

---

## Using it

### Render an image

1. Open the view you want to render — a 3D view, elevation, or section. Shaded and realistic views give the model more to work with than hidden line.
2. Click **Render Image**. The active view is exported to a temporary PNG and the render window opens.
3. Pick options from the six dropdowns, or choose a **Theme** or **Preset** to set them all at once. Leaving a dropdown on its default contributes nothing to the prompt.
4. Add free-text **Custom directions** if you want something the library doesn't cover.
5. Click **View Prompt** to see exactly what will be sent.
6. Click **Render**. The result is saved to your output folder and opens in your default image viewer.

The window is non-modal. Change the view in Revit, click **Render Image** again, and the open window picks up the new export.

Right-click a gallery thumbnail for:

| Action | What it does |
|---|---|
| **Use these settings** | Restores the dropdown selections, custom directions, size, and aspect ratio used for that render |
| **Touch up** | Opens a dialog for a targeted edit and sends the image back to Gemini |
| **Open image** | Opens it in your default viewer |
| **Delete** | Deletes the file from disk and removes it from the gallery |

The gallery is per-session. It survives closing and reopening the render window, but resets when Revit restarts. The files on disk are not touched.

### Render a video

Two ways in:

- Click **Video** in the render window to start from your most recent render
- Click **Render Video** on the ribbon to open the tool standalone and browse for any image

Then pick a camera motion, add optional custom motion text, choose a model / resolution / duration in that tool's own **Settings**, and click **Generate**. Generation takes a few minutes; the tool polls for up to 10 minutes before giving up. The finished MP4 lands in your video output folder and opens in your default player.

The video tool is a separate window and a separate process — closing Revit will not stop a generation in progress.

### Video model constraints

The Veo API restricts which combinations are legal, and the dropdowns narrow themselves to match:

| Model | Resolutions | Durations |
|---|---|---|
| Veo 3.1 / 3.1 Fast | 720p, 1080p, 4K | 4, 6, 8 s |
| Veo 3.0 | 720p, 1080p | 4, 6, 8 s |
| Veo 2.0 | 720p | 5, 6, 8 s |

At 1080p or 4K with a source image, only 8 seconds is valid. Aspect ratio is fixed at 16:9.

---

## Settings

**Ribbon → Settings** (image side):

| Setting | Notes |
|---|---|
| Gemini API key | Shared by both the render and video sides. Show/hide toggle. |
| Model | `gemini-2.5-flash-image`, `gemini-3.1-flash-image`, or `gemini-3-pro-image` |
| Output folder | Defaults to `%UserProfile%\Pictures\Charrette` |
| Prompt library folder / file | The active library, plus an **Edit** button that opens the JSON in your default editor |

**Video tool → Settings** (video side): API key, image output folder, video output folder (defaults to `%UserProfile%\Pictures\Charrette\Videos`). Model, resolution, and duration are set in the main video window.

Every change saves to disk immediately — there is no OK/Apply button.

---

## Prompt libraries

Prompt libraries are the main extension point. They are plain JSON files in `%AppData%\ArchSmarter\Charrette\PromptLibraries\`, and any `.json` file you drop in that folder shows up in the library dropdown. A default library is written on first run.

```json
{
  "Items": [
    { "Category": "Style",    "DisplayName": "Default style", "Phrase": "" },
    { "Category": "Style",    "DisplayName": "Watercolor",    "Phrase": "Render in a loose watercolor style with soft washes and visible brushstrokes." },
    { "Category": "Lighting", "DisplayName": "Default lighting", "Phrase": "" },
    { "Category": "Lighting", "DisplayName": "Golden hour",   "Phrase": "Use warm golden-hour sunlight with long soft shadows." }
  ],
  "Themes": [
    {
      "Name": "Sunset Watercolor",
      "Selections": {
        "Style": "Watercolor",
        "Lighting": "Golden hour"
      }
    }
  ]
}
```

- **Categories** are `Style`, `Lighting`, `Material`, `Background`, `Entourage`, and `Weather`. Each needs one entry with an empty `Phrase` to act as the no-op default; if you forget, Charrette inserts one.
- **`DisplayName`** is what appears in the dropdown. **`Phrase`** is the text actually sent to the model.
- **Themes** map a category name to a `DisplayName`. Selecting a theme sets those dropdowns. A theme that references a missing entry skips that category rather than failing.
- A bare JSON array of items (no `Items` / `Themes` wrapper) is still accepted for backward compatibility.

Because phrases are data rather than code, prompt engineering is a text edit and a dropdown change — no rebuild.

---

## Building from source

Prerequisites:

- Visual Studio 2022+ (or the `dotnet` CLI)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) for Revit 2025/2026
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) for Revit 2027 and for the video tool

Build the add-in for your Revit version:

```bash
dotnet build ArchSmarterCharrette/ArchSmarterCharrette.csproj -c "Debug R25"
```

Configurations are `Debug R25`, `Debug R26`, `Debug R27` and the matching `Release` variants. R25/R26 target `net8.0-windows`; R27 targets `net10.0-windows`. Revit API references come from the `Revit_All_Main_Versions_API_x64` NuGet package, not from your Revit install.

A post-build step copies the `.addin` manifest and DLLs into `%AppData%\Autodesk\REVIT\Addins\<year>\`. **Close Revit before building** — it locks the DLL and the copy will fail.

The video tool is a separate project and is not built by the add-in build. Build it once:

```bash
dotnet build ArchSmarterCharrette.VideoTool/ArchSmarterCharrette.VideoTool.csproj -c Debug
```

In a development build the launcher probes `ArchSmarterCharrette.VideoTool\bin\Debug\net10.0-windows\` as a fallback, so the tool works from the solution without being copied anywhere.

To debug, set the configuration to your Revit version and press F5 — the project is preconfigured to start `Revit.exe` with `/language ENG`.

### Release packaging

`Build-Release.ps1` builds the video tool plus the add-in for each requested Revit version and stages everything under `BuildOutput/` in the layout the installer expects:

```bash
pwsh ./Build-Release.ps1 -RevitVersions 25,26,27
```

Passing `-BuildMsi` additionally invokes Advanced Installer against `Installer\ArchSmarterCharrette.aip`. That `.aip` is not in this repository; you'll need your own installer project (or just distribute the staged folders).

---

## How it works

### Rendering pipeline

```
Revit view ──▶ PNG export ──▶ Gemini generateContent ──▶ decoded image ──▶ disk
```

1. **View export** — `ViewExporter` calls `Document.ExportImage()` to write the active view to a temp PNG at 300 DPI / 2048 px.
2. **Prompt assembly** — `PromptBuilder` prepends an adherence preamble, appends each non-empty style phrase as its own sentence, then appends custom directions last.
3. **API call** — `GeminiClient` POSTs the base64 image and prompt to `generativelanguage.googleapis.com/v1beta/models/<model>:generateContent`, requesting `TEXT` and `IMAGE` response modalities, with `imageSize` and `aspectRatio` in `imageConfig` when they aren't left at default.
4. **Result** — The first `inlineData` part in the response is base64-decoded and written to the output folder as `ArchSmarterCharrette_<timestamp>.png`.

Touch-up uses the same client with a different preamble — one that instructs the model to change only what was asked and hold composition, perspective, lighting, colors, and style constant. Results are saved with a `_TouchUp_` tag.

### Prompt construction

Every render prompt opens with a preamble telling the model to preserve the exact geometry, spatial composition, camera angle, and perspective of the source image, and not to add, remove, or reposition building elements. Style phrases follow. If every dropdown is on its default, a neutral fallback ("realistic materials, natural lighting, clean background") stands in so the prompt is never bare.

### Video pipeline

```
Source image ──▶ Veo generateVideos ──▶ poll operation ──▶ download MP4 ──▶ disk
```

1. The user picks a source image — pre-filled from the render window, or browsed for.
2. Camera motion preset text and custom motion text are combined into a prompt.
3. The video tool calls `Google.GenAI`'s `GenerateVideosAsync` with the image bytes, prompt, duration, and (for Veo 3.x) resolution.
4. The API returns a long-running operation; the tool polls every 10 seconds for up to 10 minutes.
5. The finished MP4 is downloaded to the video output folder as `ArchSmarterCharrette_Video_<timestamp>.mp4`, with a companion thumbnail of the source image for the gallery.

### Why the video tool is a separate process

The `Google.GenAI` SDK targets .NET 10, which Revit 2025/2026 does not host. Rather than fight assembly resolution inside Revit, the video generator ships as a standalone `net10.0-windows` WPF executable. `VideoToolLauncher` starts it as a process, passing only `--image <path>` — the API key and all other settings are read by the tool itself from the shared settings JSON, so the key never appears on a command line.

A side effect of that split: the video tool is fully usable on its own, without Revit running.

---

## Project layout

Two projects, one solution (`ArchSmarterCharrette.slnx`).

### ArchSmarterCharrette — the Revit add-in

```
ArchSmarterCharrette/
├── App.cs                          # IExternalApplication — builds the ribbon tab and panel
├── RenderCommand.cs                # Exports the active view, opens the render window
├── VideoCommand.cs                 # Launches the standalone video tool
├── SettingsCommand.cs              # Opens the settings window
├── ArchSmarter.Charrette.addin     # Revit add-in manifest
│
├── Data/
│   ├── GeminiClient.cs             # REST client for Gemini image generation
│   ├── GeminiVideoClient.cs        # VideoToolLauncher — starts the external video process
│   ├── IVideoClient.cs             # VideoGenerationException
│   ├── PromptBuilder.cs            # Assembles prompt text from phrase selections
│   ├── PromptPhrase.cs             # Category + DisplayName + Phrase
│   ├── PromptTheme.cs              # Named set of per-category selections
│   ├── PromptLibrary.cs            # Items[] + Themes[] file shape
│   ├── PromptLibraryManager.cs     # Loads libraries, supplies built-in defaults
│   ├── RenderPreset.cs             # A saved user preset
│   ├── RenderPresetManager.cs      # Preset persistence
│   ├── RenderSettings.cs           # All app settings, with defaults
│   ├── RenderSettingsManager.cs    # Settings persistence and path resolution
│   ├── GalleryItem.cs              # Rendered image + thumbnail + settings used
│   └── SessionHistory.cs           # Static per-session render history
│
├── Helpers/
│   ├── ViewExporter.cs             # Active view ──▶ temp PNG
│   ├── ButtonDataClass.cs          # PushButtonData construction, light/dark icons
│   ├── CommandAvailability.cs      # Ribbon button enablement
│   ├── GlobalUsing.cs              # Project-wide usings
│   └── Utils.cs                    # Ribbon panel creation
│
├── UI/
│   ├── RenderWindow.xaml[.cs]      # Main render window
│   ├── RenderWindowViewModel.cs    # Render state, prompt assembly, gallery, touch-up
│   ├── SettingsWindow.xaml[.cs]    # API key, model, folders, library
│   ├── SettingsWindowViewModel.cs
│   ├── SavePresetDialog.xaml[.cs]  # Preset name input
│   ├── TouchUpDialog.xaml[.cs]     # Touch-up prompt input with thumbnail
│   └── PromptPreviewWindow.xaml[.cs]  # Read-only prompt viewer
│
└── Resources/
    ├── *.png                       # Ribbon icons, light and dark, 16 and 32 px
    └── svg/                        # Tokenized SVG masters the PNGs are rendered from
```

### ArchSmarterCharrette.VideoTool — standalone WPF app (.NET 10)

```
ArchSmarterCharrette.VideoTool/
├── App.xaml[.cs]                   # Entry point; parses args, shows the window
├── VideoArgs.cs                    # Parses --image <path>
├── VideoWindow.xaml[.cs]           # Main video window
├── VideoWindowViewModel.cs         # Veo calls, polling, download, gallery
├── VideoSettingsWindow.xaml[.cs]   # API key and output folders (+ its ViewModel)
└── Data/
    ├── CameraMotionPreset.cs       # The twelve camera movements
    ├── VideoSettings.cs            # Video-relevant fields of the shared settings file
    ├── VideoSettingsManager.cs     # Round-trips the shared JSON without dropping render fields
    ├── VideoGalleryItem.cs         # Generated video + source thumbnail
    ├── InverseBoolToVisConverter.cs
    ├── MoodPreset.cs               # Not currently wired into the UI
    ├── WeatherPreset.cs            # Not currently wired into the UI
    └── SceneActivityPreset.cs      # Not currently wired into the UI
```

### Design decisions worth knowing

- **MVVM, thin code-behind** — ViewModels hold the logic; code-behind handles only WPF mechanics (PasswordBox sync, context-menu plumbing, dialog ownership).
- **No Revit API in the render window** — The view is exported to a temp PNG before the window opens, so the window deals only in file paths and HTTP. That's what makes it safe to be non-modal.
- **Static session history** — `SessionHistory` is static so the gallery survives window close/reopen within a Revit session, and resets on assembly unload.
- **Immediate persistence** — Settings and presets write to disk on every change.
- **Prompt phrases are data** — Editable and shareable JSON, no rebuild required.
- **Separate video process** — See [above](#why-the-video-tool-is-a-separate-process). Both processes write the same settings file, and each models only the fields it cares about, so both round-trip the properties they don't recognize rather than dropping them on save. Note the raw field snapshot is taken when a settings manager is constructed: if both windows are open at once, the last one to save wins for any field the other changed in the meantime.

---

## Configuration files

Everything lives under `%AppData%\ArchSmarter\Charrette\`:

| File | Purpose |
|---|---|
| `Charrette.json` | All settings: API key, model, output folders, library selection, video model/resolution/duration |
| `Charrette_Presets.json` | Saved render presets |
| `PromptLibraries\*.json` | Prompt library files (default: `Charrette_PromptLibrary.json`) |

Default output locations:

| Output | Path |
|---|---|
| Rendered images | `%UserProfile%\Pictures\Charrette\` |
| Generated videos | `%UserProfile%\Pictures\Charrette\Videos\` |

Both are configurable. `Charrette.json` contains your API key in plain text — don't commit it or sync it somewhere public.

---

## Troubleshooting

**The ArchSmarter tab doesn't appear.** Check that the `.addin` file is directly in `%AppData%\Autodesk\REVIT\Addins\<year>\` and that the DLLs are in the `ArchSmarterCharrette` subfolder next to it. Revit only reads the Addins folder at startup.

**"Video tool not found."** `ArchSmarterCharrette.VideoTool.exe` isn't next to the add-in DLL and isn't in the development build output. Build the VideoTool project, or copy its output into the add-in folder.

**The video tool starts and immediately closes.** The .NET 10 Desktop Runtime is probably missing.

**"Please configure your API key in Settings."** The key is empty in `Charrette.json`. Note the render window and video tool read the same key, but each has its own Settings window.

**Revit did not produce an exported image.** The active view can't be exported — schedules and empty sheets are the usual culprits. Switch to a model view.

**The render ignores my geometry.** Some models follow the adherence preamble more faithfully than others, and heavy style phrases pull against it. Try a shaded or realistic source view, a less aggressive Style selection, or a different model.

**Build fails with a file-in-use error.** Revit is running and holding the DLL. Close it.

---

## Contributing

Issues and pull requests are welcome. A few conventions the codebase follows:

- One class per file, named to match; `_camelCase` private fields; PascalCase members
- Global usings live in `Helpers/GlobalUsing.cs` — don't add redundant per-file usings
- New style options belong in a prompt library JSON, not in C#
- Keep Revit API calls out of ViewModels; collect what you need before opening a window

---

## License

MIT.
