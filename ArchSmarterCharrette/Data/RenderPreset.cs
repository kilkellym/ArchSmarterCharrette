using System.Collections.Generic;

namespace ArchSmarterCharrette.Data
{
    /// <summary>
    /// A saved render configuration: category selections, custom directions,
    /// and output settings. Selections map category name to DisplayName
    /// (same convention as PromptTheme — never stores phrase text).
    /// </summary>
    public class RenderPreset
    {
        public string Name { get; set; } = "";
        public Dictionary<string, string> Selections { get; set; } = new Dictionary<string, string>();
        public string CustomDirections { get; set; } = "";
        public string ImageSize { get; set; } = "";
        public string AspectRatio { get; set; } = "";
    }
}
