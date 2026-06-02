using System.Collections.Generic;

namespace ReRender.Data
{
    public class RenderSettings
    {
        public string OutputFolder { get; set; } = "";
        public string GeminiApiKey { get; set; } = "";
        public string ApiEndpoint { get; set; } = "v1beta";
        public string ModelName { get; set; } = "gemini-2.5-flash-image";
        public string PromptLibraryFolder { get; set; } = "";
        public string PromptLibraryFile { get; set; } = "ReRender_PromptLibrary.json";
        public List<string> AvailableModels { get; set; } = new List<string>
        {
            "gemini-2.5-flash-image",
            "gemini-3.1-flash-image",
            "gemini-3-pro-image"
        };
        public string ImageSize { get; set; } = "1K";
        public List<string> AvailableImageSizes { get; set; } = new List<string>
        {
            "512",
            "1K",
            "2K",
            "4K"
        };
        public string AspectRatio { get; set; } = "Default";
        public List<string> AvailableAspectRatios { get; set; } = new List<string>
        {
            "Default",
            "1:1",
            "3:2",
            "2:3",
            "4:3",
            "3:4",
            "16:9",
            "9:16",
            "21:9"
        };
    }
}
