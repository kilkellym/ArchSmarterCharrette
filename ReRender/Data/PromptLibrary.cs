using System.Collections.Generic;

namespace ReRender.Data
{
    /// <summary>
    /// The object-shape prompt library file: contains Items (the phrase list)
    /// and an optional Themes array. Used for deserialization when the JSON root
    /// is an object rather than a plain array.
    /// </summary>
    public class PromptLibrary
    {
        public List<PromptPhrase> Items { get; set; } = new List<PromptPhrase>();
        public List<PromptTheme> Themes { get; set; } = new List<PromptTheme>();
    }
}
