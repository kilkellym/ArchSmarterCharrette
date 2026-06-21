using System.Windows;
using System.Windows.Media;

namespace ArchSmarterCharrette.UI
{
    public partial class RenderWindow : Window
    {
        private readonly RenderWindowViewModel _viewModel;

        /// <summary>
        /// Creates the render window. Pass the path to the exported view image
        /// so the ViewModel can send it to Gemini without touching Revit API.
        /// </summary>
        public RenderWindow(string exportedImagePath)
        {
            InitializeComponent();
            _viewModel = new RenderWindowViewModel(exportedImagePath);
            DataContext = _viewModel;
        }

        /// <summary>
        /// Updates the source image with a fresh export from Revit.
        /// Called when the user clicks the ribbon button while this window is already open.
        /// </summary>
        public void UpdateSourceImage(string exportedImagePath)
        {
            _viewModel.UpdateExportedImage(exportedImagePath);
        }

        private async void BtnRender_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.RenderAsync();
        }

        private void BtnVideo_Click(object sender, RoutedEventArgs e)
        {
            string imagePath = _viewModel.LastRenderedImagePath;
            if (string.IsNullOrEmpty(imagePath))
                return;

            try
            {
                Data.VideoToolLauncher.LaunchWithImage(imagePath);
            }
            catch (Data.VideoGenerationException ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Charrette Video",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        private void BtnSavePreset_Click(object sender, RoutedEventArgs e)
        {
            // Simple input dialog using a WPF prompt
            string defaultName = _viewModel.SelectedPresetName;
            if (defaultName == "(No preset)" || string.IsNullOrWhiteSpace(defaultName))
                defaultName = "";

            var dialog = new SavePresetDialog(defaultName);
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                string name = dialog.PresetName.Trim();
                if (!string.IsNullOrEmpty(name))
                    _viewModel.SaveCurrentAsPreset(name);
            }
        }

        private void BtnDeletePreset_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedPresetName == "(No preset)")
                return;

            System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show(
                $"Delete preset \"{_viewModel.SelectedPresetName}\"?",
                "Delete Preset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
                _viewModel.DeleteCurrentPreset();
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow();
            settings.Owner = this;
            settings.ShowDialog();

            // Settings may have changed the model, prompt library, etc.
            _viewModel.RefreshFromSettings();
        }

        private void BtnToggleGallery_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ToggleGallery();

            // Flip arrow: right-pointing (closed) vs left-pointing (open)
            if (_viewModel.IsGalleryOpen)
                GalleryArrow.Data = Geometry.Parse("M 4 0 L 0 4 L 4 8 Z");
            else
                GalleryArrow.Data = Geometry.Parse("M 0 0 L 4 4 L 0 8 Z");
        }

        private void BtnGalleryImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string filePath)
                _viewModel.OpenGalleryImage(filePath);
        }

        private void BtnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OpenOutputFolder();
        }

        private void BtnClearGallery_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show(
                "Clear all images from the session gallery?",
                "Clear Gallery",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                _viewModel.ClearGallery();
            }
        }

        private void MenuUseSettings_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.MenuItem menuItem
                && menuItem.Parent is System.Windows.Controls.ContextMenu contextMenu
                && contextMenu.PlacementTarget is System.Windows.FrameworkElement target
                && target.DataContext is Data.GalleryItem item)
            {
                _viewModel.ApplyGallerySettings(item);
            }
        }

        private void MenuOpenImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.MenuItem menuItem
                && menuItem.Parent is System.Windows.Controls.ContextMenu contextMenu
                && contextMenu.PlacementTarget is System.Windows.FrameworkElement target
                && target.DataContext is Data.GalleryItem item)
            {
                _viewModel.OpenGalleryImage(item.FilePath);
            }
        }

        private async void MenuTouchUp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.MenuItem menuItem
                && menuItem.Parent is System.Windows.Controls.ContextMenu contextMenu
                && contextMenu.PlacementTarget is System.Windows.FrameworkElement target
                && target.DataContext is Data.GalleryItem item)
            {
                var dialog = new TouchUpDialog(item.FilePath);
                dialog.Owner = this;
                if (dialog.ShowDialog() == true)
                {
                    string prompt = dialog.TouchUpPrompt.Trim();
                    if (!string.IsNullOrEmpty(prompt))
                        await _viewModel.TouchUpAsync(item.FilePath, prompt);
                }
            }
        }

        private void MenuDeleteImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.MenuItem menuItem
                && menuItem.Parent is System.Windows.Controls.ContextMenu contextMenu
                && contextMenu.PlacementTarget is System.Windows.FrameworkElement target
                && target.DataContext is Data.GalleryItem item)
            {
                var result = System.Windows.MessageBox.Show(
                    $"Delete \"{item.FileName}\"?",
                    "Delete Image",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                    _viewModel.DeleteGalleryItem(item);
            }
        }

        private void BtnViewPrompt_Click(object sender, RoutedEventArgs e)
        {
            string prompt = _viewModel.GetPromptPreview();
            var preview = new PromptPreviewWindow(prompt);
            preview.Owner = this;
            preview.ShowDialog();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
