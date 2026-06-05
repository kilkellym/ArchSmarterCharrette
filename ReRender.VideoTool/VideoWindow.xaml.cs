using System.Windows;
using ReRender.VideoTool.Data;

namespace ReRender.VideoTool
{
    public partial class VideoWindow : Window
    {
        private readonly VideoWindowViewModel _viewModel;

        /// <summary>
        /// Standalone mode — no image pre-selected. User must browse for a source image.
        /// </summary>
        public VideoWindow()
        {
            InitializeComponent();
            _viewModel = new VideoWindowViewModel(null);
            DataContext = _viewModel;
        }

        /// <summary>
        /// Shortcut mode — image pre-filled (from CLI args or Revit).
        /// </summary>
        public VideoWindow(string renderedImagePath)
        {
            InitializeComponent();
            _viewModel = new VideoWindowViewModel(renderedImagePath);
            DataContext = _viewModel;
        }

        private async void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.GenerateVideoAsync();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CancelGeneration();
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.BrowseForImage();
        }

        private void BtnVideoGalleryItem_Click(object sender, RoutedEventArgs e)
        {
            VideoGalleryItem item = null;

            if (sender is System.Windows.Controls.Button btn
                && btn.DataContext is VideoGalleryItem btnItem)
                item = btnItem;
            else if (sender is System.Windows.Controls.MenuItem menuItem
                && menuItem.Parent is System.Windows.Controls.ContextMenu contextMenu
                && contextMenu.PlacementTarget is System.Windows.FrameworkElement target
                && target.DataContext is VideoGalleryItem menuItemVideo)
                item = menuItemVideo;

            if (item != null)
                _viewModel.PlayVideo(item);
        }

        private void MenuDeleteVideo_Click(object sender, RoutedEventArgs e)
        {
            VideoGalleryItem item = null;

            if (sender is System.Windows.Controls.MenuItem menuItem
                && menuItem.Parent is System.Windows.Controls.ContextMenu contextMenu
                && contextMenu.PlacementTarget is System.Windows.FrameworkElement target
                && target.DataContext is VideoGalleryItem galleryItem)
                item = galleryItem;

            if (item == null)
                return;

            var result = System.Windows.MessageBox.Show(
                $"Delete \"{item.FileName}\"?",
                "Delete Video",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
                _viewModel.DeleteVideoItem(item);
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var settings = new VideoSettingsWindow();
            settings.Owner = this;
            settings.ShowDialog();
        }

        private void BtnToggleGallery_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ToggleGallery();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
