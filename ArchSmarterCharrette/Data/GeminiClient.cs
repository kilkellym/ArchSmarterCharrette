using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArchSmarterCharrette.Data
{
    /// <summary>
    /// Calls the Gemini REST API to render an architectural image.
    /// Sends the exported view as a base64-encoded image plus the assembled text prompt.
    /// Returns the generated image bytes, or throws on failure.
    /// </summary>
    public class GeminiClient
    {
        private static readonly HttpClient Http = new HttpClient();

        private readonly string _apiKey;
        private readonly string _modelName;
        private readonly string _apiEndpoint;

        public GeminiClient(string apiKey, string modelName, string apiEndpoint = "v1beta")
        {
            _apiKey = apiKey;
            _modelName = modelName;
            _apiEndpoint = apiEndpoint;
        }

        /// <summary>
        /// Sends the source image and prompt to Gemini and returns the rendered image bytes.
        /// </summary>
        /// <param name="imageBytes">The exported view image (PNG or JPEG).</param>
        /// <param name="mimeType">MIME type of the image, e.g. "image/png".</param>
        /// <param name="prompt">The assembled text prompt from PromptBuilder.</param>
        /// <returns>The rendered image as a byte array (PNG).</returns>
        /// <param name="imageSize">Image size (e.g. "1K", "2K"). Empty string omits the parameter.</param>
        /// <param name="aspectRatio">Aspect ratio (e.g. "16:9"). Empty string omits the parameter.</param>
        public async Task<byte[]> RenderAsync(byte[] imageBytes, string mimeType, string prompt,
            string imageSize = "", string aspectRatio = "")
        {
            string url = $"https://generativelanguage.googleapis.com/{_apiEndpoint}/models/{_modelName}:generateContent?key={_apiKey}";

            string base64Image = Convert.ToBase64String(imageBytes);

            // Normalize "Default" to empty so it's omitted from the API call
            if (string.Equals(aspectRatio, "Default", StringComparison.OrdinalIgnoreCase))
                aspectRatio = "";

            // Build the image config options only if specified
            Dictionary<string, object> imageConfig = null;
            if (!string.IsNullOrEmpty(imageSize) || !string.IsNullOrEmpty(aspectRatio))
            {
                imageConfig = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(imageSize))
                    imageConfig["imageSize"] = imageSize;
                if (!string.IsNullOrEmpty(aspectRatio))
                    imageConfig["aspectRatio"] = aspectRatio;
            }

            // Build generationConfig
            var generationConfig = new Dictionary<string, object>
            {
                { "responseModalities", new[] { "TEXT", "IMAGE" } }
            };

            if (imageConfig != null)
            {
                generationConfig["imageConfig"] = imageConfig;
            }

            // Build the request payload
            var requestBody = new Dictionary<string, object>
            {
                {
                    "contents", new[]
                    {
                        new Dictionary<string, object>
                        {
                            {
                                "parts", new object[]
                                {
                                    new Dictionary<string, string> { { "text", prompt } },
                                    new Dictionary<string, object>
                                    {
                                        {
                                            "inline_data", new Dictionary<string, string>
                                            {
                                                { "mime_type", mimeType },
                                                { "data", base64Image }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                { "generationConfig", generationConfig }
            };

            string jsonPayload = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await Http.PostAsync(url, content);
            string responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new GeminiException(
                    $"Gemini API returned {(int)response.StatusCode}: {response.ReasonPhrase}",
                    responseJson);
            }

            // Parse the response to extract the generated image
            return ExtractImageFromResponse(responseJson);
        }

        /// <summary>
        /// Walks the Gemini JSON response to find the first inline_data image part
        /// and returns its bytes.
        /// </summary>
        private static byte[] ExtractImageFromResponse(string responseJson)
        {
            using JsonDocument doc = JsonDocument.Parse(responseJson);
            JsonElement root = doc.RootElement;

            // Navigate: candidates[0].content.parts[*].inlineData.data
            if (!root.TryGetProperty("candidates", out JsonElement candidates) ||
                candidates.GetArrayLength() == 0)
            {
                throw new GeminiException(
                    "Gemini response contained no candidates.",
                    responseJson);
            }

            JsonElement firstCandidate = candidates[0];
            if (!firstCandidate.TryGetProperty("content", out JsonElement contentEl) ||
                !contentEl.TryGetProperty("parts", out JsonElement parts))
            {
                throw new GeminiException(
                    "Gemini response candidate had no content parts.",
                    responseJson);
            }

            foreach (JsonElement part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("inlineData", out JsonElement inlineData) &&
                    inlineData.TryGetProperty("data", out JsonElement dataEl))
                {
                    string base64 = dataEl.GetString();
                    if (!string.IsNullOrEmpty(base64))
                    {
                        return Convert.FromBase64String(base64);
                    }
                }
            }

            throw new GeminiException(
                "Gemini response did not contain an image.",
                responseJson);
        }
    }

    /// <summary>
    /// Represents an error from the Gemini API or response parsing.
    /// Carries the raw JSON response for diagnostic display.
    /// </summary>
    public class GeminiException : Exception
    {
        public string ResponseJson { get; }

        public GeminiException(string message, string responseJson)
            : base(message)
        {
            ResponseJson = responseJson;
        }
    }
}
