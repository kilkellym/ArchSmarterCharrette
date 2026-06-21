using System.Windows;
using System.Windows.Media.Imaging;

namespace ReRender.UI
{
    public partial class TouchUpDialog : Window
    {
        /// <summary>
        /// The user's touch-up description (only valid when DialogResult is true).
        /// </summary>
        public string TouchUpPrompt => TxtPrompt.Text;

        public TouchUpDialog(string imagePath)
        {
            InitializeComponent();

            // Show a thumbnail preview of the image being edited
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new System.Uri(imagePath, System.UriKind.Absolute);
                bitmap.DecodePixelWidth = 200;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                ImgPreview.Source = bitmap;
            }
            catch
            {
                // If we can't load the thumbnail, just leave it blank
            }

            TxtPrompt.Focus();
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtPrompt.Text))
                return;

            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
