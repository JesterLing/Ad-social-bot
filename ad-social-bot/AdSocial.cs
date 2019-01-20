using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace ad_social_bot
{
    class AdSocial
    {
        private CookieCollection _auth;
        private uint _balance = 0;
        public uint Balance
        {
            get { return _balance; }
        }
        public AdSocial(CookieCollection ADauth)
        {
            _auth = ADauth;
                HttpWebResponse response = Program.GETRequest("https://ad-social.org/profile/balance", _auth);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    if (response.ResponseUri.AbsoluteUri == "https://ad-social.org/#login-form") throw new InvalidAdSocialAuthorization();
                    Regex regex = new Regex("На Вашем счете: <strong>([0-9].+) (балла|балл|баллов)</strong>");
                    foreach (Match match in regex.Matches(new StreamReader(response.GetResponseStream()).ReadToEnd()))
                    {
                        _balance = Convert.ToUInt32(match.Groups[1].Value.Trim().Replace(" ", string.Empty));
                    }
                    response.GetResponseStream().Close();
                    response.Close();
                }
                else
                {
                    throw new Exception("Сервер не отвечает (" + response.StatusCode.ToString() + ")");
                }
           }
        

        public List<Task> getTasks(string type)
        {
            int idDivider = 0;
            List<Task> tasks = new List<Task>();
            HttpWebResponse response = Program.GETRequest("https://ad-social.org/vk/earn?type=" + type, _auth);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string doc = new StreamReader(response.GetResponseStream()).ReadToEnd();
                if (response.ResponseUri.AbsoluteUri == "https://ad-social.org/#login-form") throw new InvalidAdSocialAuthorization();
                response.GetResponseStream().Close();
                response.Close();

                if(doc.Contains("<script src=\"https://www.google.com/recaptcha/api.js\" async defer></script>")) {
                    throw new SociaAdRobotCheck("https://ad-social.org/vk/earn?type=" + type);
                }

                Regex regex = new Regex(@"var id_2 = id / ([0-9]+);");
                foreach (Match match in regex.Matches(doc))
                {
                    idDivider = Convert.ToInt32(match.Groups[1].Value);
                }

                regex = new Regex(@"Не найдено ни одного задания.");
                foreach (Match match in regex.Matches(doc))
                {
                    throw new Exception("Нет доступных заданий на данный момент");
                }
                
                regex = new Regex("<tr class=.task([0-9]+).>");
                foreach (Match match in regex.Matches(doc))
                {
                    Task t = new Task();
                    t.idDivider = idDivider;
                    t.taskType = type;
                    t.taskId = Convert.ToInt32(match.Groups[1].Value);
                    tasks.Add(t);
                }

                if (tasks.Count==0) throw new Exception("Не удалось получить задания. Возможно нет доступных на данный момент");
                if (idDivider==0) throw new Exception("Не удаеться получить делитель");
            } else {
          throw new Exception("Сервер не отвечает (" + response.StatusCode.ToString() + ")");
        }
            return tasks;
        }
        
        public string openTask(Task t)
        {
            string url="";
            HttpWebResponse response = Program.GETRequest("https://ad-social.org/vk/earn/get/" + t.taskId / t.idDivider, _auth);
            if (response.StatusCode == HttpStatusCode.MovedPermanently)
            {
                HttpWebResponse response2 = Program.GETRequest(response.Headers["Location"], null);
                string doc = new StreamReader(response2.GetResponseStream()).ReadToEnd();
                if (Regex.IsMatch(doc, "<meta http-equiv=\"refresh\" content=\"0; URL=(.+)\">"))
                    {
                        Regex regex = new Regex("<meta http-equiv=\"refresh\" content=\"0; URL=(.+)\">");
                        foreach (Match match in regex.Matches(doc))
                        {
                            url = match.Groups[1].Value;
                        }
                    }
                if (Regex.IsMatch(doc, "<script>window.location.href = \'(.+)\'</script></head>"))
                    {

                        Regex regex = new Regex("<script>window.location.href = \'(.+)\'</script></head>");
                        foreach (Match match in regex.Matches(doc))
                        {
                            url = match.Groups[1].Value;
                        }
                    }
            }


            /*if(doc.Contains("<script src=\"https://www.google.com/recaptcha/api.js\" async defer></script>")) {
                    throw new SociaAdRobotCheck("https://ad-social.org/vk/earn/get/" + t.taskId / t.idDivider);
            }*/
            /*if (usfd.Contains("Заказ сейчас недоступен...Возможно, он уже полностью выполнен, либо заблокирован из - за нарушений, либо выполнен именно Вами"))
            {
                   throw new Exception("Заказ сейчас недоступен... Возможно, он уже полностью выполнен, либо заблокирован из-за нарушений, либо выполнен именно Вами");
            }
            */


            response.GetResponseStream().Close();
            response.Close();
            return url;
        }

        public string checkTask(Task t)
        {
            HttpWebResponse response = Program.GETRequest(@"https://ad-social.org/vk/earn/checkTask/" + t.taskId / t.idDivider + "/" + t.taskType, _auth);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(AdSocialResponse));
                AdSocialResponse req = (AdSocialResponse)js.ReadObject(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(new StreamReader(response.GetResponseStream()).ReadToEnd())));

                if(!req.status) throw new Exception("Задание не засчитано, возможно оно не выполнено");

                _balance += Convert.ToUInt32(req.reward);
                response.GetResponseStream().Close();
                response.Close();
                return "OK (" + req.reward + ")";
            } else {
                throw new Exception("Сервер не отвечает (" + response.StatusCode.ToString() + ")");
            }
        }

    }

    
    [DataContract]
    public class AdSocialResponse
    {
        [DataMember]
        public bool status { get; set; }
        [DataMember]
        public string balance { get; set; }
        [DataMember]
        public string reward{ get; set; }
    }
}
