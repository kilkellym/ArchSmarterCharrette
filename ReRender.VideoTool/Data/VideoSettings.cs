namespace ReRender.VideoTool.Data
{
    /// <summary>
    /// Mirrors the video-related fields from the shared ReRender.json settings file.
    /// This class is deserialized from the same file that the Revit add-in uses,
    /// so property names must match exactly.
    /// </summary>
    public class VideoSettings
    {
        public string OutputFolder { get; set; } = "";
        public string VideoOutputFolder { get; set; } = "";
        public string GeminiApiKey { get; set; } = "";

        // -- Video generation settings (Google Veo) --

        public string VideoModel { get; set; } = "veo-3.1-generate-preview";
        public List<string> AvailableVideoModels { get; set; } = new List<string>
        {
            "veo-3.1-generate-preview",
            "veo-3.1-fast-generate-preview",
            "veo-3.0-generate-001",
            "veo-2.0-generate-001"
        };
        public string VideoResolution { get; set; } = "720p";
        public List<string> AvailableVideoResolutions { get; set; } = new List<string>
        {
            "720p",
            "1080p",
            "4K"
        };
        public int VideoDuration { get; set; } = 8;
        public List<int> AvailableVideoDurations { get; set; } = new List<int> { 4, 6, 8 };
    }
}
