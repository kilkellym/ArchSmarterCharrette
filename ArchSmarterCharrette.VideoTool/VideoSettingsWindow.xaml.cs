using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using ArchSmarterCharrette.VideoTool.Data;

namespace ArchSmarterCharrette.VideoTool
{
    public partial class VideoSettingsWindow : Window
    {
        private readonly VideoSettingsViewModel _viewModel;
        private bool _suppressApiKeySync;

        public VideoSettingsWindow()
        {
            InitializeComponent();
            _viewModel = new VideoSettingsViewModel();
            DataContext = _viewModel;

            ApiKeyPasswordBox.Password = _viewModel.ApiKey;
        }

        private void ApiKeyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_suppressApiKeySync) return;
            _viewModel.ApiKey = ApiKeyPasswordBox.Password;
        }

        private void ApiKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressApiKeySync) return;
            _viewModel.ApiKey = ApiKeyTextBox.Text;
        }

        private void BtnToggleApiKey_Click(object sender, RoutedEventArgs e)
        {
            _suppressApiKeySync = true;

            if (ApiKeyPasswordBox.Visibility == Visibility.Visible)
            {
                ApiKeyTextBox.Text = ApiKeyPasswordBox.Password;
                ApiKeyPasswordBox.Visibility = Visibility.Collapsed;
                ApiKeyTextBox.Visibility = Visibility.Visible;
                BtnToggleApiKey.Content = "Hide";
            }
            else
            {
                ApiKeyPasswordBox.Password = ApiKeyTextBox.Text;
                ApiKeyTextBox.Visibility = Visibility.Collapsed;
                ApiKeyPasswordBox.Visibility = Visibility.Visible;
                BtnToggleApiKey.Content = "Show";
            }

            _suppressApiKeySync = false;
        }

        private void BtnBrowseOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select folder for rendered images"
            };

            if (dialog.ShowDialog() == true)
                _viewModel.OutputFolder = dialog.FolderName;
        }

        private void BtnBrowseVideoOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select folder for generated videos"
            };

            if (dialog.ShowDialog() == true)
                _viewModel.VideoOutputFolder = dialog.FolderName;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class VideoSettingsViewModel : INotifyPropertyChanged
    {
        private readonly VideoSettingsManager _settingsManager;

        public VideoSettingsViewModel()
        {
            _settingsManager = new VideoSettingsManager();
            _apiKey = _settingsManager.GetGeminiApiKey();
            _outputFolder = _settingsManager.GetOutputFolder();
            _videoOutputFolder = _settingsManager.GetVideoOutputFolder();
        }

        private string _apiKey;
        public string ApiKey
        {
            get => _apiKey;
            set
            {
                _apiKey = value;
                OnPropertyChanged();
                _settingsManager.SetGeminiApiKey(value);
            }
        }

        private string _outputFolder;
        public string OutputFolder
        {
            get => _outputFolder;
            set
            {
                _outputFolder = value;
                OnPropertyChanged();
                _settingsManager.SetOutputFolder(value);
            }
        }

        private string _videoOutputFolder;
        public string VideoOutputFolder
        {
            get => _videoOutputFolder;
            set
            {
                _videoOutputFolder = value;
                OnPropertyChanged();
                _settingsManager.SetVideoOutputFolder(value);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
