using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Google.GenAI;
using Google.GenAI.Types;
using ArchSmarterCharrette.VideoTool.Data;

namespace ArchSmarterCharrette.VideoTool
{
    public class VideoWindowViewModel : INotifyPropertyChanged
    {
        private string _renderedImagePath;
        private readonly VideoSettingsManager _settingsManager;
        private CancellationTokenSource _cts;

        public VideoWindowViewModel(string renderedImagePath)
        {
            _renderedImagePath = renderedImagePath;
            _settingsManager = new VideoSettingsManager();

            // Load camera motion presets
            MotionPresets = new ObservableCollection<CameraMotionPreset>(
                CameraMotionPreset.GetDefaults());
            _selectedMotionPreset = MotionPresets.FirstOrDefault();

            // Load video model options
            VideoModels = new ObservableCollection<string>(
                _settingsManager.GetAvailableVideoModels());
            string savedModel = _settingsManager.GetVideoModel();
            _selectedVideoModel = VideoModels.Contains(savedModel)
                ? savedModel
                : VideoModels.FirstOrDefault();

            // Load resolution options (model-dependent)
            List<string> validResolutions = GetValidResolutions(_selectedVideoModel);
            Resolutions = new ObservableCollection<string>(validResolutions);
            string savedResolution = _settingsManager.GetVideoResolution();
            _selectedResolution = validResolutions.Contains(savedResolution)
                ? savedResolution
                : validResolutions.First();

            // Load duration options (model- and resolution-dependent)
            List<int> validDurations = GetValidDurations(_selectedVideoModel, _selectedResolution);
            Durations = new ObservableCollection<int>(validDurations);
            int savedDuration = _settingsManager.GetVideoDuration();
            _selectedDuration = validDurations.Contains(savedDuration)
                ? savedDuration
                : validDurations.Last();

            // Start with empty video gallery (populated as videos are generated)
            VideoGalleryItems = new ObservableCollection<VideoGalleryItem>();

            // Load thumbnail if an image was provided (shortcut/CLI mode)
            if (!string.IsNullOrEmpty(_renderedImagePath) && System.IO.File.Exists(_renderedImagePath))
            {
                LoadThumbnail();
                _statusText = "Select a camera motion and click Generate.";
                _statusColor = Brushes.Gray;
                _canGenerate = true;
            }
            else
            {
                _renderedImagePath = "";
                _statusText = "Select a source image to get started.";
                _statusColor = Brushes.Gray;
                _canGenerate = false;
            }
        }

        // -- Video gallery --

        public ObservableCollection<VideoGalleryItem> VideoGalleryItems { get; }

        public bool HasVideoGalleryItems => VideoGalleryItems.Count > 0;

        public void PlayVideo(VideoGalleryItem item)
        {
            item?.Play();
        }

        /// <summary>
        /// Deletes a video and its companion thumbnail from disk, and removes it from the gallery.
        /// </summary>
        public void DeleteVideoItem(VideoGalleryItem item)
        {
            if (item == null)
                return;

            try
            {
                if (System.IO.File.Exists(item.FilePath))
                    System.IO.File.Delete(item.FilePath);

                string thumbPath = VideoGalleryItem.GetThumbnailPath(item.FilePath);
                if (System.IO.File.Exists(thumbPath))
                    System.IO.File.Delete(thumbPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting video: {ex.Message}");
            }

            VideoGalleryItems.Remove(item);
            OnPropertyChanged(nameof(HasVideoGalleryItems));
        }

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

        // -- Source image selection --

        public bool HasSourceImage =>
            !string.IsNullOrEmpty(_renderedImagePath) && System.IO.File.Exists(_renderedImagePath);

        private string _sourceImageLabel = "No image selected";
        public string SourceImageLabel
        {
            get => _sourceImageLabel;
            private set { _sourceImageLabel = value; OnPropertyChanged(); }
        }

        public void BrowseForImage()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select source image for video",
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                SetSourceImage(dialog.FileName);
            }
        }

        private void SetSourceImage(string filePath)
        {
            _renderedImagePath = filePath;
            LoadThumbnail();
            SourceImageLabel = Path.GetFileName(filePath);
            CanGenerate = !_isGenerating;
            StatusText = "Select a camera motion and click Generate.";
            StatusColor = Brushes.Gray;
            OnPropertyChanged(nameof(HasSourceImage));
            OnPropertyChanged(nameof(Thumbnail));
        }

        // -- Source image thumbnail --

        private BitmapImage _thumbnail;
        public BitmapImage Thumbnail => _thumbnail;

        private void LoadThumbnail()
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(_renderedImagePath);
                bitmap.DecodePixelWidth = 360;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                _thumbnail = bitmap;
                SourceImageLabel = Path.GetFileName(_renderedImagePath);
            }
            catch
            {
                _thumbnail = null;
            }
        }

        // -- Camera motion preset --

        public ObservableCollection<CameraMotionPreset> MotionPresets { get; }

        private CameraMotionPreset _selectedMotionPreset;
        public CameraMotionPreset SelectedMotionPreset
        {
            get => _selectedMotionPreset;
            set { _selectedMotionPreset = value; OnPropertyChanged(); }
        }

        // -- Custom motion text --

        private string _customMotionText = "";
        public string CustomMotionText
        {
            get => _customMotionText;
            set { _customMotionText = value; OnPropertyChanged(); }
        }

        // -- Video model --

        public ObservableCollection<string> VideoModels { get; }

        private string _selectedVideoModel;
        public string SelectedVideoModel
        {
            get => _selectedVideoModel;
            set
            {
                _selectedVideoModel = value;
                OnPropertyChanged();
                _settingsManager.SetVideoModel(value);
                RefreshResolutionsForModel();
                RefreshDurationsForModel();
            }
        }

        // -- Resolution --

        public ObservableCollection<string> Resolutions { get; }

        private string _selectedResolution;
        public string SelectedResolution
        {
            get => _selectedResolution;
            set
            {
                _selectedResolution = value;
                OnPropertyChanged();
                _settingsManager.SetVideoResolution(value);
                RefreshDurationsForModel();
            }
        }

        // -- Duration --

        public ObservableCollection<int> Durations { get; }

        private int _selectedDuration;
        public int SelectedDuration
        {
            get => _selectedDuration;
            set
            {
                _selectedDuration = value;
                OnPropertyChanged();
                _settingsManager.SetVideoDuration(value);
            }
        }

        /// <summary>
        /// Returns the valid resolutions for a given Veo model.
        /// Veo 2.0 only supports 720p (resolution param not accepted by API).
        /// Veo 3.0 supports 720p, 1080p.
        /// Veo 3.1 supports 720p, 1080p, 4K.
        /// </summary>
        private static List<string> GetValidResolutions(string modelName)
        {
            if (modelName != null && modelName.Contains("veo-2"))
                return new List<string> { "720p" };

            if (modelName != null && modelName.Contains("veo-3.0"))
                return new List<string> { "720p", "1080p" };

            // Veo 3.1 and newer
            return new List<string> { "720p", "1080p", "4K" };
        }

        private void RefreshResolutionsForModel()
        {
            List<string> validResolutions = GetValidResolutions(_selectedVideoModel);
            string previousSelection = _selectedResolution;

            Resolutions.Clear();
            foreach (string r in validResolutions)
                Resolutions.Add(r);

            // Keep previous selection if still valid, otherwise pick the first (720p)
            if (validResolutions.Contains(previousSelection))
                SelectedResolution = previousSelection;
            else
                SelectedResolution = validResolutions.First();
        }

        /// <summary>
        /// Returns the valid discrete duration values for a given Veo model.
        /// Veo 2.0 uses {5, 6, 8}; all Veo 3.x/3.1.x models use {4, 6, 8}.
        /// When resolution is 1080p or 4K, only 8 is valid (image-to-video constraint).
        /// </summary>
        private static List<int> GetValidDurations(string modelName, string resolution)
        {
            // 1080p or 4K with a source image requires duration = 8
            if (string.Equals(resolution, "1080p", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(resolution, "4K", StringComparison.OrdinalIgnoreCase))
                return new List<int> { 8 };

            // Veo 2.0 models use 5, 6, 8
            if (modelName != null && modelName.Contains("veo-2"))
                return new List<int> { 5, 6, 8 };

            // All Veo 3.x / 3.1.x models use 4, 6, 8
            return new List<int> { 4, 6, 8 };
        }

        private void RefreshDurationsForModel()
        {
            List<int> validDurations = GetValidDurations(_selectedVideoModel, _selectedResolution);
            int previousSelection = _selectedDuration;

            Durations.Clear();
            foreach (int d in validDurations)
                Durations.Add(d);

            // Keep previous selection if still valid, otherwise pick the largest
            if (validDurations.Contains(previousSelection))
                SelectedDuration = previousSelection;
            else
                SelectedDuration = validDurations.Last();
        }

        // -- Generation state --

        private bool _isGenerating;
        public bool IsGenerating
        {
            get => _isGenerating;
            set { _isGenerating = value; OnPropertyChanged(); }
        }

        private bool _canGenerate;
        public bool CanGenerate
        {
            get => _canGenerate;
            set { _canGenerate = value; OnPropertyChanged(); }
        }

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

        // -- Generate video (async, runs inline) --

        public async Task GenerateVideoAsync()
        {
            string apiKey = _settingsManager.GetGeminiApiKey();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                StatusText = "API key is not set. Add your Google API key to Charrette.json in\n" +
                             VideoSettingsManager.GetSettingsFilePath();
                StatusColor = Brushes.Red;
                return;
            }

            if (!System.IO.File.Exists(_renderedImagePath))
            {
                StatusText = "Source image not found on disk.";
                StatusColor = Brushes.Red;
                return;
            }

            _cts = new CancellationTokenSource();
            IsGenerating = true;
            CanGenerate = false;

            try
            {
                await RunVideoGenerationAsync(apiKey, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                StatusText = "Cancelled.";
                StatusColor = Brushes.Gray;
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
                StatusColor = Brushes.Red;
                Debug.WriteLine($"Video generation error: {ex}");
            }
            finally
            {
                IsGenerating = false;
                CanGenerate = HasSourceImage;
            }
        }

        public void CancelGeneration()
        {
            _cts?.Cancel();
        }

        private async Task RunVideoGenerationAsync(string apiKey, CancellationToken ct)
        {
            // Read image bytes
            StatusText = "Reading source image...";
            StatusColor = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0));

            byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(_renderedImagePath, ct);

            // Determine MIME type
            string ext = Path.GetExtension(_renderedImagePath).ToLowerInvariant();
            string mimeType = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".bmp" => "image/bmp",
                _ => "image/png"
            };

            // Build the prompt
            string prompt = BuildVideoPrompt();
            Debug.WriteLine($"Video prompt: {prompt}");

            // Show the prompt briefly so user can verify settings are applied
            StatusText = $"Prompt: {prompt.Substring(0, Math.Min(prompt.Length, 120))}...";
            StatusColor = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0));
            await Task.Delay(1500, ct);

            // Create the GenAI client
            StatusText = "Submitting video request...";
            using var client = new Client(apiKey: apiKey);

            var source = new GenerateVideosSource
            {
                Prompt = prompt,
                Image = new Google.GenAI.Types.Image
                {
                    ImageBytes = imageBytes,
                    MimeType = mimeType
                }
            };

            var config = new GenerateVideosConfig
            {
                NumberOfVideos = 1,
                AspectRatio = "16:9",
                DurationSeconds = SelectedDuration,
                PersonGeneration = "allow_adult"
            };

            // Veo 2.0 does not accept the resolution parameter — only set it for Veo 3.x+
            bool isVeo2 = SelectedVideoModel != null && SelectedVideoModel.Contains("veo-2");
            if (!isVeo2 && !string.IsNullOrEmpty(SelectedResolution))
                config.Resolution = SelectedResolution;

            // Start the generation
            GenerateVideosOperation operation = await client.Models.GenerateVideosAsync(
                model: SelectedVideoModel,
                source: source,
                config: config,
                cancellationToken: ct);

            StatusText = $"Video is generating... this may take a few minutes.\nOperation: {operation.Name}";

            // Poll for completion
            int pollCount = 0;
            int maxPolls = 60; // 10 minutes at 10-second intervals

            while (pollCount < maxPolls)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(10000, ct);
                pollCount++;

                operation = await client.Operations.GetAsync(operation, null, ct);

                if (operation.Done == true)
                    break;

                StatusText = $"Generating video... ({pollCount * 10}s elapsed)";
            }

            if (operation.Done != true)
            {
                StatusText = "Video generation timed out after 10 minutes.";
                StatusColor = Brushes.Red;
                return;
            }

            // Check for result
            GenerateVideosResponse response = operation.Response;
            if (response?.GeneratedVideos == null || response.GeneratedVideos.Count == 0)
            {
                StatusText = "Video generation completed but no videos were returned.";
                StatusColor = Brushes.Red;
                return;
            }

            // Download the video
            StatusText = "Downloading video...";
            GeneratedVideo generatedVideo = response.GeneratedVideos[0];

            string outputFolder = _settingsManager.GetVideoOutputFolder();
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string outputPath = Path.Combine(outputFolder, $"ArchSmarterCharrette_Video_{timestamp}.mp4");

            await client.Files.DownloadToFileAsync(generatedVideo, outputPath, cancellationToken: ct);

            // Save a thumbnail of the source image alongside the video
            VideoGalleryItem.SaveThumbnail(outputPath, _renderedImagePath);

            // Add the new video to the gallery sidebar
            var newItem = new VideoGalleryItem(outputPath);
            VideoGalleryItems.Insert(0, newItem);
            OnPropertyChanged(nameof(HasVideoGalleryItems));

            StatusText = $"Video saved to:\n{outputPath}";
            StatusColor = new SolidColorBrush(Color.FromRgb(0x6E, 0xC9, 0x6E));

            // Open in default viewer
            Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });
        }

        private string BuildVideoPrompt()
        {
            string motionPhrase = _selectedMotionPreset?.PromptText ?? "";
            string custom = _customMotionText?.Trim() ?? "";

            var sb = new System.Text.StringBuilder();
            sb.Append("Animate this architectural image into a video.");

            if (!string.IsNullOrEmpty(motionPhrase))
                sb.Append($" Camera: {motionPhrase}");

            if (!string.IsNullOrEmpty(custom))
                sb.Append($" {custom}");

            if (string.IsNullOrEmpty(motionPhrase) && string.IsNullOrEmpty(custom))
                return "Animate this architectural rendering with gentle ambient motion.";

            return sb.ToString();
        }

        // -- INotifyPropertyChanged --

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
