using System;

namespace TTV
{
    public class Utils
    {
        public static string formatGetChapterListQuery(string storyId, string delta, string all, int user_id, string hash) {
            //return String.Format("{\"id_story\": \"{0}\", \"delta\": \"{1}\",\"all\": \"{2}\",\"hash\":\"{3}\"}", storyId, delta, all, hash);
            return "{\"id_story\": \"" + storyId + "\", \"delta\": \"" + delta + "\",\"all\": \"" + all + "\",\"user_id\": \"" + user_id + "\",\"hash\":\"" + hash + "\"}";
        }

        public static string formatGetChapterContentQuery(string id_chapter, string id_story, string user_id, string hash) {
            // return String.Format("{\"id_chapter\": \"{0}\",\"id_story\":\"{1}\",\"user_id\":\"{2}\",\"hash\":\"{3}\"}", id_chapter, id_story, user_id, hash);
            return "{\"id_chapter\":\"" + id_chapter + "\",\"id_story\":\"" + id_story + "\",\"user_id\":\"" + user_id + "\",\"hash\":\"" + hash + "\"}";
        }
    }
}