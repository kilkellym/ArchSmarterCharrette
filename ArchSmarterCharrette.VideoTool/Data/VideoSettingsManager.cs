using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace ArchSmarterCharrette.VideoTool.Data
{
    /// <summary>
    /// Reads and writes video settings from the shared Charrette.json file.
    /// The settings file lives at %AppData%\ArchSmarter\Charrette\Charrette.json
    /// and is shared with the Revit add-in.
    ///
    /// Because the JSON contains fields for both render and video settings,
    /// we deserialize into a JsonDocument to preserve unknown fields on save.
    /// </summary>
    public class VideoSettingsManager
    {
        private static readonly string SettingsFileName = "Charrette.json";
        private readonly string _filePath;
        private VideoSettings _settings;

        /// <summary>
        /// Raw JSON dictionary used to round-trip all fields (including render settings
        /// that this app doesn't know about) without data loss.
        /// </summary>
        private Dictionary<string, JsonElement> _rawFields;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = null
        };

        public VideoSettingsManager()
        {
            _filePath = GetSettingsFilePath();
            LoadSettings();
        }

        public static string GetSettingsFilePath()
        {
            string folderPath = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                "ArchSmarter", "Charrette");
            return Path.Combine(folderPath, SettingsFileName);
        }

        private void LoadSettings()
        {
            _settings = new VideoSettings();
            _rawFields = ReadRawFieldsFromDisk();

            if (!System.IO.File.Exists(_filePath))
                return;

            try
            {
                string json = System.IO.File.ReadAllText(_filePath);

                // Deserialize only the fields we care about
                _settings = JsonSerializer.Deserialize<VideoSettings>(json) ?? new VideoSettings();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading settings: {ex.Message}");
                _settings = new VideoSettings();
            }
        }

        /// <summary>
        /// Reads every property currently in the settings file.
        /// Called before each save rather than relying on the snapshot taken at
        /// construction: the Revit add-in shares this file and may have written
        /// to it since — including creating it, if it did not exist when this
        /// window opened. A stale snapshot would silently drop those fields.
        /// </summary>
        private Dictionary<string, JsonElement> ReadRawFieldsFromDisk()
        {
            var fields = new Dictionary<string, JsonElement>();

            try
            {
                if (System.IO.File.Exists(_filePath))
                {
                    using JsonDocument doc = JsonDocument.Parse(System.IO.File.ReadAllText(_filePath));
                    foreach (JsonProperty prop in doc.RootElement.EnumerateObject())
                        fields[prop.Name] = prop.Value.Clone();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading settings for merge: {ex.Message}");
            }

            return fields;
        }

        private void SaveSettings()
        {
            try
            {
                string folderPath = Path.GetDirectoryName(_filePath);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // Re-read first so fields the add-in wrote since we loaded are
                // picked up, then merge our own values over the top.
                _rawFields = ReadRawFieldsFromDisk();

                // Serialize our video settings to get updated values
                string videoJson = JsonSerializer.Serialize(_settings, JsonOptions);
                using JsonDocument videoDoc = JsonDocument.Parse(videoJson);

                // Merge our fields into the raw dictionary (preserves render settings)
                foreach (JsonProperty prop in videoDoc.RootElement.EnumerateObject())
                {
                    _rawFields[prop.Name] = prop.Value.Clone();
                }

                // Write the merged dictionary
                using var stream = new MemoryStream();
                using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
                {
                    writer.WriteStartObject();
                    foreach (var kvp in _rawFields)
                    {
                        writer.WritePropertyName(kvp.Key);
                        kvp.Value.WriteTo(writer);
                    }
                    writer.WriteEndObject();
                }

                System.IO.File.WriteAllBytes(_filePath, stream.ToArray());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        // -- Output folder (images) --

        public static string GetDefaultOutputFolder()
        {
            return Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures),
                "Charrette");
        }

        public string GetOutputFolder()
        {
            string folder = _settings.OutputFolder;
            if (string.IsNullOrWhiteSpace(folder))
                return GetDefaultOutputFolder();
            return folder;
        }

        public void SetOutputFolder(string folder)
        {
            _settings.OutputFolder = folder ?? "";
            SaveSettings();
        }

        // -- Video output folder --

        public static string GetDefaultVideoOutputFolder()
        {
            return Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures),
                "Charrette", "Videos");
        }

        public string GetVideoOutputFolder()
        {
            string folder = _settings.VideoOutputFolder;
            if (string.IsNullOrWhiteSpace(folder))
                return GetDefaultVideoOutputFolder();
            return folder;
        }

        public void SetVideoOutputFolder(string folder)
        {
            _settings.VideoOutputFolder = folder ?? "";
            SaveSettings();
        }

        // -- API key --

        public string GetGeminiApiKey()
        {
            return _settings.GeminiApiKey ?? "";
        }

        public void SetGeminiApiKey(string key)
        {
            _settings.GeminiApiKey = key ?? "";
            SaveSettings();
        }

        // -- Video model --

        public string GetVideoModel()
        {
            return _settings.VideoModel ?? new VideoSettings().VideoModel;
        }

        public void SetVideoModel(string model)
        {
            _settings.VideoModel = model ?? new VideoSettings().VideoModel;
            SaveSettings();
        }

        public List<string> GetAvailableVideoModels()
        {
            List<string> models = _settings.AvailableVideoModels;
            if (models == null || models.Count == 0)
                return new VideoSettings().AvailableVideoModels;
            return models;
        }

        // -- Resolution --

        public List<string> GetAvailableVideoResolutions()
        {
            List<string> resolutions = _settings.AvailableVideoResolutions;
            if (resolutions == null || resolutions.Count == 0)
                return new VideoSettings().AvailableVideoResolutions;
            return resolutions;
        }

        public string GetVideoResolution()
        {
            return _settings.VideoResolution ?? new VideoSettings().VideoResolution;
        }

        public void SetVideoResolution(string resolution)
        {
            _settings.VideoResolution = resolution ?? new VideoSettings().VideoResolution;
            SaveSettings();
        }

        // -- Duration --

        public List<int> GetAvailableVideoDurations()
        {
            List<int> durations = _settings.AvailableVideoDurations;
            if (durations == null || durations.Count == 0)
                return new VideoSettings().AvailableVideoDurations;
            return durations;
        }

        public int GetVideoDuration()
        {
            return _settings.VideoDuration > 0
                ? _settings.VideoDuration
                : new VideoSettings().VideoDuration;
        }

        public void SetVideoDuration(int duration)
        {
            _settings.VideoDuration = duration > 0 ? duration : new VideoSettings().VideoDuration;
            SaveSettings();
        }
    }
}
