using System.Windows.Media.Imaging;

namespace ReRender.Data
{
    /// <summary>
    /// Wraps a rendered image file path for display in the session gallery.
    /// Loads a small thumbnail to avoid holding the full image in memory.
    /// </summary>
    public class GalleryItem
    {
        public string FilePath { get; }
        public string FileName { get; }
        public BitmapImage Thumbnail { get; }
        public RenderPreset Settings { get; }

        public GalleryItem(string filePath, RenderPreset settings = null)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            Thumbnail = LoadThumbnail(filePath);
            Settings = settings;
        }

        private static BitmapImage LoadThumbnail(string filePath)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmap.DecodePixelWidth = 140;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
