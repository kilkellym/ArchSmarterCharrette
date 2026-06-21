using System.Collections.Generic;

namespace ArchSmarterCharrette.Data
{
    /// <summary>
    /// A camera motion preset that maps a user-facing display name
    /// to prompt text that describes the camera movement.
    /// Camera motion is controlled through the prompt — these are
    /// prompt-engineering phrases, not API parameters.
    /// </summary>
    public class CameraMotionPreset
    {
        public string DisplayName { get; set; } = "";
        public string PromptText { get; set; } = "";

        public override string ToString() => DisplayName;

        /// <summary>
        /// Returns the built-in set of camera motion presets.
        /// </summary>
        public static List<CameraMotionPreset> GetDefaults()
        {
            return new List<CameraMotionPreset>
            {
                new CameraMotionPreset
                {
                    DisplayName = "Static",
                    PromptText = "Keep the camera completely still. Add subtle ambient motion like swaying trees, drifting clouds, or flickering light to bring the scene to life."
                },
                new CameraMotionPreset
                {
                    DisplayName = "Orbit Left",
                    PromptText = "Slowly orbit the camera to the left around the building, maintaining a consistent distance and eye level throughout the rotation."
                },
                new CameraMotionPreset
                {
                    DisplayName = "Orbit Right",
                    PromptText = "Slowly orbit the camera to the right around the building, maintaining a consistent distance and eye level throughout the rotation."
                },
                new CameraMotionPreset
                {
                    DisplayName = "Spiral Down",
                    PromptText = "Start from an elevated angle and slowly spiral the camera downward around the building, gradually descending to street level."
                },
                new CameraMotionPreset
                {
                    DisplayName = "Zoom In",
                    PromptText = "Slowly zoom the camera in toward the building entrance, starting from a wide establishing shot and ending on an architectural detail."
                },
                new CameraMotionPreset
                {
                    DisplayName = "Zoom Out",
                    PromptText = "Slowly zoom the camera out from an architectural detail to reveal the full building in its surrounding context."
                },
                new CameraMotionPreset
                {
                    DisplayName = "Pan Right",
                    PromptText = "Slowly pan the camera to the right across the building facade, keeping a steady horizontal movement at eye level."
                },
                new CameraMotionPreset
                {
                    DisplayName = "Pan Left",
                    PromptText = "Slowly pan the camera to the left across the building facade, keeping a steady horizontal movement at eye level."
                },
                new CameraMotionPreset
                {
                    DisplayName = "Tilt Up",
                    PromptText = "Start at the building base and slowly tilt the camera upward, revealing the full height of the structure from ground to roofline."
                },
                new CameraMotionPreset
                {
                    DisplayName = "Fly Through",
                    PromptText = "Move the camera forward in a smooth fly-through motion, approaching and passing through the building entrance."
                },
                new CameraMotionPreset
                {
                    DisplayName = "Aerial Reveal",
                    PromptText = "Start with a close aerial view looking straight down, then slowly pull back and tilt to reveal the full building and its surrounding landscape from above."
                },
                new CameraMotionPreset
                {
                    DisplayName = "Dolly Forward",
                    PromptText = "Slowly dolly the camera forward along a straight path toward the building, keeping the camera at a low angle for a dramatic approach."
                }
            };
        }
    }
}
