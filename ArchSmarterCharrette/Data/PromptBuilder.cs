using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArchSmarterCharrette.Data
{
    /// <summary>
    /// Assembles the text prompt sent to Gemini alongside the exported view image.
    /// The adherence preamble is always emitted first to lock geometry, composition,
    /// and viewpoint to the source image. Art-direction phrases are appended only
    /// when the user picks a non-default option (i.e. a phrase with non-empty text).
    /// </summary>
    public static class PromptBuilder
    {
        private static readonly string AdherencePreamble =
            "You are an architectural rendering engine. " +
            "I am providing an exported building view. " +
            "Preserve the exact geometry, spatial composition, camera angle, and perspective of this image. " +
            "Do not add, remove, or reposition any building elements. " +
            "Do not change the proportions or layout of the structure. " +
            "Render the image with the following style instructions:";

        private static readonly string DefaultSuffix =
            "Apply realistic materials, natural lighting, and a clean background " +
            "that complement the architectural design as shown.";

        /// <summary>
        /// Builds the full prompt from the user's phrase selections.
        /// Pass the PromptPhrase objects the user selected from each dropdown.
        /// Phrases with empty text are no-op defaults and are skipped.
        /// </summary>
        public static string Build(IEnumerable<PromptPhrase> selectedPhrases, string customDirections = "")
        {
            var sb = new StringBuilder();
            sb.Append(AdherencePreamble);

            List<string> activePhrases = selectedPhrases
                .Where(p => !string.IsNullOrWhiteSpace(p.Phrase))
                .Select(p => p.Phrase.Trim())
                .ToList();

            if (activePhrases.Count == 0)
            {
                // All defaults selected — strict adherence with a clean render
                sb.Append(" ");
                sb.Append(DefaultSuffix);
            }
            else
            {
                // Append each art-direction phrase as its own sentence
                foreach (string phrase in activePhrases)
                {
                    sb.Append(" ");
                    sb.Append(phrase);
                }
            }

            // Append custom directions last so they get the final word
            if (!string.IsNullOrWhiteSpace(customDirections))
            {
                sb.Append(" ");
                sb.Append(customDirections.Trim());
            }

            return sb.ToString();
        }
    }
}
