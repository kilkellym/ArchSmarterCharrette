namespace ReRender.Helpers
{
    /// <summary>
    /// Exports the active Revit view to a temporary PNG file on disk.
    /// This is separated from the command so the export logic is reusable
    /// and the command stays lean.
    /// </summary>
    internal static class ViewExporter
    {
        /// <summary>
        /// Exports the given view to a PNG file in the system temp folder.
        /// Returns the full path to the exported image.
        /// </summary>
        public static string ExportViewToTempPng(Document doc, View view)
        {
            string tempFolder = Path.GetTempPath();
            string baseName = $"ReRender_{view.Id.Value}";

            var options = new ImageExportOptions
            {
                FilePath = Path.Combine(tempFolder, baseName),
                FitDirection = FitDirectionType.Horizontal,
                ZoomType = ZoomFitType.FitToPage,
                ImageResolution = ImageResolution.DPI_300,
                HLRandWFViewsFileType = ImageFileType.PNG,
                ShadowViewsFileType = ImageFileType.PNG,
                PixelSize = 2048,
                ExportRange = ExportRange.CurrentView
            };

            doc.ExportImage(options);

            // Revit appends the view name and extension to the base path.
            // Find the file that was actually created.
            string exportedFile = FindExportedFile(tempFolder, baseName);

            if (exportedFile == null)
            {
                throw new InvalidOperationException(
                    "Revit did not produce an exported image. " +
                    "Make sure the active view can be exported (not a schedule or sheet without views).");
            }

            return exportedFile;
        }

        /// <summary>
        /// Revit's ExportImage appends the view name to the base filename.
        /// We search the temp folder for the file that was just created.
        /// </summary>
        private static string FindExportedFile(string folder, string baseName)
        {
            string[] candidates = Directory.GetFiles(folder, baseName + "*.png");

            if (candidates.Length == 0)
                return null;

            // If multiple matches, pick the most recently written one
            string best = null;
            DateTime newest = DateTime.MinValue;

            foreach (string path in candidates)
            {
                DateTime written = File.GetLastWriteTime(path);
                if (written > newest)
                {
                    newest = written;
                    best = path;
                }
            }

            return best;
        }
    }
}
