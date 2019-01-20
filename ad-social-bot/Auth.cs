using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ad_social_bot
{
    public partial class Auth : Form
    {
        BackgroundWorker bgw = new BackgroundWorker();

        public Auth()
        {
            InitializeComponent();
        }

        private void Auth_Load(object sender, EventArgs e)
        {
           bgw.WorkerReportsProgress = true;
            bgw.WorkerSupportsCancellation = true;
            bgw.DoWork += new DoWorkEventHandler(bgw_DoWork);
            bgw.ProgressChanged += new ProgressChangedEventHandler(bgw_Message);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_RunWorkerCompleted);
            bgw.RunWorkerAsync();
        }

        void bgw_Message(object sender, ProgressChangedEventArgs e)
        {
            var message = (e.UserState as Message);
            switch (Message.Type)
            {
                case Message.Types.Exception:
                    {
                        MessageBox.Show(message.Msg, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }
                    break;
                case Message.Types.Warning:
                    {
                        MessageBox.Show(message.Msg, "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    break;
                case Message.Types.Info:
                    {
                        status.Text = message.Msg;
                    }
                    break;
            }
        }

        void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (Program.settings.KeyExists("1", "Proxy"))
                {
                    bool isWork = false;
                    int i = 1;
                    bgw.ReportProgress(0, new Message(Message.Types.Info, "Ищем рабочий прокси..."));
                    string[] proxy = Program.settings.Read(i.ToString(), "Proxy").Split(':');
                    do
                    {
                        Program.proxy = new WebProxy(proxy[0], Convert.ToInt16(proxy[1]));
                        isWork = true;
                        try
                        {
                            bgw.ReportProgress(0, new Message(Message.Types.Info, "Ищем рабочий прокси... " + i.ToString()));
                            Program.GETRequest("http://google.com", null);
                        }
                        catch (Exception ex)
                        {
                            isWork = false;
                        }
                        i++;
                        proxy = Program.settings.Read(i.ToString(), "Proxy").Split(':');
                    } while (proxy.Length > 1 && !isWork);
                    if (!isWork) throw new Exception("Ни один проски из списка не отвечает");
                }

                bgw.ReportProgress(0, new Message(Message.Types.Info, "Авторизация ВК..."));

                string lg_h = "", UloginToken = "", temp = "";
                // remixlhk, lg_h // VKMainPage
                HttpWebResponse VKMainPage = Program.GETRequest("https://vk.com/", null);
                temp = (new StreamReader(VKMainPage.GetResponseStream()).ReadToEnd());
                Regex regex = new Regex("name=\"lg_h\" value=\"(.+)\"");
                Match match = regex.Match(temp);
                if (match.Success) lg_h = match.Groups[1].Value;
                else throw new Exception("Не удается спарсить lg_h на vk.com");
                // p, s , i itd
                HttpWebResponse VKauth1 = Program.POSTRequest("https://login.vk.com/?act=login", String.Format("act=login&role=al_frame&expire=&recaptcha=&captcha_sid=&captcha_key=&_origin=https://vk.com&ip_h=0&lg_h={0}&email={1}&pass={2}", lg_h, Program.settings.Read("login", "VK"), Program.settings.Read("password", "VK")), VKMainPage.Cookies);

                // remixsid
                HttpWebResponse VKauth2 = Program.GETRequest(VKauth1.Headers["Location"], VKauth1.Cookies);

                bgw.ReportProgress(0, new Message(Message.Types.Info, "Получение ключа доступа ВК..."));
                // remixlhk
                HttpWebResponse VKoauth1 = Program.GETRequest("https://oauth.vk.com/authorize?client_id=4275827&display=page&redirect_uri=https://oauth.vk.com/blank.html&scope=friends,wall,groups,offline&response_type=token", VKauth2.Cookies);

                CookieCollection VKoauth = new CookieCollection();
                VKoauth.Add(VKauth2.Cookies);
                VKoauth.Add(VKauth1.Cookies);
                VKoauth.Add(VKoauth1.Cookies);
                // remixlhk, p, s , i itd, remixsid
                HttpWebResponse VKoauth2 = Program.GETRequest(VKoauth1.Headers["Location"], VKoauth);

                regex = new Regex(@"#access_token=([\w]+)&");
                match = regex.Match(VKoauth2.Headers["Location"]);
                if (match.Success) Program.VKtoken = match.Groups[1].Value;
                else throw new Exception("Не удается получить access_token");

                bgw.ReportProgress(0, new Message(Message.Types.Info, "Авторизация Ulogin..."));
                // ulogin, ulog
                HttpWebResponse Ulogin1 = Program.GETRequest("https://ulogin.ru/auth.php?name=vkontakte&window=0&lang=ru&fields=uid,first_name,last_name,photo,photo_big&force_fields=&optional=email,bdate,sex,city,country&redirect_uri=https://ad-social.org/vk/social2/login&host=https://ad-social.org/&page=https://ad-social.org/&callback=&verify=&mobile=1&m=1&screen=414x736&altway=1&q=", null);

                bgw.ReportProgress(0, new Message(Message.Types.Info, "Получение ключа доступа для Ulogin..."));
                // remixlhk
                HttpWebResponse UloginVKOauth1 = Program.GETRequest(Ulogin1.Headers["Location"], VKauth2.Cookies);

                CookieCollection UloginVKOauth = new CookieCollection();
                UloginVKOauth.Add(VKauth2.Cookies);
                UloginVKOauth.Add(UloginVKOauth1.Cookies);
                UloginVKOauth.Add(VKauth1.Cookies);

                // remixlhk, remixsid, p, s , i itd
                HttpWebResponse UloginVKOauth2 = Program.GETRequest(UloginVKOauth1.Headers["Location"], UloginVKOauth);
                bgw.ReportProgress(0, new Message(Message.Types.Info, "Получение токена для Ad-social..."));
                // ulogin, ulog
                HttpWebResponse Ulogin2 = Program.GETRequest(UloginVKOauth2.Headers["Location"], Ulogin1.Cookies);
                temp = (new StreamReader(Ulogin2.GetResponseStream()).ReadToEnd());
                regex = new Regex("name=\"token\" value=\"(.+)\"/>");
                match = regex.Match(temp);
                if (match.Success) UloginToken = match.Groups[1].Value;
                else throw new Exception();

                bgw.ReportProgress(0, new Message(Message.Types.Info, "Авторизация на Ad-social..."));
                HttpWebResponse AdSoc1 = Program.POSTRequest("https://ad-social.org/vk/social2/login", "token=" + UloginToken, null);

                bgw.ReportProgress(0, new Message(Message.Types.Info, "Завершение"));
                Program.ADauth = AdSoc1.Cookies;
            }
            catch (Exception ex)
            {
                bgw.RunWorkerCompleted -= bgw_RunWorkerCompleted;
                bgw.ReportProgress(0, new Message(Message.Types.Exception, ex.Message));
            }

}

        void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Close();
        }
    }

    public class Message
    {
        public static Types Type { get; private set; }
        public string Msg { get; private set; }
        public enum Types { Exception, Warning, Info }
        public Message(Types type, string message)
        {
            Type = type;
            Msg = message;
        }
    }
}