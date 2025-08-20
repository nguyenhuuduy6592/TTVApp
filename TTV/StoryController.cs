
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Linq;
using TTV.Config;
using TTV.Services;

namespace TTV
{
    public class StoryController
    {
        private const string BaseUrl = "https://www.nae.vn/";
        private const string TTVBaseUrl = BaseUrl + "ttv/ttv/public/";
        private const string IMEI = "CCA4E8EB-E71D-41C5-BACF-4D60488903C0";
        private const string UserAgent = "TTV/2.3 (iPhone; iOS 13.1; Scale/2.00)";
        private int UserId { get; set; }
        private string Token { get; set; }
        private int StoryId { get; set; }
        public bool HasToken { get; set; }
        private readonly IGeminiService _geminiService;

        public StoryController(int userId, string token, int storyId, GeminiConfig geminiConfig = null)
        {
            UserId = userId;
            Token = token;
            StoryId = storyId;
            HasToken = true;
            _geminiService = new GeminiService(geminiConfig);
        }

        public StoryController(int storyId)
        {
            StoryId = storyId;
            if (string.IsNullOrEmpty(Token))
            {
                GetToken().Wait();
            }
            if (string.IsNullOrEmpty(Token))
                HasToken = false;
            else
                HasToken = true;
        }

        public async Task GetToken()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);

            var querydata = "get_token={\"imei\":\"" + IMEI + "\",\"token_adr\":\"null\",\"token_ios\":\"null\"}";
            var encodeQuery = new StringContent(Uri.EscapeUriString(querydata), Encoding.UTF8, "application/json");
            encodeQuery.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var httpResponse = await client.PostAsync(new Uri(TTVBaseUrl + "get_token"), encodeQuery);

            if (httpResponse.Content != null)
            {
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                TokenModel data = JsonConvert.DeserializeObject<TokenModel>(responseContent);
                if (data != null)
                {
                    Token = data.IMEI.remember_token;
                }
            }
        }

        public StoryResponse GetStoryContent()
        {
            StoryResponse data = null;
            try
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(BaseUrl)
                };
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var postModel = new StoryRequestModel
                {
                    id_story = StoryId
                };

                // Serialize our concrete class into a JSON String
                var querydata = JsonConvert.SerializeObject(postModel);
                var encodeQuery = new StringContent(querydata, Encoding.UTF8, "application/json");

                encodeQuery.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                encodeQuery.Headers.TryAddWithoutValidation("appname", "ttvios");
                encodeQuery.Headers.TryAddWithoutValidation("imei", IMEI);
                encodeQuery.Headers.TryAddWithoutValidation("token", Token);
                encodeQuery.Headers.TryAddWithoutValidation("userid", UserId.ToString());
                encodeQuery.Headers.TryAddWithoutValidation("versionios", "230");

                var httpResponse = client.PostAsync(new Uri(TTVBaseUrl + "get_list_story_author"), encodeQuery).Result;

                if (httpResponse.Content != null)
                {
                    var responseContent = httpResponse.Content.ReadAsStringAsync().Result;
                    data = JsonConvert.DeserializeObject<StoryResponse>(responseContent);
                }
            }
            catch (Exception ex)
            {
            }
            return data;
        }

        public ChapterListResponse GetChapterList()
        {
                                ChapterListResponse chapterList = null;
                    int retryCount = 0;
                    const int maxRetries = 3;
            try
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(BaseUrl)
                };
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string hash = CalculateHash_GetChapterList();
                var postModel = new GetListChapterRequestModel
                {
                    get_list_chapter = Utils.formatGetChapterListQuery(StoryId.ToString(), "0", "all", UserId, hash)
                };

                // Serialize our concrete class into a JSON String
                var querydata = JsonConvert.SerializeObject(postModel);
                var encodeQuery = new StringContent(querydata, Encoding.UTF8, "application/json");

                encodeQuery.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                encodeQuery.Headers.TryAddWithoutValidation("appname", "ttvios");
                encodeQuery.Headers.TryAddWithoutValidation("imei", IMEI);
                encodeQuery.Headers.TryAddWithoutValidation("token", Token);
                encodeQuery.Headers.TryAddWithoutValidation("userid", UserId.ToString());
                encodeQuery.Headers.TryAddWithoutValidation("versionios", "230");

                var httpResponse = client.PostAsync(new Uri(TTVBaseUrl + "get_list_chapter"), encodeQuery).Result;

                if (httpResponse.Content != null)
                {
                    var responseContent = httpResponse.Content.ReadAsStringAsync().Result;
                    chapterList = JsonConvert.DeserializeObject<ChapterListResponse>(responseContent);
                }
            }
            catch (Exception ex)
            {
            }
            return chapterList;
        }

        public async Task<ChapterModel> GetChapterContent(int chapterId)
        {
            var chapter = new ChapterModel
            {
                Id = chapterId
            };
            try
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(BaseUrl)
                };
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string hash = CalculateHash_GetChapter(chapterId);
                var postModel = new GetListChapterContentRequestModel
                {
                    get_content_chapter = Utils.formatGetChapterContentQuery(
                        chapterId.ToString(), StoryId.ToString(), UserId.ToString(), hash)
                };

                // Serialize our concrete class into a JSON String
                var querydata = JsonConvert.SerializeObject(postModel);
                var encodeQuery = new StringContent(querydata, Encoding.UTF8, "application/json");

                encodeQuery.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                encodeQuery.Headers.TryAddWithoutValidation("appname", "ttvios");
                encodeQuery.Headers.TryAddWithoutValidation("imei", IMEI);
                encodeQuery.Headers.TryAddWithoutValidation("token", Token);
                encodeQuery.Headers.TryAddWithoutValidation("userid", UserId.ToString());
                encodeQuery.Headers.TryAddWithoutValidation("versionios", "230");

                var httpResponse = await client.PostAsync(new Uri(TTVBaseUrl + "get_content_chapter"), encodeQuery);

                if (httpResponse.Content != null)
                {
                    var responseContent = await httpResponse.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<ChapterResponse>(responseContent);
                    if (data.Message == "succes")
                    {
                        chapter.Content = data.Content_Chapter[0].Content;
                        
                        // Only enhance if Gemini service is configured
                        if (_geminiService?.IsConfigured == true)
                        {
                            int retryCount = 0;
                            const int maxRetries = 3;
                            bool enhancementSuccessful = false;

                            while (retryCount < maxRetries && !enhancementSuccessful)
                            {
                                try
                                {
                                    chapter.EnhancedContent = await _geminiService.EnhanceContentAsync(chapter.Content);
                                    chapter.IsEnhancedWithAI = true;
                                    enhancementSuccessful = true;
                                }
                                catch (Exception)
                                {
                                    retryCount++;
                                    if (retryCount == maxRetries)
                                    {
                                        chapter.EnhancedContent = chapter.Content;
                                        chapter.IsEnhancedWithAI = false;
                                    }
                                    await Task.Delay(1000 * retryCount); // Exponential backoff
                                }
                            }
                        }
                    }
                }
                return chapter;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string CalculateHash_GetChapterList()
        {
            var input = Token + StoryId + "0all" + UserId + "174587236491eyoruwoiernzwueyquhszsadhajsdha8";
            return Sha256Hash(input);
        }

        private string CalculateHash_GetChapter(int chapterId)
        {
            var input = Token + chapterId + StoryId + UserId + "174587236491eyoruwoiernzwueyquhszsadhajsdha8";
            return Sha256Hash(input);
        }

        public string Sha256Hash(string data)
        {
            // SHA256 is disposable by inheritance.  
            using (var sha256 = SHA256.Create())
            {
                // Send a sample text to hash.  
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                var output = hashedBytes.Select(x => (sbyte)x).ToArray();
                string str = "";
                foreach (byte b in output)
                {
                    var currentValue = b.ToString("x2");
                    str += currentValue;
                }
                return str;
                // Get the hashed string.
                //return BitConverter.ToString(output).Replace("-", "").ToLower();
            }
        }
        public sbyte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            sbyte[] bytes = new sbyte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToSByte(hex.Substring(i, 2), 16);
            return bytes;
        }

    }
}