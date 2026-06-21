namespace ArchSmarterCharrette.VideoTool.Data
{
    /// <summary>
    /// A mood/style preset that maps a user-facing display name
    /// to prompt text describing the visual style and cinematic feel of the video.
    /// </summary>
    public class MoodPreset
    {
        public string DisplayName { get; set; } = "";
        public string PromptText { get; set; } = "";

        public override string ToString() => DisplayName;

        public static List<MoodPreset> GetDefaults()
        {
            return new List<MoodPreset>
            {
                new MoodPreset
                {
                    DisplayName = "None",
                    PromptText = ""
                },
                new MoodPreset
                {
                    DisplayName = "Cinematic",
                    PromptText = "Apply a cinematic look with shallow depth of field, visible film grain, and smooth motion blur. Professional architectural film quality."
                },
                new MoodPreset
                {
                    DisplayName = "Documentary",
                    PromptText = "Documentary-style footage with natural, observational framing. Clean and informative visual approach."
                },
                new MoodPreset
                {
                    DisplayName = "Drone Footage",
                    PromptText = "Aerial drone footage style with smooth, sweeping movements. Wide perspective showing the building in its full context."
                },
                new MoodPreset
                {
                    DisplayName = "Timelapse",
                    PromptText = "Create a timelapse effect. Clouds should move rapidly across the sky, shadows should shift quickly, and any people or vehicles should move at high speed."
                },
                new MoodPreset
                {
                    DisplayName = "Photorealistic",
                    PromptText = "Photorealistic rendering with accurate materials, natural lighting, and true-to-life proportions. No stylization."
                },
                new MoodPreset
                {
                    DisplayName = "Warm & Inviting",
                    PromptText = "Warm color temperature with inviting, welcoming atmosphere. Soft golden tones and gentle lighting."
                },
                new MoodPreset
                {
                    DisplayName = "Cool & Modern",
                    PromptText = "Cool color temperature with a sleek, modern aesthetic. Clean lines emphasized with crisp, neutral tones."
                },
                new MoodPreset
                {
                    DisplayName = "Dramatic",
                    PromptText = "Dramatic and bold visual style with strong contrast, deep shadows, and striking compositions."
                },
                new MoodPreset
                {
                    DisplayName = "Soft & Ethereal",
                    PromptText = "Soft, dreamy atmosphere with gentle diffusion, pastel tones, and an ethereal quality to the light."
                }
            };
        }
    }
}
