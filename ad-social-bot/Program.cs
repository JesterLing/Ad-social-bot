using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.IO;

namespace ad_social_bot
{
    static class Program
    {
        public static string VKtoken = null;
        public static CookieCollection ADauth = null;

        public static WebProxy proxy = null;

        public static MainForm main;
        public static Settings settings;

        /// <summary>
        /// Отправить GET запрос на урл
        /// </summary>
        public static HttpWebResponse GETRequest(string URL, CookieCollection cookies)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            else
            {
                request.CookieContainer = new CookieContainer();
            }
            if (proxy != null) request.Proxy = proxy;
            request.AllowAutoRedirect = false;
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response;
        }
        /// <summary>
        /// Отправить POST запрос на урл
        /// </summary>

        public static HttpWebResponse POSTRequest(string URL, string data, CookieCollection cookies)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            else
            {
                request.CookieContainer = new CookieContainer();
            }
            if (proxy != null) request.Proxy = proxy;
            request.AllowAutoRedirect = false;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            byte[] dataByte = System.Text.Encoding.UTF8.GetBytes(data);
            request.ContentLength = dataByte.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(dataByte, 0, dataByte.Length);
            dataStream.Close();
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response;
        }
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Конфигурация
            if (!File.Exists(Environment.CurrentDirectory + "\\settings.ini"))
            {
                MessageBox.Show("Не найден файл с конфигурацией settings.ini", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            settings = new Settings(Environment.CurrentDirectory + "\\settings.ini");
            if(!settings.KeyExists("login", "VK") || !settings.KeyExists("password", "VK"))
            {
                MessageBox.Show("На найден логин или пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Понеслась
            Auth auth = new Auth();
            Application.Run(auth);

            main = new MainForm();
            Application.Run(main);
        }
    }
}
