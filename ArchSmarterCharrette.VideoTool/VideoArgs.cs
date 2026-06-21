namespace ArchSmarterCharrette.VideoTool
{
    /// <summary>
    /// Parses command-line arguments for pre-filling the video window.
    /// All args are optional — the app works fully standalone without any.
    /// Supported args: --image &lt;path&gt;
    /// </summary>
    public class VideoArgs
    {
        public string ImagePath { get; set; } = "";

        /// <summary>
        /// Parses command-line args in --key value format.
        /// Returns null if no recognized args are found.
        /// </summary>
        public static VideoArgs Parse(string[] args)
        {
            if (args == null || args.Length == 0)
                return null;

            var result = new VideoArgs();
            bool foundAny = false;

            for (int i = 0; i < args.Length - 1; i += 2)
            {
                string key = args[i].ToLowerInvariant();
                string value = args[i + 1];

                switch (key)
                {
                    case "--image":
                        result.ImagePath = value;
                        foundAny = true;
                        break;
                }
            }

            return foundAny ? result : null;
        }
    }
}
