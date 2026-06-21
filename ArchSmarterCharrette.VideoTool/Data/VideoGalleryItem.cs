using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace ArchSmarterCharrette.VideoTool.Data
{
    /// <summary>
    /// Wraps a generated video file for display in the video gallery sidebar.
    /// Looks for a companion .thumb.png file saved alongside the video during generation.
    /// If no thumbnail exists, Thumbnail will be null and the UI shows a placeholder.
    /// </summary>
    public class VideoGalleryItem
    {
        public string FilePath { get; }
        public string FileName { get; }
        public string FileSize { get; }
        public string CreatedDate { get; }
        public BitmapImage Thumbnail { get; }
        public bool HasThumbnail => Thumbnail != null;

        public VideoGalleryItem(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);

            try
            {
                var info = new FileInfo(filePath);
                FileSize = FormatFileSize(info.Length);
                CreatedDate = info.LastWriteTime.ToString("MMM d, h:mm tt");
            }
            catch
            {
                FileSize = "";
                CreatedDate = "";
            }

            Thumbnail = LoadCompanionThumbnail(filePath);
        }

        /// <summary>
        /// Opens the video in the system default player.
        /// </summary>
        public void Play()
        {
            try
            {
                Process.Start(new ProcessStartInfo(FilePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening video: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the companion thumbnail path for a video file.
        /// e.g., ArchSmarterCharrette_Video_20250604_123456.mp4 → ArchSmarterCharrette_Video_20250604_123456.thumb.png
        /// </summary>
        public static string GetThumbnailPath(string videoPath)
        {
            string dir = Path.GetDirectoryName(videoPath) ?? "";
            string name = Path.GetFileNameWithoutExtension(videoPath);
            return Path.Combine(dir, $"{name}.thumb.png");
        }

        /// <summary>
        /// Saves a source image as a small companion thumbnail for a video file.
        /// Called after video generation completes.
        /// </summary>
        public static void SaveThumbnail(string videoPath, string sourceImagePath)
        {
            try
            {
                string thumbPath = GetThumbnailPath(videoPath);

                // Load the source image and save a small version
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(sourceImagePath, UriKind.Absolute);
                bitmap.DecodePixelWidth = 160;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                using var stream = System.IO.File.Create(thumbPath);
                encoder.Save(stream);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving video thumbnail: {ex.Message}");
                // Non-fatal — the gallery will just show a placeholder
            }
        }

        private static BitmapImage LoadCompanionThumbnail(string videoPath)
        {
            try
            {
                string thumbPath = GetThumbnailPath(videoPath);
                if (!System.IO.File.Exists(thumbPath))
                    return null;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(thumbPath, UriKind.Absolute);
                bitmap.DecodePixelWidth = 160;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F0} KB";
            return $"{bytes / (1024.0 * 1024.0):F1} MB";
        }
    }
}
