using System;

namespace TTV.Config
{
    public class AppConfig
    {
        public string UserId { get; set; }
        public string Token { get; set; }
        public string StoryId { get; set; }
        public string OutputFileName { get; set; }
        public int StartChapter { get; set; }
        public int EndChapter { get; set; }
        public GeminiConfig Gemini { get; set; }
    }

    public class GeminiConfig
    {
        public string ApiKey { get; set; }
        public string ModelId { get; set; }

        public bool IsConfigured => !string.IsNullOrEmpty(ApiKey);
    }
}
