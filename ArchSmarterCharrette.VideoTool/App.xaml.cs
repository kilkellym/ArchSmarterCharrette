using System.Windows;

namespace ArchSmarterCharrette.VideoTool
{
    public partial class App : Application
    {
        /// <summary>
        /// Optional parsed command-line arguments.
        /// Null when the app is launched standalone (no args).
        /// </summary>
        public static VideoArgs Args { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Parse CLI args if any were provided
            if (e.Args.Length > 0)
            {
                Args = VideoArgs.Parse(e.Args);
            }

            // Create the main window
            // If --image was provided, pre-fill the source image
            VideoWindow window;
            if (Args != null && !string.IsNullOrEmpty(Args.ImagePath))
            {
                window = new VideoWindow(Args.ImagePath);
            }
            else
            {
                window = new VideoWindow();
            }

            MainWindow = window;
            window.Show();
        }
    }
}
