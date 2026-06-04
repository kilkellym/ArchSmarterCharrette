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
            if (sender is System.Windows.Controls.Button btn
                && btn.DataContext is VideoGalleryItem item)
            {
                _viewModel.PlayVideo(item);
            }
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
