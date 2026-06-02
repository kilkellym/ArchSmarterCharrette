using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ReRender.Data
{
    public class PromptLibraryManager
    {
        private static readonly string DefaultFileName = "ReRender_PromptLibrary.json";
        private readonly string _filePath;
        private List<PromptPhrase> _phrases;
        private List<PromptTheme> _themes;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = null
        };

        /// <summary>
        /// Creates a PromptLibraryManager that loads from the specified file path.
        /// </summary>
        public PromptLibraryManager(string filePath)
        {
            _filePath = filePath;
            LoadLibrary();
        }

        /// <summary>
        /// Creates a PromptLibraryManager using the default prompt library location.
        /// </summary>
        public PromptLibraryManager()
            : this(GetDefaultLibraryFilePath())
        {
        }

        /// <summary>
        /// Returns the full path to the currently loaded library file.
        /// </summary>
        public string FilePath => _filePath;

        public static string GetDefaultLibraryFolder()
        {
            return RenderSettingsManager.GetDefaultPromptLibraryFolder();
        }

        public static string GetDefaultLibraryFilePath()
        {
            return Path.Combine(GetDefaultLibraryFolder(), DefaultFileName);
        }

        [Obsolete("Use GetDefaultLibraryFilePath() instead.")]
        public static string GetLibraryFilePath()
        {
            return GetDefaultLibraryFilePath();
        }

        public static List<PromptPhrase> GetBuiltInDefaults()
        {
            return new List<PromptPhrase>
            {
                // Medium
                new PromptPhrase { Category = "Style", DisplayName = "Default style", Phrase = "" },
                new PromptPhrase { Category = "Style", DisplayName = "Watercolor", Phrase = "Render in a loose watercolor style with soft washes and visible brushstrokes." },
                new PromptPhrase { Category = "Style", DisplayName = "Pencil sketch", Phrase = "Render as a hand-drawn pencil sketch with crosshatching and graphite shading." },
                new PromptPhrase { Category = "Style", DisplayName = "Oil painting", Phrase = "Render as an oil painting with rich impasto texture and painterly strokes." },
                new PromptPhrase { Category = "Style", DisplayName = "Photorealistic", Phrase = "Render as a photorealistic image with accurate materials, lighting, and reflections." },
                new PromptPhrase { Category = "Style", DisplayName = "Cel shading", Phrase = "Render in a cel-shaded style with flat colors and bold outlines." },
                new PromptPhrase { Category = "Style", DisplayName = "Ink wash", Phrase = "Render in an ink wash style with flowing black ink gradients on white paper." },

                // Lighting
                new PromptPhrase { Category = "Lighting", DisplayName = "Default lighting", Phrase = "" },
                new PromptPhrase { Category = "Lighting", DisplayName = "Golden hour", Phrase = "Use warm golden-hour sunlight with long soft shadows." },
                new PromptPhrase { Category = "Lighting", DisplayName = "Overcast", Phrase = "Use soft diffused overcast lighting with minimal shadows." },
                new PromptPhrase { Category = "Lighting", DisplayName = "Dramatic", Phrase = "Use dramatic high-contrast lighting with deep shadows and bright highlights." },
                new PromptPhrase { Category = "Lighting", DisplayName = "Night scene", Phrase = "Render as a night scene with artificial lighting, warm interior glow, and cool ambient moonlight." },
                new PromptPhrase { Category = "Lighting", DisplayName = "Studio", Phrase = "Use clean studio lighting with even illumination and soft shadows." },

                // Material
                new PromptPhrase { Category = "Material", DisplayName = "As modeled", Phrase = "" },
                new PromptPhrase { Category = "Material", DisplayName = "Concrete and glass", Phrase = "Apply exposed concrete and floor-to-ceiling glass as the dominant materials." },
                new PromptPhrase { Category = "Material", DisplayName = "Warm wood", Phrase = "Apply warm natural wood tones and timber cladding to surfaces." },
                new PromptPhrase { Category = "Material", DisplayName = "White plaster", Phrase = "Apply clean white plaster and stucco to all wall surfaces." },
                new PromptPhrase { Category = "Material", DisplayName = "Brick and steel", Phrase = "Apply red brick masonry with exposed steel structural elements." },
                new PromptPhrase { Category = "Material", DisplayName = "Weathered patina", Phrase = "Apply weathered and aged patina finishes such as oxidized copper and reclaimed wood." },

                // Background
                new PromptPhrase { Category = "Background", DisplayName = "As is", Phrase = "" },
                new PromptPhrase { Category = "Background", DisplayName = "Clear sky", Phrase = "Set the background to a clear blue sky with minimal clouds." },
                new PromptPhrase { Category = "Background", DisplayName = "Landscaped", Phrase = "Add lush landscaping with mature trees, shrubs, and ground cover around the building." },
                new PromptPhrase { Category = "Background", DisplayName = "Urban context", Phrase = "Place the building in an urban streetscape with sidewalks, neighboring buildings, and street trees." },
                new PromptPhrase { Category = "Background", DisplayName = "Mountainous", Phrase = "Set the background to a mountainous landscape with distant peaks and evergreen trees." },
                new PromptPhrase { Category = "Background", DisplayName = "Waterfront", Phrase = "Place the building on a waterfront setting with a reflective body of water in the foreground." },

                // Entourage
                new PromptPhrase { Category = "Entourage", DisplayName = "No entourage", Phrase = "" },
                new PromptPhrase { Category = "Entourage", DisplayName = "Scale figures", Phrase = "Add a few scale figures of people standing and walking to convey human scale." },
                new PromptPhrase { Category = "Entourage", DisplayName = "Busy and lively", Phrase = "Populate the scene with pedestrians, small groups, and visible activity." },
                new PromptPhrase { Category = "Entourage", DisplayName = "Vehicles", Phrase = "Add parked and moving vehicles appropriate to the setting." },
                new PromptPhrase { Category = "Entourage", DisplayName = "Empty and quiet", Phrase = "Keep the scene unpopulated with no people or vehicles." },

                // Weather
                new PromptPhrase { Category = "Weather", DisplayName = "As is", Phrase = "" },
                new PromptPhrase { Category = "Weather", DisplayName = "Light fog", Phrase = "Add a light atmospheric fog that softens distant elements and adds depth." },
                new PromptPhrase { Category = "Weather", DisplayName = "Rain", Phrase = "Render with light rain and wet reflective surfaces." },
                new PromptPhrase { Category = "Weather", DisplayName = "Snow", Phrase = "Add falling snow with a light accumulation on surfaces." },
                new PromptPhrase { Category = "Weather", DisplayName = "Morning mist", Phrase = "Add a low morning mist hugging the ground." },
            };
        }

        private static readonly Dictionary<string, string> CategoryDefaults = new Dictionary<string, string>
        {
            { "Style", "Default style" },
            { "Lighting", "Default lighting" },
            { "Material", "As modeled" },
            { "Background", "As is" },
            { "Entourage", "No entourage" },
            { "Weather", "As is" },
        };

        /// <summary>
        /// Loads Items and Themes from the library file. Accepts two JSON shapes:
        /// 1) A flat array (legacy) — treated as Items with no Themes.
        /// 2) An object with "Items" and optional "Themes" properties.
        /// </summary>
        private void LoadLibrary()
        {
            if (!File.Exists(_filePath))
            {
                _phrases = GetBuiltInDefaults();
                _themes = new List<PromptTheme>();
                WriteLibrary(_phrases);
                return;
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                json = json.TrimStart();

                if (json.StartsWith("["))
                {
                    // Legacy flat-array shape
                    _phrases = JsonSerializer.Deserialize<List<PromptPhrase>>(json)
                        ?? new List<PromptPhrase>();
                    _themes = new List<PromptTheme>();
                }
                else
                {
                    // Object shape with Items and optional Themes
                    PromptLibrary library = JsonSerializer.Deserialize<PromptLibrary>(json)
                        ?? new PromptLibrary();
                    _phrases = library.Items ?? new List<PromptPhrase>();
                    _themes = library.Themes ?? new List<PromptTheme>();
                }

                _phrases = EnsureDefaults(_phrases);
            }
            catch (Exception)
            {
                _phrases = GetBuiltInDefaults();
                _themes = new List<PromptTheme>();
            }
        }

        private List<PromptPhrase> EnsureDefaults(List<PromptPhrase> phrases)
        {
            foreach (var kvp in CategoryDefaults)
            {
                string category = kvp.Key;
                string defaultDisplayName = kvp.Value;

                bool hasDefault = phrases.Any(p =>
                    p.Category == category && string.IsNullOrEmpty(p.Phrase));

                if (!hasDefault)
                {
                    phrases.Insert(0, new PromptPhrase
                    {
                        Category = category,
                        DisplayName = defaultDisplayName,
                        Phrase = ""
                    });
                }
            }

            return phrases;
        }

        private void WriteLibrary(List<PromptPhrase> phrases)
        {
            try
            {
                string folderPath = Path.GetDirectoryName(_filePath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string json = JsonSerializer.Serialize(phrases, JsonOptions);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error writing prompt library: {ex.Message}");
            }
        }

        public void ReloadLibrary()
        {
            LoadLibrary();
        }

        public List<PromptPhrase> GetAllPhrases()
        {
            return _phrases;
        }

        public List<PromptPhrase> GetPhrasesForCategory(string category)
        {
            return _phrases.Where(p => p.Category == category).ToList();
        }

        public List<string> GetCategories()
        {
            return _phrases.Select(p => p.Category).Distinct().ToList();
        }

        public List<PromptTheme> GetThemes()
        {
            return _themes;
        }
    }
}
