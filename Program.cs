using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;

namespace dotnet_core
{
    class Program
    {
        static void Main(string[] args)
        {
            // int step = int.Parse(args[0]);
            // var fileName = args[1];
            int step = 1;
            string fileName = "Chua_te";
            switch (step) {
                case 1:
                    GetChapterListContent(fileName);
                    break;
                case 2:
                    ProcessFile(fileName);
                    break;
            }
        }

        public static void GetChapterListContent(string fileName)
        {
            var story = new StoryModel();
            using (StreamReader file = File.OpenText(@"C:\Truyen2\" + fileName + ".txt"))
            {
                var content = file.ReadToEnd();
                Regex regex = new Regex(@"\{(.|\s)*?\}\s");
                var index = 0;
                // read data
                Encoding enc = new UTF8Encoding(true, true);
                JsonSerializer serializer = new JsonSerializer();
                byte[] bytes;
                foreach (Match match in regex.Matches(content))
                {
                    var value = match.Value;
                    if (index == 1) {
                        StoryResponse data = JsonConvert.DeserializeObject<StoryResponse>(value);
                        story.Id = data.Story.Id;
                        story.Image = data.Story.Image;
                        story.Finish = data.Story.Finish;
                        story.ForumThreadId = data.Story.Id_Thread;
                        story.Author = new AuthorModel {
                            Name = data.Story.Author
                        };
                        bytes= enc.GetBytes(data.Story.Name);
                        story.Name = enc.GetString(bytes);
                        bytes= enc.GetBytes(data.Story.Introduce);
                        story.Introduce = enc.GetString(bytes);
                    }
                    else if (index == 3) {
                        ChapterListResponse data = JsonConvert.DeserializeObject<ChapterListResponse>(value);
                        foreach (var item in data.Chapter){
                            story.Chapters.Add(new ChapterModel {
                                ChapterName = item.Content_Title_Of_Chapter,
                                ChapterNumber = item.Name_Id_Chapter,
                                Id = item.Id,
                                StoryId = story.Id
                            });
                        }
                    }
                    index++;
                }
            }
            
            var storyController = new StoryController(story.Id);
            foreach (var chapter in story.Chapters) {
                chapter.Content = storyController.GetChapterContent(chapter.Id).Result;
            }
            // Output data
            if (story.Id > 0) {
                var outputFile = @"C:\Truyen2\" + fileName + ".html";
                var output = "<html>";
                // header
                output += @"<head>" + 
                    "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />" +
                    "<title></title>" + 
                    "<style>" + 
                    "    *{margin:0!important;line-height:1.3em}" + 
                    "    body{margin:8px!important;}" + 
                    "    a{font-weight:700;text-decoration:none}" + 
                    "    h2{color:red;font-weight:700;text-align:center;}" +
                    "    .center{text-align:center;}" +
                    "    .info{font-weight:700;}" + 
                    "    .red{color:red;}" + 
                    "    .purple{color:purple;}" + 
                    "    .introduce{font-weight:100}" +
                    "    .blue{color:blue}" + 
                    "    .green{color:green}" + 
                    "    </style>" + 
                    "</head>";
                output += "<body>";
                output += "<h2 class=\"blue center\">Truyện: " + story.Name + "</h2>";
                output += "<p class=\"info center\">Thông tin ebook</p>";
                output += "<p class=\"red center\">Người tạo: Bestfriend</p>";
                output += "<p class=\"purple center\">Nguồn: Tàng thư viện</p>";
                output += "<p class=\"blue center\">Tác giả: " + story.Author.Name + "</p>";
                output += "<p class=\"green center\">Trạng thái: " + (story.Finish == 0 ? "Đang ra" : "Hoàn thành") + "</p>";
                output += "<p class=\"info center\">Giới thiệu<p><p class=\"introduce center\">" + story.Introduce.Replace("\n", "<br />") + "</p>";
                output += "<p class=\"info center\">Danh sách chương</p>";
                foreach (var chapter in story.Chapters){
                    output += "<p><a href=\"#" + chapter.Id + "\">" + chapter.ChapterNumber + ": " + chapter.ChapterName + "</a></p>";
                }
                foreach (var chapter in story.Chapters){
                    output += "<br /><br /><h2 id=\"" + chapter.Id + "\">" + story.Name + " - " + chapter.ChapterNumber + ": " + chapter.ChapterName + "</h2><br /><br />";
                    if (chapter.Content != null) {
                        output += "<p>" + chapter.Content.Replace("\n", "<br />") + "</p>";
                    }
                }
                output += "</body>";
                output += "</html>";
                File.WriteAllLines(outputFile, output.Split('\n'));
            }
        }

        public static void ProcessFile(string fileName)
        {
            using (StreamReader file = File.OpenText(@"C:\Truyen2\" + fileName + ".txt"))
            {
                var content = file.ReadToEnd();
                Regex regex = new Regex(@"\{(.|\s)*?\}\s");
                var index = 0;
                // read data
                Encoding enc = new UTF8Encoding(true, true);
                JsonSerializer serializer = new JsonSerializer();
                byte[] bytes;
                var story = new StoryModel();
                var chapterId = 0;
                var storyId = 0;
                foreach (Match match in regex.Matches(content))
                {
                    var value = match.Value;
                    if (index == 1) {
                        StoryResponse data = JsonConvert.DeserializeObject<StoryResponse>(value);
                        story.Id = data.Story.Id;
                        story.Image = data.Story.Image;
                        story.Finish = data.Story.Finish;
                        story.ForumThreadId = data.Story.Id_Thread;
                        story.Author = new AuthorModel {
                            Name = data.Story.Author
                        };
                        bytes= enc.GetBytes(data.Story.Name);
                        story.Name = enc.GetString(bytes);
                        bytes= enc.GetBytes(data.Story.Introduce);
                        story.Introduce = enc.GetString(bytes);
                    }
                    else if (index == 3) {
                        ChapterListResponse data = JsonConvert.DeserializeObject<ChapterListResponse>(value);
                        foreach (var item in data.Chapter){
                            story.Chapters.Add(new ChapterModel {
                                ChapterName = item.Content_Title_Of_Chapter,
                                ChapterNumber = item.Name_Id_Chapter,
                                Id = item.Id,
                                StoryId = story.Id
                            });
                        }
                    }
                    else if (index >= 4 && index % 2 == 0) {
                        value = value.Replace("\\\"", "abc123");
                        ChapterRequest data = JsonConvert.DeserializeObject<ChapterRequest>(value);
                        Regex regex2 = new Regex(@"id_chapterabc123:abc123(\d+)abc123,abc123id_storyabc123:abc123(\d+)");
                        var match2 = regex2.Match(data.Get_Content_Chapter);
                        if (match2.Groups.Count == 3) { 
                            chapterId = int.Parse(match2.Groups[1].Value);
                            storyId = int.Parse(match2.Groups[2].Value);
                        }
                    }
                    else if (index >= 5 && index % 2 == 1 && chapterId > 0 && storyId > 0) {
                        ChapterResponse data = JsonConvert.DeserializeObject<ChapterResponse>(value);
                        var chapter = story.Chapters.FirstOrDefault(x => x.Id == chapterId && x.StoryId == storyId);
                        if (chapter != null) {
                            chapter.Content = data.Content_Chapter[0].Content;
                        }
                        chapterId = 0;
                        storyId = 0;
                    }
                    else {
                        index++;
                        continue;
                    }
                    index++;
                }
                // Output data
                if (story.Id > 0) {
                    var outputFile = @"C:\Truyen2\" + fileName + ".html";
                    var output = "<html>";
                    // header
                    output += @"<head>" + 
                        "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />" +
                        "<title></title>" + 
                        "<style>" + 
                        "    *{margin:0!important;line-height:1.3em}" + 
                        "    body{margin:8px!important;}" + 
                        "    a{font-weight:700;text-decoration:none}" + 
                        "    h2{color:red;font-weight:700;text-align:center;}" +
                        "    .center{text-align:center;}" +
                        "    .info{font-weight:700;}" + 
                        "    .red{color:red;}" + 
                        "    .purple{color:purple;}" + 
                        "    .introduce{font-weight:100}" +
                        "    .blue{color:blue}" + 
                        "    .green{color:green}" + 
                        "    </style>" + 
                        "</head>";
                    output += "<body>";
                    output += "<h2 class=\"blue center\">Truyện: " + story.Name + "</h2>";
                    output += "<p class=\"info center\">Thông tin ebook</p>";
                    output += "<p class=\"red center\">Người tạo: Bestfriend</p>";
                    output += "<p class=\"purple center\">Nguồn: Tàng thư viện</p>";
                    output += "<p class=\"blue center\">Tác giả: " + story.Author.Name + "</p>";
                    output += "<p class=\"green center\">Trạng thái: " + (story.Finish == 0 ? "Đang ra" : "Hoàn thành") + "</p>";
                    output += "<p class=\"info center\">Giới thiệu<p><p class=\"introduce center\">" + story.Introduce.Replace("\n", "<br />") + "</p>";
                    output += "<p class=\"info center\">Danh sách chương</p>";
                    foreach (var chapter in story.Chapters){
                        output += "<p><a href=\"#" + chapter.Id + "\">" + chapter.ChapterNumber + ": " + chapter.ChapterName + "</a></p>";
                    }
                    foreach (var chapter in story.Chapters){
                        output += "<br /><br /><h2 id=\"" + chapter.Id + "\">" + story.Name + " - " + chapter.ChapterNumber + ": " + chapter.ChapterName + "</h2><br /><br />";
                        if (chapter.Content != null) {
                            output += "<p>" + chapter.Content.Replace("\n", "<br />") + "</p>";
                        }
                    }
                    output += "</body>";
                    output += "</html>";
                    File.WriteAllLines(outputFile, output.Split('\n'));
                }
            }
        }
    }
}
