using System.Collections.Generic;

namespace TTV
{
    public class TokenModel {
        public IMEI IMEI { get; set; }
    }
    public class IMEI { 
        public string remember_token { get; set; }
    }
    public class StoryRequestModel
    {
        public int id_story { get; set; }
    }
    public class GetListChapterRequestModel {
        public string get_list_chapter { get; set; }
    }
    public class GetListChapterRequestContentModel {
        public int id_story { get; set; }
        public int delta { 
            get {
                return 0;
            } 
        }
        public string all { 
            get {
                return "all";
            } 
        }
        public int user_id { get; set; }
        public string hash { get; set; }
    }
    public class GetListChapterContentRequestModel {
        public string get_content_chapter { get; set; }
    }
    public class StoryModel {
        public StoryModel()
        {
            Chapters = new List<ChapterModel>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Introduce { get; set; }
        public int Finish { get; set; }
        public int ForumThreadId { get; set; }
        public AuthorModel Author { get; set; }
        public List<ChapterModel> Chapters { get; set; }
    }
    public class ChapterModel {
        public int Id { get; set; }
        public int StoryId { get; set; }
        public string ChapterNumber { get; set; }
        public string ChapterName { get; set; }
        public int VolumeNumber { get; set; }
        public string Content { get; set; }
        public string EnhancedContent { get; set; }
        public bool IsEnhancedWithAI { get; set; }

        public string GetDisplayContent() => IsEnhancedWithAI ? EnhancedContent : Content;
    }
    public class AuthorModel {
        public string Name { get; set; }

    }
}