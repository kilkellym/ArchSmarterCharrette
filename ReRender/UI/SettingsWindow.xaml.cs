using System.Windows;
using System.Windows.Controls;

namespace ReRender.UI
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsWindowViewModel _viewModel;
        private bool _suppressApiKeySync;

        public SettingsWindow()
        {
            InitializeComponent();
            _viewModel = new SettingsWindowViewModel();
            DataContext = _viewModel;

            // PasswordBox can't be data-bound, so set it from the ViewModel
            ApiKeyPasswordBox.Password = _viewModel.ApiKey;
        }

        // -- API Key show/hide toggle --

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

            if (ApiKeyPasswordBox.Visibility == System.Windows.Visibility.Visible)
            {
                // Switch to plain-text view
                ApiKeyTextBox.Text = ApiKeyPasswordBox.Password;
                ApiKeyPasswordBox.Visibility = System.Windows.Visibility.Collapsed;
                ApiKeyTextBox.Visibility = System.Windows.Visibility.Visible;
                BtnToggleApiKey.Content = "Hide";
            }
            else
            {
                // Switch back to masked view
                ApiKeyPasswordBox.Password = ApiKeyTextBox.Text;
                ApiKeyTextBox.Visibility = System.Windows.Visibility.Collapsed;
                ApiKeyPasswordBox.Visibility = System.Windows.Visibility.Visible;
                BtnToggleApiKey.Content = "Show";
            }

            _suppressApiKeySync = false;
        }

        private void BtnBrowseOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select folder for rendered images",
                SelectedPath = _viewModel.OutputFolder,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _viewModel.OutputFolder = dialog.SelectedPath;
            }
        }

        private void BtnEditPromptLibrary_Click(object sender, RoutedEventArgs e)
        {
            string path = _viewModel.SelectedLibraryFilePath;
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
