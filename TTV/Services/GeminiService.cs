using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TTV.Config;
using TTV.Error;
using System.Text.Json;

namespace TTV.Services
{
    public interface IGeminiService
    {
        Task<string> EnhanceContentAsync(string content);
        Task EnhanceContentStreamAsync(string content, Action<string> onChunk);
        bool IsConfigured { get; }
    }

    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _modelId;
        private const float Temperature = 0.7f;
        private const float TopP = 0.95f;
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/";

        private const string SystemInstruction = @"
Bạn là một biên tập viên chuyên nghiệp, am hiểu sâu sắc về văn học mạng Trung Quốc, đặc biệt là các thể loại tiên hiệp, kiếm hiệp, và huyền huyễn. Nhiệm vụ của bạn là nhận một đoạn văn bản tiếng Việt được 'convert' (dịch thô bằng máy) và chỉnh sửa nó thành một phiên bản mượt mà, tự nhiên, giàu cảm xúc và đúng ngữ pháp, phù hợp với văn phong của người Việt.

QUY TẮC BẮT BUỘC:
1.  **Bảo toàn nội dung:** Giữ nguyên 100% cốt truyện, tình tiết, nhân vật, và các thuật ngữ đặc trưng như tên riêng, địa danh, cấp bậc tu luyện, công pháp, pháp bảo.
2.  **Việt hóa ngôn ngữ:** Sửa lỗi ngữ pháp, lặp từ, và cấu trúc câu lủng củng. Dùng từ ngữ phong phú, uyển chuyển để diễn đạt ý.
3.  **Làm mượt câu văn:** Chuyển đổi các câu văn khô khan, thiếu chủ ngữ/vị ngữ thành các câu văn hoàn chỉnh, dễ đọc và hấp dẫn.
4.  **Không sáng tạo:** Tuyệt đối không được thêm, bớt hay thay đổi bất kỳ tình tiết nào của câu chuyện. Chỉ tập trung vào việc chau chuốt ngôn từ.
5.  **Trả về chỉ văn bản:** Chỉ trả về duy nhất phần văn bản đã được biên tập, không thêm bất kỳ lời giải thích, ghi chú hay tiêu đề nào.";

        public bool IsConfigured => !string.IsNullOrEmpty(_apiKey);

        public GeminiService(GeminiConfig config)
        {
            _apiKey = config?.ApiKey;
            _modelId = config?.ModelId ?? "gemini-2.0-flash";
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
        }

        public async Task<string> EnhanceContentAsync(string content)
        {
            if (!IsConfigured)
                return content;

            try
            {
                var request = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[]
                            {
                                new { text = $"{SystemInstruction}\n\nNội dung cần làm mượt:\n{content}" }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = Temperature,
                        topP = TopP,
                        stopSequences = new string[] { }
                    }
                };

                var response = await _httpClient.PostAsync(
                    $"{_modelId}:generateContent?key={_apiKey}",
                    new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
                );

                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GeminiResponse>(responseContent);

                return result?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? content;
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("401") || ex.Message.Contains("403"))
                    throw ErrorMessages.Api.CreateInvalidApiKey();
                if (ex.Message.Contains("429"))
                    throw ErrorMessages.Api.CreateQuotaExceeded();
                if (ex.Message.Contains("timeout") || ex.InnerException is System.Net.Sockets.SocketException)
                    throw ErrorMessages.Api.CreateNetworkError();
                
                throw ErrorMessages.Enhancement.CreateEnhancementFailed();
            }
            catch (Exception)
            {
                throw ErrorMessages.Enhancement.CreateEnhancementFailed();
            }
        }

        public async Task EnhanceContentStreamAsync(string content, Action<string> onChunk)
        {
            if (!IsConfigured)
            {
                onChunk(content);
                return;
            }

            try
            {
                var request = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[]
                            {
                                new { text = $"{SystemInstruction}\n\nNội dung cần làm mượt:\n{content}" }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = Temperature,
                        topP = TopP,
                        stopSequences = new string[] { }
                    }
                };

                var response = await _httpClient.PostAsync(
                    $"{_modelId}:streamGenerateContent?key={_apiKey}",
                    new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
                );

                response.EnsureSuccessStatusCode();
                var stream = await response.Content.ReadAsStreamAsync();
                var reader = new StreamReader(stream);

                var buffer = new StringBuilder();
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrEmpty(line)) continue;

                    var chunk = JsonSerializer.Deserialize<GeminiStreamResponse>(line);
                    if (chunk?.Candidates?[0]?.Content?.Parts?[0]?.Text is string text)
                    {
                        buffer.Append(text);
                        onChunk(text);
                    }
                }

                if (buffer.Length == 0)
                    onChunk(content);
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("401") || ex.Message.Contains("403"))
                    throw ErrorMessages.Api.CreateInvalidApiKey();
                if (ex.Message.Contains("429"))
                    throw ErrorMessages.Api.CreateQuotaExceeded();
                if (ex.Message.Contains("timeout") || ex.InnerException is System.Net.Sockets.SocketException)
                    throw ErrorMessages.Api.CreateNetworkError();
                
                throw ErrorMessages.Enhancement.CreateEnhancementFailed();
            }
            catch (Exception)
            {
                throw ErrorMessages.Enhancement.CreateEnhancementFailed();
            }
        }

        private class GeminiResponse
        {
            public GeminiCandidate[] Candidates { get; set; }
        }

        private class GeminiStreamResponse
        {
            public GeminiCandidate[] Candidates { get; set; }
        }

        private class GeminiCandidate
        {
            public GeminiContent Content { get; set; }
        }

        private class GeminiContent
        {
            public GeminiPart[] Parts { get; set; }
        }

        private class GeminiPart
        {
            public string Text { get; set; }
        }
    }
}
