using System.Collections.Generic;

namespace ReRender.Data
{
    /// <summary>
    /// A named preset that selects one item per prompt category.
    /// Selections map category name to DisplayName (never phrase text).
    /// </summary>
    public class PromptTheme
    {
        public string Name { get; set; } = "";
        public Dictionary<string, string> Selections { get; set; } = new Dictionary<string, string>();
    }
}
