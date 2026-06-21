using System.Collections.Generic;

namespace ArchSmarterCharrette.Data
{
    /// <summary>
    /// Holds the list of rendered image paths for the current Revit session.
    /// Static state persists across RenderWindow open/close cycles and resets
    /// when Revit restarts (since the assembly is unloaded).
    /// </summary>
    /// <summary>
    /// One entry in the session history: the output file path and the
    /// render settings that produced it.
    /// </summary>
    public class SessionHistoryEntry
    {
        public string FilePath { get; }
        public RenderPreset Settings { get; }

        public SessionHistoryEntry(string filePath, RenderPreset settings)
        {
            FilePath = filePath;
            Settings = settings;
        }
    }

    public static class SessionHistory
    {
        private static readonly List<SessionHistoryEntry> _entries = new List<SessionHistoryEntry>();

        public static IReadOnlyList<SessionHistoryEntry> Entries => _entries;

        public static void Add(string path, RenderPreset settings)
        {
            _entries.Add(new SessionHistoryEntry(path, settings));
        }

        public static void Clear()
        {
            _entries.Clear();
        }
    }
}
