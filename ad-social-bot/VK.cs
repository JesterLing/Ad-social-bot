using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ad_social_bot
{
    class VK
    {
        private string _token;
        public string name;
        public VK(string VKauth)
        {
            _token = VKauth;
            HttpWebResponse response = Program.GETRequest("https://api.vk.com/method/users.get?access_token=" + _token, null);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(VKUserInfoRoot));
                VKUserInfoRoot req = (VKUserInfoRoot)js.ReadObject(new MemoryStream(System.Text.ASCIIEncoding.UTF8.GetBytes(new StreamReader(response.GetResponseStream()).ReadToEnd())));
                if (req.error != null)
                {
                    switch (req.error.error_code)
                    {
                        case 5:
                            throw new InvalidAccessToken();
                            break;
                        case 14:
                            throw new VKCaptchaNeeded(response.ResponseUri.AbsolutePath);
                            break;
                        default:
                            throw new Exception("Не удалось получить имя пользователя. ErrorCode: " + req.error.error_code + " ErrorMsg: " + req.error.error_msg);
                            break;
                    }
                }
                response.GetResponseStream().Close();
                response.Close();
                response = Program.GETRequest(@"https://api.vk.com/method/stats.trackVisitor?access_token=" + _token, null);
                response.GetResponseStream().Close();
                response.Close();
                name = req.response[0].first_name + " " + req.response[0].last_name;
            }
            else
            {
                throw new Exception("Сервер не отвечает (" + response.StatusCode.ToString() + ")");
            }
        }

        public string AddLike(string URL)
        {
            string type = "", owner_id = "", item_id = "";
            Regex regex = new Regex("(photo|wall)(-*[0-9]+)_([0-9]+)");
            foreach (Match match in regex.Matches(URL))
            {
                if (match.Groups[1].Value == "wall") type = "post";
                else type = "photo";
                owner_id = match.Groups[2].Value;
                item_id = match.Groups[3].Value;
            }
            if (type!="" && owner_id!="" && item_id!="")
            {
                    HttpWebResponse response = Program.GETRequest("https://api.vk.com/method/likes.add?type=" + type + "&owner_id=" + owner_id + "&item_id=" + item_id + "&access_token=" + _token, null);
                if (response.StatusCode == HttpStatusCode.OK)
                    {
                    DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(VKLikeResponseRoot));
                    VKLikeResponseRoot req = (VKLikeResponseRoot)js.ReadObject(new MemoryStream(System.Text.ASCIIEncoding.UTF8.GetBytes(new StreamReader(response.GetResponseStream()).ReadToEnd())));
                    if (req.error != null)
                    {
                        switch (req.error.error_code)
                        {
                            case 5:
                                throw new InvalidAccessToken();
                                break;
                            case 14:
                                throw new VKCaptchaNeeded(response.ResponseUri.AbsolutePath);
                                break;
                            default:
                                throw new Exception("Не удалось поставить лайк. ErrorCode: " + req.error.error_code + " ErrorMsg: " + req.error.error_msg);
                                break;
                        }
                    }
                    response.GetResponseStream().Close();
                    response.Close();
                    return "OK";
                    } else {
                        throw new Exception("Сервер не отвечает (" + response.StatusCode.ToString() + ")");
                    }
            } else {
              throw new Exception("Не удается разобрать задание");
            }
        }

        public string JoinGroup(string URL)
        {
            string group_id = "";
            Regex regex = new Regex("(public|club)([0-9]+)");
            foreach (Match match in regex.Matches(URL))
            {
                group_id = match.Groups[2].Value;
            }
            if(group_id!="")
            {
                HttpWebResponse response = Program.GETRequest(@"https://api.vk.com/method/groups.join?group_id=" + group_id + "&access_token=" + _token, null);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(VKGroupResponse));
                    VKGroupResponse req = (VKGroupResponse)js.ReadObject(new MemoryStream(System.Text.ASCIIEncoding.UTF8.GetBytes(new StreamReader(response.GetResponseStream()).ReadToEnd())));
                    if (req.error != null)
                    {
                        switch (req.error.error_code)
                        {
                            case 5:
                                throw new InvalidAccessToken();
                                break;
                            case 14:
                                throw new VKCaptchaNeeded(response.ResponseUri.AbsolutePath);
                                break;
                            default:
                                throw new Exception("Не удалось вступить в групу. ErrorCode: " + req.error.error_code + " ErrorMsg: " + req.error.error_msg);
                                break;
                        }
                    }
                    response.GetResponseStream().Close();
                    response.Close();
                    return "OK";
                }
                else
                {
                    throw new Exception("Сервер не отвечает (" + response.StatusCode.ToString() + ")");
                }
            } else {
                throw new Exception("Не удается разобрать задание");
            }
        }

        public string AddFriend(string URL)
        {
            string friend_id = "";
            Regex regex = new Regex("(id)([0-9]+)");
            foreach (Match match in regex.Matches(URL))
            {
                friend_id = match.Groups[2].Value;
            }
            if (friend_id != "")
            {
                HttpWebResponse response = Program.GETRequest("https://api.vk.com/method/friends.add?user_id=" + friend_id + "&access_token=" + _token, null);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(VKFriendResponse));
                    VKFriendResponse req = (VKFriendResponse)js.ReadObject(new MemoryStream(System.Text.ASCIIEncoding.UTF8.GetBytes(new StreamReader(response.GetResponseStream()).ReadToEnd())));
                    if (req.error != null)
                    {
                        switch (req.error.error_code)
                        {
                            case 5:
                                throw new InvalidAccessToken();
                                break;
                            case 14:
                                throw new VKCaptchaNeeded(response.ResponseUri.AbsolutePath);
                                break;
                            default:
                                throw new Exception("Не удалось добавить пользователя. ErrorCode: " + req.error.error_code + " ErrorMsg: " + req.error.error_msg);
                                break;
                        }
                    }
                    response.GetResponseStream().Close();
                    response.Close();
                    return "OK";
                }
                else
                {
                    throw new Exception("Сервер не отвечает (" + response.StatusCode.ToString() + ")");
                }
            }
            else
            {
                throw new Exception("Не удается разобрать задание");
            }
        }

        public string AddRepost(string URL)
        {
            string obj = "";
            Regex regex = new Regex("(wall.+)");
            foreach (Match match in regex.Matches(URL))
            {
                obj = match.Groups[1].Value;
            }
            if (obj != "")
            {
                HttpWebResponse response = Program.GETRequest(@"https://api.vk.com/method/wall.repost?object=" + obj + "&access_token=" +_token, null);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(VKRepostResponseRoot));
                    VKRepostResponseRoot req = (VKRepostResponseRoot)js.ReadObject(new MemoryStream(System.Text.ASCIIEncoding.UTF8.GetBytes(new StreamReader(response.GetResponseStream()).ReadToEnd())));
                    if (req.error != null)
                    {
                        switch (req.error.error_code)
                        {
                            case 5:
                                throw new InvalidAccessToken();
                                break;
                            case 14:
                                throw new VKCaptchaNeeded(response.ResponseUri.AbsolutePath);
                                break;
                            default:
                                throw new Exception("Не удалось репотснуть запись. ErrorCode: " + req.error.error_code + " ErrorMsg: " + req.error.error_msg);
                                break;
                        }
                    }
                    response.GetResponseStream().Close();
                    response.Close();
                    return "OK";
                }
                else
                {
                    throw new Exception("Сервер не отвечает (" + response.StatusCode.ToString() + ")");
                }
            }
            else
            {
                throw new Exception("Не удается разобрать задание");
            }
        }


        [DataContract]
        public class Error
        {
            [DataMember]
            public int error_code { get; set; }
            [DataMember]
            public string error_msg { get; set; }
        }

        [DataContract]
        public class VKLikeResponse
        {
            [DataMember]
            public int likes { get; set; }
        }

        [DataContract]
        public class VKLikeResponseRoot
        {
            [DataMember]
            public VKLikeResponse response { get; set; }
            [DataMember]
            public Error error { get; set; }
        }
        [DataContract]
        public class VKGroupResponse
        {
            [DataMember]
            public int response { get; set; }
            [DataMember]
            public Error error { get; set; }
        }
        [DataContract]
        public class VKFriendResponse
        {
            [DataMember]
            public int response { get; set; }
            [DataMember]
            public Error error { get; set; }
        }
        [DataContract]
        public class VKRepostResponse
        {
            [DataMember]
            public int success { get; set; }
            [DataMember]
            public int post_id { get; set; }
            [DataMember]
            public int reposts_count { get; set; }
            [DataMember]
            public int likes_count { get; set; }
        }
        [DataContract]
        public class VKRepostResponseRoot
        {
            [DataMember]
            public VKRepostResponse response { get; set; }
            [DataMember]
            public Error error { get; set; }
        }

        [DataContract]
        public class VKUserInfo
        {
            [DataMember]
            public int uid { get; set; }
            [DataMember]
            public string first_name { get; set; }
            [DataMember]
            public string last_name { get; set; }
        }
        [DataContract]
        public class VKUserInfoRoot
        {
            [DataMember]
            public List<VKUserInfo> response { get; set; }
            [DataMember]
            public Error error { get; set; }
        }
    }
}
