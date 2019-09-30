using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Linq;
using System.Diagnostics;

namespace TTV
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int.TryParse(args[0], out var userId);
            var token = args[1];
            int.TryParse(args[2], out var storyId);
            var fileName = args[3];

            var storyController = new StoryController(userId, token, storyId);
            StoryModel story;
            if (!string.IsNullOrEmpty(fileName) && fileName != "no")
            {
                // Read previous work
                Console.WriteLine("Read previous work!");
                story = ReadPreviousWork(fileName);
                // Parse story info if needed            
                if (story == null)
                {
                    Console.WriteLine("No previous work. Read story info!");
                    story = GetStoryInfo(storyController);
                    if (story == null)
                    {
                        Console.WriteLine($"Story with id {storyId} not found!");
                        return;
                    }
                }
            }
            else
            {
                story = GetStoryInfo(storyController);
                if (story == null)
                {
                    Console.WriteLine($"Story with id {storyId} not found!");
                    return;
                }
                fileName = story.Name;
            }

            SaveCurrentWork(story, fileName);
            // Get chapters' content if needed
            Console.WriteLine("Get chapters' content!");
            var complete = GetChapterListContent(story, fileName, storyController);
            PrintElapsedTime(stopwatch);
            // Save output if completed
            if (complete)
            {
                Console.WriteLine("Saving output!");
                SaveHtml(story, fileName);
                Console.WriteLine("Completed!");
            }
            PrintElapsedTime(stopwatch);
            // Save current work for future
            Console.WriteLine("Saved Current Work!");
            SaveCurrentWork(story, fileName);
            PrintElapsedTime(stopwatch, false);
        }

        private static void PrintElapsedTime(Stopwatch stopwatch, bool reset = true)
        {
            stopwatch.Stop();
            Console.WriteLine($"Download time {stopwatch.ElapsedMilliseconds / 1000}s");
            if (reset)
            {
                stopwatch.Reset();
                stopwatch.Start();
            }
        }

        public static StoryModel ReadPreviousWork(string fileName){
            StoryModel story = null;
            var filePath = fileName + ".bin";
            if (File.Exists(filePath)){
                using (StreamReader file = File.OpenText(filePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    story = (StoryModel)serializer.Deserialize(file, typeof(StoryModel));
                }
            }
            return story;
        }

        public static bool SaveCurrentWork(StoryModel story, string fileName)
        {
            try {
                var filePath = fileName + ".bin";
                using (StreamWriter file = File.CreateText(filePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, story);
                }
                return true;
            }
            catch {
                return false;
            }
        }

        public static StoryModel GetStoryInfo(StoryController storyController)
        {
            StoryModel model = null;
            if (storyController.HasToken)
            {
                var data = storyController.GetStoryContent();
                if (data != null)
                {
                    Encoding enc = new UTF8Encoding(true, true);
                    byte[] bytes;
                    model = new StoryModel
                    {
                        Id = data.Story.Id,
                        Image = data.Story.Image,
                        Finish = data.Story.Finish,
                        ForumThreadId = data.Story.Id_Thread,
                        Author = new AuthorModel
                        {
                            Name = data.Story.Author
                        }
                    };
                    bytes = enc.GetBytes(data.Story.Name);
                    model.Name = enc.GetString(bytes);
                    bytes = enc.GetBytes(data.Story.Introduce);
                    model.Introduce = enc.GetString(bytes);

                    var chapterList = storyController.GetChapterList();
                    if (chapterList != null)
                    {
                        foreach (var item in chapterList.Chapter)
                        {
                            model.Chapters.Add(new ChapterModel
                            {
                                ChapterName = item.Content_Title_Of_Chapter,
                                ChapterNumber = item.Name_Id_Chapter,
                                Id = item.Id,
                                StoryId = model.Id
                            });
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("No Token!");
            }
            return model;
        }

        public static StoryModel ParseStoryInfo(string fileName)
        {
            using (StreamReader file = File.OpenText(fileName + ".txt"))
            {
                var content = file.ReadToEnd();
                Regex regex = new Regex(@"\{(.|\s)*?\}\s");
                var index = 0;
                // read data
                Encoding enc = new UTF8Encoding(true, true);
                JsonSerializer serializer = new JsonSerializer();
                byte[] bytes;
                var story = new StoryModel();
                var matches = regex.Matches(content);
                foreach (Match match in matches)
                {
                    var value = match.Value;
                    if (index == 1)
                    {
                        StoryResponse data = JsonConvert.DeserializeObject<StoryResponse>(value);
                        story.Id = data.Story.Id;
                        story.Image = data.Story.Image;
                        story.Finish = data.Story.Finish;
                        story.ForumThreadId = data.Story.Id_Thread;
                        story.Author = new AuthorModel
                        {
                            Name = data.Story.Author
                        };
                        bytes = enc.GetBytes(data.Story.Name);
                        story.Name = enc.GetString(bytes);
                        bytes = enc.GetBytes(data.Story.Introduce);
                        story.Introduce = enc.GetString(bytes);
                    }
                    else if (index == 3)
                    {
                        ChapterListResponse data = JsonConvert.DeserializeObject<ChapterListResponse>(value);
                        foreach (var item in data.Chapter)
                        {
                            story.Chapters.Add(new ChapterModel
                            {
                                ChapterName = item.Content_Title_Of_Chapter,
                                ChapterNumber = item.Name_Id_Chapter,
                                Id = item.Id,
                                StoryId = story.Id
                            });
                        }
                    }
                    index++;
                }
                return story;
            }
        }

        public static bool GetChapterListContent(StoryModel story, string fileName, StoryController storyController)
        {
            var chapterCount = 0;
            if (storyController.HasToken){
                foreach (var (chapter, index) in story.Chapters.Select((chapter, index) => (chapter, index))) {
                    if (string.IsNullOrEmpty(chapter.Content))
                    {
                        Console.WriteLine($"Chapter {index + 1}/{story.Chapters.Count}");
                        var content = storyController.GetChapterContent(chapter.Id);
                        if (!string.IsNullOrEmpty(content)){
                            chapter.Content = content;
                            chapterCount++;
                            SaveCurrentWork(story, fileName);
                            continue;
                        }
                    }
                    chapterCount++;
                }
                return chapterCount == story.Chapters.Count;
            }
            else
            {
                Console.WriteLine("No Token!");
                return false;
            }
        }

        public static void SaveHtml(StoryModel story, string fileName)
        {
            // Output data
            var outputFile = fileName + ".html";
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
            var step = 100;
            for (var index = 1; index <= story.Chapters.Count; index = index + step) {
                var chapter = story.Chapters[index - 1];
                if (chapter != null) {
                    var nextIndex = index + step;
                    if (nextIndex > story.Chapters.Count) nextIndex = story.Chapters.Count;
                    var nextChapter = story.Chapters[nextIndex - 1];
                    if (nextChapter != null) {
                        output += "<p><a href=\"#" + index + "\">" + chapter.ChapterNumber + " - " + nextChapter.ChapterNumber + "</a></p>";
                    }
                }
            }
            for (var index = 1; index <= story.Chapters.Count; index++) {
                var chapter = story.Chapters[index - 1];
                if (chapter != null) {
                    output += "<p id=\"" + index + "\"><a href=\"#" + chapter.Id + "\">" + chapter.ChapterNumber + ": " + chapter.ChapterName + "</a></p>";
                }
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

        public static void ProcessFile(string fileName)
        {
            using (StreamReader file = File.OpenText(fileName + ".txt"))
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
                    var outputFile = fileName + ".html";
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

        static void OnProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("I'm out of here");
            Console.ReadLine();
        }
    }
}
