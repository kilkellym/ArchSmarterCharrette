namespace ArchSmarterCharrette.Data
{
    /// <summary>
    /// Represents an error from the video generation process.
    /// </summary>
    public class VideoGenerationException : Exception
    {
        public string ResponseBody { get; }

        public VideoGenerationException(string message, string responseBody = "")
            : base(message)
        {
            ResponseBody = responseBody;
        }
    }
}
