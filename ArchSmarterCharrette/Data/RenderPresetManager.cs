using System.Collections.Generic;
using System.Text.Json;

namespace ArchSmarterCharrette.Data
{
    /// <summary>
    /// Loads and saves render presets to a JSON file in the ArchSmarterCharrette config folder.
    /// Each preset captures the full render configuration: category selections,
    /// custom directions, image size, and aspect ratio.
    /// </summary>
    public class RenderPresetManager
    {
        private static readonly string PresetsFileName = "Charrette_Presets.json";
        private readonly string _filePath;
        private List<RenderPreset> _presets;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = null
        };

        public RenderPresetManager()
        {
            _filePath = GetPresetsFilePath();
            _presets = LoadPresets();
        }

        public static string GetPresetsFilePath()
        {
            string folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ArchSmarter", "Charrette");
            return Path.Combine(folderPath, PresetsFileName);
        }

        private List<RenderPreset> LoadPresets()
        {
            if (!File.Exists(_filePath))
                return new List<RenderPreset>();

            try
            {
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<RenderPreset>>(json)
                    ?? new List<RenderPreset>();
            }
            catch (Exception)
            {
                return new List<RenderPreset>();
            }
        }

        private void SavePresets()
        {
            try
            {
                string folderPath = Path.GetDirectoryName(_filePath);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string json = JsonSerializer.Serialize(_presets, JsonOptions);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving presets: {ex.Message}");
            }
        }

        /// <summary>
        /// Returns the names of all saved presets.
        /// </summary>
        public List<string> GetPresetNames()
        {
            return _presets.Select(p => p.Name).ToList();
        }

        /// <summary>
        /// Returns all saved presets.
        /// </summary>
        public List<RenderPreset> GetPresets()
        {
            return _presets;
        }

        /// <summary>
        /// Finds a preset by name (case-insensitive).
        /// </summary>
        public RenderPreset GetPreset(string name)
        {
            return _presets.FirstOrDefault(p =>
                string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Saves a preset. If a preset with the same name already exists, it is overwritten.
        /// </summary>
        public void SavePreset(RenderPreset preset)
        {
            int existingIndex = _presets.FindIndex(p =>
                string.Equals(p.Name, preset.Name, StringComparison.OrdinalIgnoreCase));

            if (existingIndex >= 0)
                _presets[existingIndex] = preset;
            else
                _presets.Add(preset);

            SavePresets();
        }

        /// <summary>
        /// Deletes a preset by name. Returns true if it was found and removed.
        /// </summary>
        public bool DeletePreset(string name)
        {
            int removed = _presets.RemoveAll(p =>
                string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

            if (removed > 0)
            {
                SavePresets();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reloads presets from disk.
        /// </summary>
        public void ReloadPresets()
        {
            _presets = LoadPresets();
        }
    }
}
