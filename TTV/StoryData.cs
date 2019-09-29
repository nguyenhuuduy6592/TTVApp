using System.Collections.Generic;

namespace TTV
{
    public class StoryRequest {
        public int Id_Story { get; set; }
    }
    public class StoryResponse {
        public StoryResponse()
        {
            //Stories_Author = new List<Story>();
        }
        public int Status { get; set; }
        //public List<Story> Stories_Author { get; set; }
        public Story Story { get; set; }
    }
    public class Story {
        public string Author { get; set; }
        public int Count_Chapter { get; set; }
        public int Finish { get; set; }
        public int Id { get; set; }
        public int Id_Thread { get; set; }
        public string Image { get; set; }
        public string Introduce { get; set; }
        public string Name { get; set; }
    }
    public class ChapterListRequest {
        public ChapterListRequestContent Get_List_Chapter { get; set; }
    }
    public class ChapterListRequestContent {
        public int Id_Story { get; set; }
    }
    public class ChapterListResponse {
        public ChapterListResponse()
        {
            Chapter = new List<ChapterListResponseContent>();
        }
        public List<ChapterListResponseContent> Chapter { get; set; }
    }
    public class ChapterListResponseContent {
        public string Content_Title_Of_Chapter { get; set; }
        public int Id { get; set; }
        public string Name_Id_Chapter { get; set; }
        public int Vol { get; set; }
    }
    public class ChapterRequest {
        public string Get_Content_Chapter { get; set; }
    }
    public class ChapterRequestContent {
        public int Id_Chapter { get; set; }
        public int Id_Story { get; set; }
    }
    public class ChapterResponse {
        public int Id_Chapter { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
        public List<ChapterResponseContent> Content_Chapter { get; set; }
    }

    public class ChapterResponseContent {
        public string Converter { get; set; }
        public string Content { get; set; }    
    }
}