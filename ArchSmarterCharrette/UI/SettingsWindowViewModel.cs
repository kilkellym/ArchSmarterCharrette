using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ArchSmarterCharrette.Data;

namespace ArchSmarterCharrette.UI
{
    public class SettingsWindowViewModel : INotifyPropertyChanged
    {
        private readonly RenderSettingsManager _settingsManager;

        /// <summary>
        /// Set while ReloadFromDisk is repopulating bound values, so the property
        /// setters don't write what they just read straight back to the file.
        /// </summary>
        private bool _suppressPersist;

        public SettingsWindowViewModel()
        {
            _settingsManager = new RenderSettingsManager();

            _apiKey = _settingsManager.GetGeminiApiKey();

            AvailableModels = new ObservableCollection<string>(
                _settingsManager.GetAvailableModels());

            string savedModel = _settingsManager.GetModelName();
            _selectedModel = AvailableModels.Contains(savedModel)
                ? savedModel
                : AvailableModels.FirstOrDefault();

            // Output folder
            _outputFolder = _settingsManager.GetOutputFolder();

            // Prompt library folder and file
            _promptLibraryFolder = _settingsManager.GetPromptLibraryFolder();
            RefreshLibraryFiles();

        }

        // -- API Key --

        private string _apiKey;
        public string ApiKey
        {
            get => _apiKey;
            set
            {
                _apiKey = value;
                OnPropertyChanged();
                if (!_suppressPersist)
                    _settingsManager.SetGeminiApiKey(value);
            }
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
                if (!_suppressPersist && value != null)
                    _settingsManager.SetModelName(value);
            }
        }

        // -- Output folder --

        private string _outputFolder;
        public string OutputFolder
        {
            get => _outputFolder;
            set
            {
                _outputFolder = value;
                OnPropertyChanged();
                if (!_suppressPersist)
                    _settingsManager.SetOutputFolder(value);
            }
        }

        // -- Prompt library folder and file --

        private string _promptLibraryFolder;
        public string PromptLibraryFolder
        {
            get => _promptLibraryFolder;
            set
            {
                _promptLibraryFolder = value;
                OnPropertyChanged();
                if (!_suppressPersist)
                    _settingsManager.SetPromptLibraryFolder(value);
                RefreshLibraryFiles();
            }
        }

        public ObservableCollection<string> AvailableLibraryFiles { get; }
            = new ObservableCollection<string>();

        private string _selectedLibraryFile;
        public string SelectedLibraryFile
        {
            get => _selectedLibraryFile;
            set
            {
                _selectedLibraryFile = value;
                OnPropertyChanged();
                if (!_suppressPersist && value != null)
                    _settingsManager.SetPromptLibraryFile(value);
            }
        }

        /// <summary>
        /// Returns the full path to the currently selected prompt library file.
        /// </summary>
        public string SelectedLibraryFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_selectedLibraryFile))
                    return "";
                return Path.Combine(_promptLibraryFolder, _selectedLibraryFile);
            }
        }

        /// <summary>
        /// Re-reads the settings file and refreshes every bound value. Called when
        /// the window regains focus after the user has edited the JSON by hand, so
        /// this window's stale in-memory copy doesn't overwrite their changes on
        /// the next save.
        /// </summary>
        public void ReloadFromDisk()
        {
            _suppressPersist = true;
            try
            {
                _settingsManager.ReloadSettings();

                ApiKey = _settingsManager.GetGeminiApiKey();

                string savedModel = _settingsManager.GetModelName();
                AvailableModels.Clear();
                foreach (string model in _settingsManager.GetAvailableModels())
                    AvailableModels.Add(model);
                SelectedModel = AvailableModels.Contains(savedModel)
                    ? savedModel
                    : AvailableModels.FirstOrDefault();

                OutputFolder = _settingsManager.GetOutputFolder();
                PromptLibraryFolder = _settingsManager.GetPromptLibraryFolder();
                RefreshLibraryFiles();
            }
            finally
            {
                _suppressPersist = false;
            }
        }

        /// <summary>
        /// Scans the prompt library folder for JSON files and refreshes the dropdown.
        /// </summary>
        public void RefreshLibraryFiles()
        {
            AvailableLibraryFiles.Clear();

            string folder = _promptLibraryFolder;
            if (Directory.Exists(folder))
            {
                List<string> files = Directory.GetFiles(folder, "*.json")
                    .Select(Path.GetFileName)
                    .OrderBy(f => f)
                    .ToList();

                foreach (string file in files)
                    AvailableLibraryFiles.Add(file);
            }

            // If the folder doesn't exist yet or is empty, ensure the default is created
            if (AvailableLibraryFiles.Count == 0)
            {
                // Create the default file so there's always something to select
                string defaultPath = Path.Combine(folder, PromptLibraryManager.DefaultFileName);
                var tempManager = new PromptLibraryManager(defaultPath);
                AvailableLibraryFiles.Add(Path.GetFileName(defaultPath));
            }

            // Select the saved file, or fall back to the first available
            string savedFile = _settingsManager.GetPromptLibraryFile();
            _selectedLibraryFile = AvailableLibraryFiles.Contains(savedFile)
                ? savedFile
                : AvailableLibraryFiles.FirstOrDefault();
            OnPropertyChanged(nameof(SelectedLibraryFile));
        }

        // -- INotifyPropertyChanged --

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
