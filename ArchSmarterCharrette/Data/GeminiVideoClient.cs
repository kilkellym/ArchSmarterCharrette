namespace ReRender.Data
{
    /// <summary>
    /// Launches the standalone ReRender.VideoTool.exe process.
    /// The external tool runs on .NET 10 to avoid assembly conflicts
    /// with Revit's .NET 8 runtime.
    /// </summary>
    public static class VideoToolLauncher
    {
        /// <summary>
        /// Launches the video tool with no arguments (standalone mode).
        /// </summary>
        public static void Launch()
        {
            string toolPath = GetVideoToolPath();
            ValidateToolPath(toolPath);

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = toolPath,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            System.Diagnostics.Process.Start(startInfo);
        }

        /// <summary>
        /// Launches the video tool with a source image pre-filled.
        /// </summary>
        public static void LaunchWithImage(string imagePath)
        {
            string toolPath = GetVideoToolPath();
            ValidateToolPath(toolPath);

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = toolPath,
                Arguments = $"--image \"{imagePath}\"",
                UseShellExecute = false,
                CreateNoWindow = false
            };

            System.Diagnostics.Process.Start(startInfo);
        }

        private static void ValidateToolPath(string toolPath)
        {
            if (!System.IO.File.Exists(toolPath))
            {
                throw new VideoGenerationException(
                    $"Video tool not found at:\n{toolPath}\n\nMake sure ReRender.VideoTool is built.");
            }
        }

        /// <summary>
        /// Finds the video tool executable. Checks:
        /// 1. Next to the add-in DLL (deployed)
        /// 2. Sibling project build output (development)
        /// </summary>
        private static string GetVideoToolPath()
        {
            string addinFolder = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";

            // Check next to add-in DLL first (deployed scenario)
            string deployedPath = System.IO.Path.Combine(addinFolder, "ReRender.VideoTool.exe");
            if (System.IO.File.Exists(deployedPath))
                return deployedPath;

            // Check sibling project build output (development scenario)
            // addinFolder is something like ...\ReRender\bin\Debug\2025
            // VideoTool output is ...\ReRender.VideoTool\bin\Debug\net10.0-windows
            string projectRoot = System.IO.Path.GetFullPath(
                System.IO.Path.Combine(addinFolder, "..", "..", "..", ".."));
            string devPath = System.IO.Path.Combine(projectRoot,
                "ReRender.VideoTool", "bin", "Debug", "net10.0-windows", "ReRender.VideoTool.exe");
            if (System.IO.File.Exists(devPath))
                return devPath;

            // Return the deployed path for the error message
            return deployedPath;
        }
    }
}
