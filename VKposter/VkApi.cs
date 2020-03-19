using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VKposter
{
    class VkApi
    {


        //VK API METHOD WRAPPER

        public static Tuple<string,string , string> getById(string token, string inputId)
        {
            var c = new WebClient();
            //groups.getById
            var u = "https://api.vk.com/method/groups.getById?v=5.52&group_id=" + inputId
                + "&access_token=" + token;

            var r = c.DownloadString(u);
            //Console.WriteLine(r);
            var j = JsonConvert.DeserializeObject(r) as JObject;
            if (j["error"] != null)
            {
                throw new VkApiException(j["error"]["error_msg"].ToString());
            }

            string screenName = j["response"][0]["screen_name"].ToString();
            string name = j["response"][0]["name"].ToString();
            string wallid = j["response"][0]["id"].ToString();
            return new Tuple<string, string , string >(screenName , name , wallid);
        }

        //VK API METHOD WRAPPER
        public static string photoGetWallUploadServer(string token , string wall)
        {
            
            var client = new WebClient();
            var requestUrl = "https://api.vk.com/method/photos.getWallUploadServer?v=5.52&user_id=" + wall
                    + "&access_token=" + token;
            var response = client.DownloadString(requestUrl);
            var responseJson = JsonConvert.DeserializeObject(response) as JObject;
            //check if error throw exception
            if (responseJson["error"] != null)
            {
                throw new VkApiException(responseJson["error"]["error_msg"].ToString());
            }

            var uploadUrl = responseJson["response"]["upload_url"].ToString();
            return uploadUrl;
        }


        //VK API METHOD WRAPPER
        public static string postPhotoToServer(string uploadUrl , string imagePath)
        {
            WebClient client = new WebClient();
            var response = Encoding.UTF8.GetString(client.UploadFile(uploadUrl, "POST", imagePath));
            var responseJson = JsonConvert.DeserializeObject(response) as JObject;

            //check exception 
            if (responseJson["error"] != null)
            {
                throw new VkApiException(responseJson["error"]["error_msg"].ToString());
            }

            return response;

        }

        //VK API METHOD WRAPPER
        public static string saveWallPhoto(string token,string postResponse)
        {
            WebClient client = new WebClient();
            var postResponseJson = JsonConvert.DeserializeObject(postResponse) as JObject;
            var saveUrl = "https://api.vk.com/method/photos.saveWallPhoto?v=5.52&access_token=" + token
                     + "&server=" + postResponseJson["server"]
                     + "&photo=" + postResponseJson["photo"]
                     + "&hash=" + postResponseJson["hash"];
            var response = client.DownloadString(saveUrl);
            var saveResponseJson = JsonConvert.DeserializeObject(response) as JObject;
            //check exception 

            if (saveResponseJson["error"] != null)
            {
                throw new VkApiException(saveResponseJson["error"]["error_msg"].ToString());
            }

            return response;
        }

        //VK API METHOD WRAPPER
        public static string wallPost(string token ,string savePhotoResponse  , string wall ,  string commentText)
        {
            WebClient client = new WebClient();
            var saveResponseJson = JsonConvert.DeserializeObject(savePhotoResponse) as JObject;
            // 
            var wallPostUrl = "https://api.vk.com/method/wall.post?v=5.52&access_token=" + token
                     + "&owner_id=" + wall
                     + "&message=" + commentText
                     + "&attachments=photo" + saveResponseJson["response"][0]["owner_id"] + "_" + saveResponseJson["response"][0]["id"];
            string response = client.DownloadString(wallPostUrl);

            var wallPostResponseJson = JsonConvert.DeserializeObject(response) as JObject;
            //check Exception
            if (wallPostResponseJson["error"] != null)
            {
                throw new VkApiException(wallPostResponseJson["error"]["error_msg"].ToString());
            }

            return response;
        }


        //custom function


        public static string postPhotoOnWall(Wall wall, string imagePath)
        {
            string uploadUrl = photoGetWallUploadServer(wall.token, wall.id);
            string postPhotoResponse = postPhotoToServer(uploadUrl, imagePath);
            string savePhotoResponse = saveWallPhoto(wall.token, postPhotoResponse);
            string response = wallPost(wall.token, savePhotoResponse, wall.id, wall.commentText);

            //if okey return response
            return response;

            //else return string with error
        }



        //public static string postPhotoOnWall(string token , string wall , string imagePath, string commentText )
        //{
        //    string uploadUrl = photoGetWallUploadServer( token ,wall);
        //    string postPhotoResponse = postPhotoToServer(uploadUrl, imagePath);
        //    string savePhotoResponse = saveWallPhoto(token, postPhotoResponse);
        //    string response = wallPost(token, savePhotoResponse, wall, commentText);

        //    //if okey return response
        //    return response;

            //else return string with error
        //}
    }
}
