using System.Windows;
using System.Windows.Controls;

namespace ArchSmarterCharrette.UI
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsWindowViewModel _viewModel;
        private bool _suppressApiKeySync;

        /// <summary>
        /// Set when we hand the settings file off to an external editor, so the
        /// next time this window is activated we pick up whatever was saved.
        /// </summary>
        private bool _reloadOnActivate;
        public SettingsWindow()
        {
            InitializeComponent();
            _viewModel = new SettingsWindowViewModel();
            DataContext = _viewModel;

            // PasswordBox can't be data-bound, so set it from the ViewModel
            ApiKeyPasswordBox.Password = _viewModel.ApiKey;

            Activated += SettingsWindow_Activated;
        }

        private void SettingsWindow_Activated(object sender, System.EventArgs e)
        {
            if (!_reloadOnActivate) return;
            _reloadOnActivate = false;

            _viewModel.ReloadFromDisk();

            // PasswordBox can't be data-bound, so push the reloaded key across by hand
            _suppressApiKeySync = true;
            ApiKeyPasswordBox.Password = _viewModel.ApiKey;
            ApiKeyTextBox.Text = _viewModel.ApiKey;
            _suppressApiKeySync = false;
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

        private void BtnBrowsePromptLibraryFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select folder containing prompt library files",
                SelectedPath = _viewModel.PromptLibraryFolder,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _viewModel.PromptLibraryFolder = dialog.SelectedPath;
            }
        }

        private void BtnEditModels_Click(object sender, RoutedEventArgs e)
        {
            // The model list lives in the settings file alongside every other
            // setting, so reload once the user comes back rather than letting this
            // window's copy overwrite whatever they edited.
            _reloadOnActivate = true;
            OpenInEditor(Data.RenderSettingsManager.GetSettingsFilePath());
        }

        private void BtnEditPromptLibrary_Click(object sender, RoutedEventArgs e)
        {
            OpenInEditor(_viewModel.SelectedLibraryFilePath);
        }

        /// <summary>
        /// Opens a JSON file in whatever the shell has associated with it, falling
        /// back to Notepad on machines where .json has no association.
        /// </summary>
        private void OpenInEditor(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;

            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception)
            {
                try
                {
                    Process.Start("notepad.exe", $"\"{path}\"");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(this,
                        "Couldn't open " + path + Environment.NewLine + Environment.NewLine + ex.Message,
                        "Charrette", System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
