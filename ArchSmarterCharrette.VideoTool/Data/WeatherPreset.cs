namespace ArchSmarterCharrette.VideoTool.Data
{
    /// <summary>
    /// A weather/atmosphere preset that maps a user-facing display name
    /// to prompt text describing atmospheric conditions in the scene.
    /// </summary>
    public class WeatherPreset
    {
        public string DisplayName { get; set; } = "";
        public string PromptText { get; set; } = "";

        public override string ToString() => DisplayName;

        public static List<WeatherPreset> GetDefaults()
        {
            return new List<WeatherPreset>
            {
                new WeatherPreset
                {
                    DisplayName = "None",
                    PromptText = ""
                },
                new WeatherPreset
                {
                    DisplayName = "Clear Sky",
                    PromptText = "Clear sky with crisp visibility. Subtle breeze moves leaves and landscaping gently."
                },
                new WeatherPreset
                {
                    DisplayName = "Partly Cloudy",
                    PromptText = "Partly cloudy sky with drifting clouds casting moving shadows across the building and ground."
                },
                new WeatherPreset
                {
                    DisplayName = "Overcast",
                    PromptText = "Overcast sky with soft, even lighting and a calm, muted atmosphere."
                },
                new WeatherPreset
                {
                    DisplayName = "Light Rain",
                    PromptText = "Add light rain falling visibly in front of the camera. Show wet reflective surfaces on pavement and glass. Small puddles forming on the ground."
                },
                new WeatherPreset
                {
                    DisplayName = "Heavy Rain",
                    PromptText = "Add heavy rain with clearly visible rain streaks and splashing on all surfaces. Show wet reflections on glass and pavement. Dark dramatic overcast sky."
                },
                new WeatherPreset
                {
                    DisplayName = "Fog / Mist",
                    PromptText = "Add thick atmospheric fog drifting through the scene. Distant elements should be partially obscured. Visible haze in the air."
                },
                new WeatherPreset
                {
                    DisplayName = "Snow",
                    PromptText = "Add visible snowflakes falling through the air. Show snow accumulating on horizontal surfaces like ledges, roofs, and the ground."
                },
                new WeatherPreset
                {
                    DisplayName = "Dramatic Clouds",
                    PromptText = "Dramatic, dynamic cloud formations moving across the sky. Strong contrast between light and shadow on the building."
                },
                new WeatherPreset
                {
                    DisplayName = "Windy",
                    PromptText = "Windy conditions with trees, flags, and landscaping swaying noticeably. Clouds moving quickly across the sky."
                }
            };
        }
    }
}
