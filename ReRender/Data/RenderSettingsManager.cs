using System.Text.Json;

namespace ReRender.Data
{
    public class RenderSettingsManager
    {
        private static readonly string SettingsFileName = "ReRender.json";
        private readonly string _filePath;
        private RenderSettings _settings;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = null
        };

        public RenderSettingsManager()
        {
            _filePath = GetSettingsFilePath();
            _settings = LoadSettings();
        }

        public static string GetSettingsFilePath()
        {
            string folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ArchSmarter", "ReRender");
            return Path.Combine(folderPath, SettingsFileName);
        }

        public static bool SettingsFileExists()
        {
            return File.Exists(GetSettingsFilePath());
        }

        public static string CreateDefaultSettingsFile()
        {
            string filePath = GetSettingsFilePath();
            string folderPath = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (File.Exists(filePath))
            {
                return filePath;
            }

            var defaultSettings = new RenderSettings();
            string json = JsonSerializer.Serialize(defaultSettings, JsonOptions);
            File.WriteAllText(filePath, json);

            return filePath;
        }

        private RenderSettings LoadSettings()
        {
            if (!File.Exists(_filePath))
            {
                CreateDefaultSettingsFile();
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<RenderSettings>(json) ?? new RenderSettings();
            }
            catch (Exception)
            {
                return new RenderSettings();
            }
        }

        public void SaveSettings()
        {
            try
            {
                string folderPath = Path.GetDirectoryName(_filePath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string json = JsonSerializer.Serialize(_settings, JsonOptions);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving settings: {ex.Message}");
                throw;
            }
        }

        public void ReloadSettings()
        {
            _settings = LoadSettings();
        }

        // -- Output folder --

        public static string GetDefaultOutputFolder()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "ReRender");
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
            // Store empty string to mean "use default"
            if (string.Equals(folder, GetDefaultOutputFolder(), StringComparison.OrdinalIgnoreCase))
                folder = "";
            _settings.OutputFolder = folder ?? "";
            SaveSettings();
        }

        public string GetGeminiApiKey()
        {
            return _settings.GeminiApiKey ?? "";
        }

        public void SetGeminiApiKey(string apiKey)
        {
            _settings.GeminiApiKey = apiKey ?? "";
            SaveSettings();
        }

        public string GetApiEndpoint()
        {
            return _settings.ApiEndpoint ?? new RenderSettings().ApiEndpoint;
        }

        public void SetApiEndpoint(string endpoint)
        {
            _settings.ApiEndpoint = endpoint ?? new RenderSettings().ApiEndpoint;
            SaveSettings();
        }

        public List<string> GetAvailableModels()
        {
            List<string> models = _settings.AvailableModels;
            if (models == null || models.Count == 0)
            {
                return new RenderSettings().AvailableModels;
            }
            return models;
        }

        public string GetModelName()
        {
            return _settings.ModelName ?? new RenderSettings().ModelName;
        }

        public void SetModelName(string modelName)
        {
            _settings.ModelName = modelName ?? new RenderSettings().ModelName;
            SaveSettings();
        }

        public List<string> GetAvailableImageSizes()
        {
            List<string> sizes = _settings.AvailableImageSizes;
            if (sizes == null || sizes.Count == 0)
            {
                return new RenderSettings().AvailableImageSizes;
            }
            return sizes;
        }

        public string GetImageSize()
        {
            return _settings.ImageSize ?? new RenderSettings().ImageSize;
        }

        public void SetImageSize(string imageSize)
        {
            _settings.ImageSize = imageSize ?? new RenderSettings().ImageSize;
            SaveSettings();
        }

        public List<string> GetAvailableAspectRatios()
        {
            List<string> ratios = _settings.AvailableAspectRatios;
            if (ratios == null || ratios.Count == 0)
            {
                return new RenderSettings().AvailableAspectRatios;
            }
            return ratios;
        }

        public string GetAspectRatio()
        {
            return _settings.AspectRatio ?? new RenderSettings().AspectRatio;
        }

        public void SetAspectRatio(string aspectRatio)
        {
            _settings.AspectRatio = aspectRatio ?? "";
            SaveSettings();
        }

        // -- Prompt library folder and file --

        public static string GetDefaultPromptLibraryFolder()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ArchSmarter", "ReRender", "PromptLibraries");
        }

        public string GetPromptLibraryFolder()
        {
            string folder = _settings.PromptLibraryFolder;
            if (string.IsNullOrWhiteSpace(folder))
                return GetDefaultPromptLibraryFolder();
            return folder;
        }

        public void SetPromptLibraryFolder(string folder)
        {
            // Store empty string to mean "use default"
            if (string.Equals(folder, GetDefaultPromptLibraryFolder(), StringComparison.OrdinalIgnoreCase))
                folder = "";
            _settings.PromptLibraryFolder = folder ?? "";
            SaveSettings();
        }

        public string GetPromptLibraryFile()
        {
            return _settings.PromptLibraryFile ?? new RenderSettings().PromptLibraryFile;
        }

        public void SetPromptLibraryFile(string fileName)
        {
            _settings.PromptLibraryFile = fileName ?? new RenderSettings().PromptLibraryFile;
            SaveSettings();
        }
    }
}
