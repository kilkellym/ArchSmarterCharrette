using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ReRender.Data;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace ReRender.UI
{
    public class RenderWindowViewModel : INotifyPropertyChanged
    {
        private readonly string _exportedImagePath;
        private readonly RenderSettingsManager _settingsManager;
        private PromptLibraryManager _libraryManager;
        private readonly RenderPresetManager _presetManager;

        public RenderWindowViewModel(string exportedImagePath)
        {
            _exportedImagePath = exportedImagePath;
            _settingsManager = new RenderSettingsManager();
            _presetManager = new RenderPresetManager();

            // Build prompt library path from settings
            string libraryFolder = _settingsManager.GetPromptLibraryFolder();
            string libraryFile = _settingsManager.GetPromptLibraryFile();
            string libraryPath = Path.Combine(libraryFolder, libraryFile);
            _libraryManager = new PromptLibraryManager(libraryPath);

            // Model selection
            AvailableModels = new ObservableCollection<string>(
                _settingsManager.GetAvailableModels());
            string savedModel = _settingsManager.GetModelName();
            _selectedModel = AvailableModels.Contains(savedModel)
                ? savedModel
                : AvailableModels.FirstOrDefault();

            // Load image size and aspect ratio options
            ImageSizes = new ObservableCollection<string>(
                _settingsManager.GetAvailableImageSizes());
            string savedSize = _settingsManager.GetImageSize();
            _selectedImageSize = ImageSizes.Contains(savedSize)
                ? savedSize
                : ImageSizes.FirstOrDefault();

            AspectRatios = new ObservableCollection<string>(
                _settingsManager.GetAvailableAspectRatios());
            string savedRatio = _settingsManager.GetAspectRatio();
            _selectedAspectRatio = AspectRatios.Contains(savedRatio)
                ? savedRatio
                : AspectRatios.FirstOrDefault();

            // Load phrase lists for each category
            StylePhrases = new ObservableCollection<PromptPhrase>(
                _libraryManager.GetPhrasesForCategory("Style"));
            LightingPhrases = new ObservableCollection<PromptPhrase>(
                _libraryManager.GetPhrasesForCategory("Lighting"));
            MaterialPhrases = new ObservableCollection<PromptPhrase>(
                _libraryManager.GetPhrasesForCategory("Material"));
            BackgroundPhrases = new ObservableCollection<PromptPhrase>(
                _libraryManager.GetPhrasesForCategory("Background"));
            EntouragePhrases = new ObservableCollection<PromptPhrase>(
                _libraryManager.GetPhrasesForCategory("Entourage"));
            WeatherPhrases = new ObservableCollection<PromptPhrase>(
                _libraryManager.GetPhrasesForCategory("Weather"));

            // Select the first item (the no-op default) in each dropdown
            _selectedStyle = StylePhrases.FirstOrDefault();
            _selectedLighting = LightingPhrases.FirstOrDefault();
            _selectedMaterial = MaterialPhrases.FirstOrDefault();
            _selectedBackground = BackgroundPhrases.FirstOrDefault();
            _selectedEntourage = EntouragePhrases.FirstOrDefault();
            _selectedWeather = WeatherPhrases.FirstOrDefault();

            // Load themes
            _themes = _libraryManager.GetThemes();
            ThemeNames = new ObservableCollection<string>();
            ThemeNames.Add(NoThemeLabel);
            foreach (PromptTheme theme in _themes)
                ThemeNames.Add(theme.Name);
            _hasThemes = _themes.Count > 0;
            _selectedThemeName = NoThemeLabel;

            // Load presets
            RefreshPresetNames();

            // Load session gallery from static history
            GalleryItems = new ObservableCollection<GalleryItem>();
            foreach (SessionHistoryEntry entry in SessionHistory.Entries)
                GalleryItems.Insert(0, new GalleryItem(entry.FilePath, entry.Settings));

            _statusText = "Ready. Select a rendering style and click Render.";
            _statusColor = Brushes.Gray;
            _canRender = true;
        }

        // -- Refresh after settings change --

        /// <summary>
        /// Reloads settings and prompt library after the Settings window closes.
        /// Tries to preserve the user's current selections where possible.
        /// </summary>
        public void RefreshFromSettings()
        {
            // Reload the settings JSON from disk
            _settingsManager.ReloadSettings();

            // Update model selection
            string currentModel = _settingsManager.GetModelName();
            if (AvailableModels.Contains(currentModel))
                SelectedModel = currentModel;
            else if (AvailableModels.Count > 0)
                SelectedModel = AvailableModels[0];

            // Rebuild prompt library manager (folder or file may have changed)
            string libraryFolder = _settingsManager.GetPromptLibraryFolder();
            string libraryFile = _settingsManager.GetPromptLibraryFile();
            string libraryPath = Path.Combine(libraryFolder, libraryFile);
            _libraryManager = new PromptLibraryManager(libraryPath);

            // Remember current selections so we can try to restore them
            string prevStyle = _selectedStyle?.DisplayName;
            string prevLighting = _selectedLighting?.DisplayName;
            string prevMaterial = _selectedMaterial?.DisplayName;
            string prevBackground = _selectedBackground?.DisplayName;
            string prevEntourage = _selectedEntourage?.DisplayName;
            string prevWeather = _selectedWeather?.DisplayName;

            // Repopulate phrase collections
            RepopulateCollection(StylePhrases, _libraryManager.GetPhrasesForCategory("Style"));
            RepopulateCollection(LightingPhrases, _libraryManager.GetPhrasesForCategory("Lighting"));
            RepopulateCollection(MaterialPhrases, _libraryManager.GetPhrasesForCategory("Material"));
            RepopulateCollection(BackgroundPhrases, _libraryManager.GetPhrasesForCategory("Background"));
            RepopulateCollection(EntouragePhrases, _libraryManager.GetPhrasesForCategory("Entourage"));
            RepopulateCollection(WeatherPhrases, _libraryManager.GetPhrasesForCategory("Weather"));

            // Restore selections (match by DisplayName) or fall back to first
            SelectedStyle = FindByName(StylePhrases, prevStyle);
            SelectedLighting = FindByName(LightingPhrases, prevLighting);
            SelectedMaterial = FindByName(MaterialPhrases, prevMaterial);
            SelectedBackground = FindByName(BackgroundPhrases, prevBackground);
            SelectedEntourage = FindByName(EntouragePhrases, prevEntourage);
            SelectedWeather = FindByName(WeatherPhrases, prevWeather);

            // Repopulate themes
            _themes = _libraryManager.GetThemes();
            ThemeNames.Clear();
            ThemeNames.Add(NoThemeLabel);
            foreach (PromptTheme theme in _themes)
                ThemeNames.Add(theme.Name);
            _hasThemes = _themes.Count > 0;
            OnPropertyChanged(nameof(HasThemes));
            SelectedThemeName = NoThemeLabel;
        }

        private static void RepopulateCollection(ObservableCollection<PromptPhrase> collection,
            List<PromptPhrase> newItems)
        {
            collection.Clear();
            foreach (PromptPhrase item in newItems)
                collection.Add(item);
        }

        private static PromptPhrase FindByName(ObservableCollection<PromptPhrase> collection,
            string displayName)
        {
            if (displayName != null)
            {
                PromptPhrase match = collection.FirstOrDefault(
                    p => p.DisplayName == displayName);
                if (match != null)
                    return match;
            }
            return collection.FirstOrDefault();
        }

        // -- Model selection --

        public ObservableCollection<string> AvailableModels { get; }

        private string _selectedModel;
        public string SelectedModel
        {
            get => _selectedModel;
            set
            {
                _selectedModel = value;
                OnPropertyChanged();
                _settingsManager.SetModelName(value);
            }
        }

        // -- Image size and aspect ratio --

        public ObservableCollection<string> ImageSizes { get; }

        private string _selectedImageSize;
        public string SelectedImageSize
        {
            get => _selectedImageSize;
            set
            {
                _selectedImageSize = value;
                OnPropertyChanged();
                _settingsManager.SetImageSize(value);
            }
        }

        public ObservableCollection<string> AspectRatios { get; }

        private string _selectedAspectRatio;
        public string SelectedAspectRatio
        {
            get => _selectedAspectRatio;
            set
            {
                _selectedAspectRatio = value;
                OnPropertyChanged();
                _settingsManager.SetAspectRatio(value);
            }
        }

        // -- Phrase collections for each dropdown --

        public ObservableCollection<PromptPhrase> StylePhrases { get; }
        public ObservableCollection<PromptPhrase> LightingPhrases { get; }
        public ObservableCollection<PromptPhrase> MaterialPhrases { get; }
        public ObservableCollection<PromptPhrase> BackgroundPhrases { get; }
        public ObservableCollection<PromptPhrase> EntouragePhrases { get; }
        public ObservableCollection<PromptPhrase> WeatherPhrases { get; }

        // -- Themes --

        private const string NoThemeLabel = "(No theme)";
        private List<PromptTheme> _themes;
        private PromptTheme _activeTheme;
        private bool _applyingTheme;

        public ObservableCollection<string> ThemeNames { get; private set; }

        private bool _hasThemes;
        public bool HasThemes => _hasThemes;

        private string _selectedThemeName;
        public string SelectedThemeName
        {
            get => _selectedThemeName;
            set
            {
                _selectedThemeName = value;
                OnPropertyChanged();
                ApplyTheme(value);
            }
        }

        private void ApplyTheme(string themeName)
        {
            PromptTheme theme = _themes.FirstOrDefault(t => t.Name == themeName);
            _activeTheme = theme;

            if (theme == null)
                return;

            _applyingTheme = true;
            try
            {
                foreach (var kvp in theme.Selections)
                {
                    string category = kvp.Key;
                    string displayName = kvp.Value;

                    PromptPhrase match = FindPhraseByDisplayName(category, displayName);
                    if (match == null)
                    {
                        Debug.WriteLine($"Theme \"{theme.Name}\": no item \"{displayName}\" in category \"{category}\", skipping.");
                        continue;
                    }

                    SetCategorySelection(category, match);
                }
            }
            finally
            {
                _applyingTheme = false;
            }
        }

        private PromptPhrase FindPhraseByDisplayName(string category, string displayName)
        {
            ObservableCollection<PromptPhrase> phrases = GetPhrasesForCategory(category);
            return phrases?.FirstOrDefault(p => p.DisplayName == displayName);
        }

        private ObservableCollection<PromptPhrase> GetPhrasesForCategory(string category)
        {
            switch (category)
            {
                case "Style": return StylePhrases;
                case "Lighting": return LightingPhrases;
                case "Material": return MaterialPhrases;
                case "Background": return BackgroundPhrases;
                case "Entourage": return EntouragePhrases;
                case "Weather": return WeatherPhrases;
                default: return null;
            }
        }

        private void SetCategorySelection(string category, PromptPhrase phrase)
        {
            switch (category)
            {
                case "Style": SelectedStyle = phrase; break;
                case "Lighting": SelectedLighting = phrase; break;
                case "Material": SelectedMaterial = phrase; break;
                case "Background": SelectedBackground = phrase; break;
                case "Entourage": SelectedEntourage = phrase; break;
                case "Weather": SelectedWeather = phrase; break;
            }
        }

        /// <summary>
        /// Checks whether the current dropdown selections still match the active theme.
        /// If they've drifted, updates the theme selector to show "(modified)".
        /// </summary>
        private void CheckThemeDrift()
        {
            if (_applyingTheme || _activeTheme == null)
                return;

            bool matches = true;
            foreach (var kvp in _activeTheme.Selections)
            {
                PromptPhrase current = GetCurrentSelection(kvp.Key);
                if (current == null || current.DisplayName != kvp.Value)
                {
                    matches = false;
                    break;
                }
            }

            if (!matches)
            {
                string modifiedLabel = $"{_activeTheme.Name} (modified)";

                // Add the modified label if it's not already in the list
                if (!ThemeNames.Contains(modifiedLabel))
                    ThemeNames.Add(modifiedLabel);

                _selectedThemeName = modifiedLabel;
                OnPropertyChanged(nameof(SelectedThemeName));
            }
        }

        private PromptPhrase GetCurrentSelection(string category)
        {
            switch (category)
            {
                case "Style": return _selectedStyle;
                case "Lighting": return _selectedLighting;
                case "Material": return _selectedMaterial;
                case "Background": return _selectedBackground;
                case "Entourage": return _selectedEntourage;
                case "Weather": return _selectedWeather;
                default: return null;
            }
        }

        // -- Selected phrase in each dropdown --

        private PromptPhrase _selectedStyle;
        public PromptPhrase SelectedStyle
        {
            get => _selectedStyle;
            set { _selectedStyle = value; OnPropertyChanged(); CheckThemeDrift(); }
        }

        private PromptPhrase _selectedLighting;
        public PromptPhrase SelectedLighting
        {
            get => _selectedLighting;
            set { _selectedLighting = value; OnPropertyChanged(); CheckThemeDrift(); }
        }

        private PromptPhrase _selectedMaterial;
        public PromptPhrase SelectedMaterial
        {
            get => _selectedMaterial;
            set { _selectedMaterial = value; OnPropertyChanged(); CheckThemeDrift(); }
        }

        private PromptPhrase _selectedBackground;
        public PromptPhrase SelectedBackground
        {
            get => _selectedBackground;
            set { _selectedBackground = value; OnPropertyChanged(); CheckThemeDrift(); }
        }

        private PromptPhrase _selectedEntourage;
        public PromptPhrase SelectedEntourage
        {
            get => _selectedEntourage;
            set { _selectedEntourage = value; OnPropertyChanged(); CheckThemeDrift(); }
        }

        private PromptPhrase _selectedWeather;
        public PromptPhrase SelectedWeather
        {
            get => _selectedWeather;
            set { _selectedWeather = value; OnPropertyChanged(); CheckThemeDrift(); }
        }

        // -- Render presets --

        private const string NoPresetLabel = "(No preset)";

        public ObservableCollection<string> PresetNames { get; private set; }

        private string _selectedPresetName;
        public string SelectedPresetName
        {
            get => _selectedPresetName;
            set
            {
                _selectedPresetName = value;
                OnPropertyChanged();
                LoadSelectedPreset(value);
            }
        }

        private bool _hasPresets;
        public bool HasPresets => _hasPresets;

        private void RefreshPresetNames()
        {
            List<string> names = _presetManager.GetPresetNames();
            PresetNames = new ObservableCollection<string>();
            PresetNames.Add(NoPresetLabel);
            foreach (string name in names)
                PresetNames.Add(name);
            _hasPresets = names.Count > 0;
            _selectedPresetName = NoPresetLabel;

            OnPropertyChanged(nameof(PresetNames));
            OnPropertyChanged(nameof(HasPresets));
            OnPropertyChanged(nameof(SelectedPresetName));
        }

        private void LoadSelectedPreset(string presetName)
        {
            if (presetName == null || presetName == NoPresetLabel)
                return;

            RenderPreset preset = _presetManager.GetPreset(presetName);
            if (preset == null)
                return;

            // Apply category selections
            _applyingTheme = true;
            try
            {
                foreach (var kvp in preset.Selections)
                {
                    PromptPhrase match = FindPhraseByDisplayName(kvp.Key, kvp.Value);
                    if (match != null)
                        SetCategorySelection(kvp.Key, match);
                }
            }
            finally
            {
                _applyingTheme = false;
            }

            // Apply custom directions
            CustomDirections = preset.CustomDirections ?? "";

            // Apply output settings
            if (!string.IsNullOrEmpty(preset.ImageSize) && ImageSizes.Contains(preset.ImageSize))
                SelectedImageSize = preset.ImageSize;
            if (!string.IsNullOrEmpty(preset.AspectRatio) && AspectRatios.Contains(preset.AspectRatio))
                SelectedAspectRatio = preset.AspectRatio;

            // Clear theme selector since we're loading a preset
            _selectedThemeName = NoThemeLabel;
            _activeTheme = null;
            OnPropertyChanged(nameof(SelectedThemeName));
        }

        /// <summary>
        /// Creates a RenderPreset from the current UI state.
        /// </summary>
        public RenderPreset BuildCurrentPreset(string name)
        {
            var selections = new Dictionary<string, string>();

            if (_selectedStyle != null)
                selections["Style"] = _selectedStyle.DisplayName;
            if (_selectedLighting != null)
                selections["Lighting"] = _selectedLighting.DisplayName;
            if (_selectedMaterial != null)
                selections["Material"] = _selectedMaterial.DisplayName;
            if (_selectedBackground != null)
                selections["Background"] = _selectedBackground.DisplayName;
            if (_selectedEntourage != null)
                selections["Entourage"] = _selectedEntourage.DisplayName;
            if (_selectedWeather != null)
                selections["Weather"] = _selectedWeather.DisplayName;

            return new RenderPreset
            {
                Name = name,
                Selections = selections,
                CustomDirections = _customDirections ?? "",
                ImageSize = _selectedImageSize ?? "",
                AspectRatio = _selectedAspectRatio ?? ""
            };
        }

        /// <summary>
        /// Saves the current UI state as a named preset.
        /// </summary>
        public void SaveCurrentAsPreset(string name)
        {
            RenderPreset preset = BuildCurrentPreset(name);
            _presetManager.SavePreset(preset);
            RefreshPresetNames();
            _selectedPresetName = name;
            OnPropertyChanged(nameof(SelectedPresetName));
        }

        /// <summary>
        /// Deletes the currently selected preset.
        /// </summary>
        public bool DeleteCurrentPreset()
        {
            if (_selectedPresetName == null || _selectedPresetName == NoPresetLabel)
                return false;

            bool deleted = _presetManager.DeletePreset(_selectedPresetName);
            if (deleted)
                RefreshPresetNames();
            return deleted;
        }

        // -- Custom directions (ephemeral, not persisted) --

        private string _customDirections = "";
        public string CustomDirections
        {
            get => _customDirections;
            set { _customDirections = value; OnPropertyChanged(); }
        }

        // -- Render gallery --

        public ObservableCollection<GalleryItem> GalleryItems { get; }

        private bool _isGalleryOpen;
        public bool IsGalleryOpen
        {
            get => _isGalleryOpen;
            set { _isGalleryOpen = value; OnPropertyChanged(); }
        }

        public void ToggleGallery()
        {
            IsGalleryOpen = !IsGalleryOpen;
        }

        public void ClearGallery()
        {
            SessionHistory.Clear();
            GalleryItems.Clear();
            OnPropertyChanged(nameof(HasRenderedImage));
        }

        /// <summary>
        /// Deletes a rendered image from disk and removes it from the gallery.
        /// </summary>
        public void DeleteGalleryItem(GalleryItem item)
        {
            if (item == null)
                return;

            try
            {
                if (File.Exists(item.FilePath))
                    File.Delete(item.FilePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting image: {ex.Message}");
            }

            GalleryItems.Remove(item);
            OnPropertyChanged(nameof(HasRenderedImage));
            OnPropertyChanged(nameof(LastRenderedImagePath));
        }

        /// <summary>
        /// True when at least one image exists in the gallery.
        /// Used to enable the Video button.
        /// </summary>
        public bool HasRenderedImage => GalleryItems.Count > 0;

        /// <summary>
        /// Returns the file path of the most recently rendered image,
        /// or empty string if no renders exist.
        /// </summary>
        public string LastRenderedImagePath =>
            GalleryItems.Count > 0
                ? GalleryItems[0].FilePath
                : "";

        /// <summary>
        /// Opens the folder containing the rendered images in File Explorer.
        /// </summary>
        public void OpenOutputFolder()
        {
            string folder = _settingsManager.GetOutputFolder();

            if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
                Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
        }

        /// <summary>
        /// Opens a specific gallery image in the default viewer.
        /// </summary>
        public void OpenGalleryImage(string filePath)
        {
            if (File.Exists(filePath))
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }

        /// <summary>
        /// Applies the render settings from a gallery item back to the UI,
        /// so the user can re-render with the same (or tweaked) configuration.
        /// </summary>
        public void ApplyGallerySettings(GalleryItem item)
        {
            if (item?.Settings == null)
                return;

            RenderPreset preset = item.Settings;
            var skipped = new List<string>();

            // Apply category selections
            _applyingTheme = true;
            try
            {
                foreach (var kvp in preset.Selections)
                {
                    PromptPhrase match = FindPhraseByDisplayName(kvp.Key, kvp.Value);
                    if (match != null)
                        SetCategorySelection(kvp.Key, match);
                    else
                        skipped.Add($"{kvp.Key}: {kvp.Value}");
                }
            }
            finally
            {
                _applyingTheme = false;
            }

            // Apply custom directions
            CustomDirections = preset.CustomDirections ?? "";

            // Apply output settings
            if (!string.IsNullOrEmpty(preset.ImageSize) && ImageSizes.Contains(preset.ImageSize))
                SelectedImageSize = preset.ImageSize;
            if (!string.IsNullOrEmpty(preset.AspectRatio) && AspectRatios.Contains(preset.AspectRatio))
                SelectedAspectRatio = preset.AspectRatio;

            // Clear theme/preset selectors since we're loading from gallery
            _selectedThemeName = NoThemeLabel;
            _activeTheme = null;
            OnPropertyChanged(nameof(SelectedThemeName));

            _selectedPresetName = NoPresetLabel;
            OnPropertyChanged(nameof(SelectedPresetName));

            // Show status feedback
            if (skipped.Count > 0)
            {
                StatusText = $"Settings applied. Could not resolve {skipped.Count} selection(s) " +
                             $"— prompt library may have changed.\n({string.Join(", ", skipped)})";
                StatusColor = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0xE5, 0xC0, 0x4E));
            }
            else
            {
                StatusText = "Settings applied from gallery.";
                StatusColor = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x6E, 0xC9, 0x6E));
            }
        }

        // -- Status display --

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        private Brush _statusColor;
        public Brush StatusColor
        {
            get => _statusColor;
            set { _statusColor = value; OnPropertyChanged(); }
        }

        private bool _canRender;
        public bool CanRender
        {
            get => _canRender;
            set { _canRender = value; OnPropertyChanged(); }
        }

        private bool _isRendering;
        public bool IsRendering
        {
            get => _isRendering;
            set { _isRendering = value; OnPropertyChanged(); }
        }

        // -- Prompt preview --

        /// <summary>
        /// Builds the prompt text from the current UI selections,
        /// exactly as it would be sent to the Gemini API.
        /// </summary>
        public string GetPromptPreview()
        {
            var selectedPhrases = new List<PromptPhrase>
            {
                SelectedStyle,
                SelectedLighting,
                SelectedMaterial,
                SelectedBackground,
                SelectedEntourage,
                SelectedWeather
            };
            return PromptBuilder.Build(selectedPhrases, CustomDirections);
        }

        // -- Render action --

        public async Task RenderAsync()
        {
            // Read API key from settings (configured in Settings window)
            string apiKey = _settingsManager.GetGeminiApiKey();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                StatusText = "Please configure your API key in Settings.";
                StatusColor = Brushes.Red;
                return;
            }

            CanRender = false;
            IsRendering = true;
            StatusText = "Sending image to Gemini... this may take a minute.";
            StatusColor = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x6E, 0xB3, 0xEB));

            try
            {
                // Read the exported view image from disk
                byte[] imageBytes = File.ReadAllBytes(_exportedImagePath);

                // Assemble the prompt from selected phrases
                var selectedPhrases = new List<PromptPhrase>
                {
                    SelectedStyle,
                    SelectedLighting,
                    SelectedMaterial,
                    SelectedBackground,
                    SelectedEntourage,
                    SelectedWeather
                };
                string prompt = PromptBuilder.Build(selectedPhrases, CustomDirections);

                // Call the Gemini API
                string modelName = SelectedModel;
                string apiEndpoint = _settingsManager.GetApiEndpoint();
                var client = new GeminiClient(apiKey, modelName, apiEndpoint);
                byte[] renderedBytes = await client.RenderAsync(
                    imageBytes, "image/png", prompt, SelectedImageSize, SelectedAspectRatio);

                // Write the rendered image to the configured output folder
                string outputPath = GetOutputPath();
                File.WriteAllBytes(outputPath, renderedBytes);

                // Add to session gallery with the settings used for this render
                RenderPreset usedSettings = BuildCurrentPreset("");
                SessionHistory.Add(outputPath, usedSettings);
                GalleryItems.Insert(0, new GalleryItem(outputPath, usedSettings));
                OnPropertyChanged(nameof(HasRenderedImage));
                OnPropertyChanged(nameof(LastRenderedImagePath));

                StatusText = $"Render complete! Saved to:\n{outputPath}";
                StatusColor = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x6E, 0xC9, 0x6E));

                // Open the rendered image in the default viewer
                Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });
            }
            catch (GeminiException ex)
            {
                StatusText = $"Gemini error: {ex.Message}";
                StatusColor = Brushes.Red;
                Debug.WriteLine($"Gemini response: {ex.ResponseJson}");
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
                StatusColor = Brushes.Red;
                Debug.WriteLine($"Render error: {ex}");
            }
            finally
            {
                CanRender = true;
                IsRendering = false;
            }
        }

        // -- Touch Up --

        /// <summary>
        /// Sends an existing rendered image back to Gemini with a targeted edit prompt.
        /// The system prompt constrains the model to only change what the user asked for.
        /// </summary>
        public async Task TouchUpAsync(string sourceImagePath, string userPrompt)
        {
            string apiKey = _settingsManager.GetGeminiApiKey();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                StatusText = "Please configure your API key in Settings.";
                StatusColor = Brushes.Red;
                return;
            }

            CanRender = false;
            IsRendering = true;
            StatusText = "Sending touch-up request to Gemini...";
            StatusColor = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x6E, 0xB3, 0xEB));

            try
            {
                byte[] imageBytes = File.ReadAllBytes(sourceImagePath);

                // Build a constrained prompt that tells Gemini to only change what the user specified
                string prompt =
                    "You are an architectural image editor. " +
                    "The user wants to make a specific change to this rendered image. " +
                    "ONLY modify what the user describes below. " +
                    "Keep everything else in the image exactly the same — " +
                    "same composition, same perspective, same lighting, same colors, same style. " +
                    "Do not add or remove elements unless the user explicitly asks.\n\n" +
                    $"Requested change: {userPrompt}";

                string modelName = SelectedModel;
                string apiEndpoint = _settingsManager.GetApiEndpoint();
                var client = new GeminiClient(apiKey, modelName, apiEndpoint);

                // Use same image size as current setting; aspect ratio matches original
                byte[] resultBytes = await client.RenderAsync(
                    imageBytes, "image/png", prompt, SelectedImageSize, "");

                // Save the touched-up image
                string outputPath = GetOutputPath("TouchUp");
                File.WriteAllBytes(outputPath, resultBytes);

                // Add to session gallery
                RenderPreset usedSettings = BuildCurrentPreset("");
                SessionHistory.Add(outputPath, usedSettings);
                GalleryItems.Insert(0, new GalleryItem(outputPath, usedSettings));
                OnPropertyChanged(nameof(HasRenderedImage));
                OnPropertyChanged(nameof(LastRenderedImagePath));

                StatusText = $"Touch-up complete! Saved to:\n{outputPath}";
                StatusColor = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x6E, 0xC9, 0x6E));

                // Open in default viewer
                Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });
            }
            catch (GeminiException ex)
            {
                StatusText = $"Gemini error: {ex.Message}";
                StatusColor = Brushes.Red;
                Debug.WriteLine($"Gemini response: {ex.ResponseJson}");
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
                StatusColor = Brushes.Red;
                Debug.WriteLine($"Touch-up error: {ex}");
            }
            finally
            {
                CanRender = true;
                IsRendering = false;
            }
        }

        /// <summary>
        /// Builds the output file path using the configured output folder.
        /// Creates the folder if it doesn't exist.
        /// </summary>
        private string GetOutputPath(string suffix = null)
        {
            string folder = _settingsManager.GetOutputFolder();
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string tag = string.IsNullOrEmpty(suffix) ? "" : $"_{suffix}";
            string fileName = $"ReRender{tag}_{timestamp}.png";
            return Path.Combine(folder, fileName);
        }

        // -- INotifyPropertyChanged --

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
