namespace ArchSmarterCharrette.VideoTool.Data
{
    /// <summary>
    /// A scene activity preset that maps a user-facing display name
    /// to prompt text describing people, vehicles, and life in the scene.
    /// </summary>
    public class SceneActivityPreset
    {
        public string DisplayName { get; set; } = "";
        public string PromptText { get; set; } = "";

        public override string ToString() => DisplayName;

        public static List<SceneActivityPreset> GetDefaults()
        {
            return new List<SceneActivityPreset>
            {
                new SceneActivityPreset
                {
                    DisplayName = "None",
                    PromptText = ""
                },
                new SceneActivityPreset
                {
                    DisplayName = "Empty / Still",
                    PromptText = "The scene is empty and still with no people or vehicles. Only subtle ambient motion like swaying vegetation."
                },
                new SceneActivityPreset
                {
                    DisplayName = "A Few Pedestrians",
                    PromptText = "Add 2-3 people walking near the building entrance and along the sidewalk. They should be moving naturally."
                },
                new SceneActivityPreset
                {
                    DisplayName = "Busy Sidewalk",
                    PromptText = "Add many pedestrians walking on the sidewalks in both directions. Show a busy, active urban scene with people in motion."
                },
                new SceneActivityPreset
                {
                    DisplayName = "Light Traffic",
                    PromptText = "Add cars driving slowly on the road near the building. Include a few pedestrians on the sidewalks."
                },
                new SceneActivityPreset
                {
                    DisplayName = "Busy Street",
                    PromptText = "Add heavy traffic with multiple cars, cyclists, and many pedestrians. Show a vibrant, busy urban street scene."
                },
                new SceneActivityPreset
                {
                    DisplayName = "Parked Cars Only",
                    PromptText = "Parked cars line the street and fill the parking area. No moving traffic or pedestrians."
                },
                new SceneActivityPreset
                {
                    DisplayName = "Outdoor Dining",
                    PromptText = "People seated at outdoor tables near the building entrance, dining and conversing. A lively but relaxed atmosphere."
                },
                new SceneActivityPreset
                {
                    DisplayName = "Construction Activity",
                    PromptText = "Active construction zone with workers, equipment, and safety barriers visible around the building."
                }
            };
        }
    }
}
