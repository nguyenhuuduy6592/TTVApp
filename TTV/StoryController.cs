
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace TTV
{
    public class StoryController {
        private const string BaseUrl = "https://www.nae.vn/";
        private const string TTVBaseUrl = BaseUrl + "ttv/ttv/public/";
        private const string IMEI = "351FFED4-6119-485B-8730-6C86B30FA8C2";
        private const string UserAgent = "TTV/1.16 (iPhone; iOS 11.0.3; Scale/2.00)";
        private const int UserId = 62790;
        private string Token { get; set; }
        private int StoryId { get; set; }
        public bool HasToken { get; set; }

        public StoryController()
        {
        }
        
        public StoryController(string token, int storyId)
        {
            Token = token;
            StoryId = storyId;
        }
        
        public StoryController(int storyId)
        {
            StoryId = storyId;
            if (string.IsNullOrEmpty(Token)){
                GetToken().Wait();
            }
            if (string.IsNullOrEmpty(Token))
                HasToken = false;
            else
                HasToken = true;
        }
        
        public async Task<string> GetChapterContent(int chapterId)
        {
            ChapterModel chapter = new ChapterModel();
            chapter.Id = chapterId;
            try 
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(BaseUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                client.DefaultRequestHeaders.Add("token",Token);
                client.DefaultRequestHeaders.Add("userid",UserId.ToString());
                
                string hash = CalculateHash_GetChapter(chapterId);
                var postModel = new GetListChapterContentRequestModel {
                    get_content_chapter = Utils.formatGetChapterContentQuery(
                        chapterId.ToString(), StoryId.ToString(), UserId.ToString(), hash)
                };

                // Serialize our concrete class into a JSON String
                var querydata = await Task.Run(() => JsonConvert.SerializeObject(postModel));
                var encodeQuery = new StringContent(querydata, Encoding.UTF8, "application/json");

                encodeQuery.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var httpResponse = await client.PostAsync(new Uri(TTVBaseUrl + "get_content_chapter"), encodeQuery);
                
                if (httpResponse.Content != null)
                {
                    var responseContent = await httpResponse.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<ChapterResponse>(responseContent);
                    if (data.Message == "succes")
                        chapter.Content = data.Content_Chapter[0].Content;
                }
                return chapter.Content;
            }
            catch 
            {
                return null;
            }
        }

        public void GetChapterList(){
            var client = new HttpClient();
            client.BaseAddress = new Uri(BaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            
            string hash = CalculateHash_GetChapterList();            
            var postModel = new GetListChapterRequestModel {
                get_list_chapter = Utils.formatGetChapterListQuery(StoryId.ToString(), "0", "all", hash)
            };

            // Serialize our concrete class into a JSON String
            var querydata = JsonConvert.SerializeObject(postModel);
            var encodeQuery = new StringContent(querydata, Encoding.UTF8, "application/json");

            encodeQuery.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var httpResponse = client.PostAsync(new Uri(TTVBaseUrl + "get_list_chapter"), encodeQuery).Result;
            
            if (httpResponse.Content != null)
            {
                var responseContent = httpResponse.Content.ReadAsStringAsync().Result;
                ChapterListResponse data = JsonConvert.DeserializeObject<ChapterListResponse>(responseContent);
                Console.Write(data);
                //Token = data.IMEI.remember_token;
            }
        }

        public async Task GetToken()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(BaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            
            var querydata = "get_token={\"imei\":\"" + IMEI + "\",\"token_adr\":\"null\",\"token_ios\":\"null\"}";
            var encodeQuery = new StringContent(System.Uri.EscapeUriString(querydata), Encoding.UTF8, "application/json");
            encodeQuery.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var httpResponse = await client.PostAsync(new Uri(TTVBaseUrl + "get_token"), encodeQuery);
            
            if (httpResponse.Content != null)
            {
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                TokenModel data = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenModel>(responseContent);
                if (data != null) {
                    Token = data.IMEI.remember_token;
                }
            }
        }

        public String CalculateHash_GetChapterList(){
            var input = Token + StoryId + "0all" + "174587236491eyoruwoiernzwueyquhszsadhajsdha8";
            return sha256Hash(input);
        }

        public String CalculateHash_GetChapter(int chapterId){
            var input = Token + chapterId + StoryId + UserId + "174587236491eyoruwoiernzwueyquhszsadhajsdha8";
            return sha256Hash(input);
        }

        public string sha256Hash(string data)
        {
            // SHA256 is disposable by inheritance.  
            using (var sha256 = SHA256.Create())
            {
                // Send a sample text to hash.  
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                // Get the hashed string.  
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}