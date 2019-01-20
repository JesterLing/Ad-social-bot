using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.IO;
using System.Threading;


namespace ad_social_bot
{


    public partial class MainForm : Form
    {
        int performTasks = 0;
        AdSocial AdSocial;
        VK Vk;

        List<Task> tasks = new List<Task>();

        static BackgroundWorker doTasksBackgroundWorker;

        public int _likesCounter
        {
            set { label5.Text = Convert.ToString(value); }
            get { return Convert.ToInt32(label5.Text); }
        }
        public int _friendsCounter
        {
            set { label6.Text = Convert.ToString(value); }
            get { return Convert.ToInt32(label6.Text); }
        }
        public int _groupsCounter
        {
            set { label7.Text = Convert.ToString(value); }
            get { return Convert.ToInt32(label7.Text); }
        }
        public int _repostsCounter
        {
            set { label8.Text = Convert.ToString(value); }
            get { return Convert.ToInt32(label8.Text); }
        }

        public int _limitation
        {
            set { numericUpDown1.Value = value; }
            get { return Convert.ToInt32(numericUpDown1.Value); }
        }
        public uint _balance
        {
            set { label10.Text = Convert.ToString(value); }
            get { return Convert.ToUInt32(label10.Text); }
        }
        public string _name
        {
            set { label15.Text = value; }
            get { return label15.Text; }
        }

        public MainForm()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;

            logAddInfo("ad-social.org bot " + Application.ProductVersion, false);
            stopButton.Enabled = false;
            try
            {
                AdSocial = new AdSocial(Program.ADauth);
            }
            catch (InvalidAdSocialAuthorization ex)
            {
                logAddInfo("Авторизация Ad-Social не удалась", true);
                MessageBox.Show("Авторизация Ad-Social не удалась", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (SociaAdRobotCheck ex)
            {
                logAddInfo("Подтвердите что вы не робот " + ex.Message, true);
                return;
            }
            catch (Exception ex)
            {
                logAddInfo(ex.Message, true);
            }
            logAddInfo("Авторизация Ad-Social > успешно", false);
            logAddInfo("Ad_sessions " + Program.ADauth["ad_sessions"].Value, false);
            _balance = AdSocial.Balance;
            try
            {
                Vk = new VK(Program.VKtoken);
            }
            catch (InvalidAccessToken ex)
            {
                logAddInfo("Авторизация VK не удалась", true);
                MessageBox.Show("Авторизация VK не удалась", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (VKCaptchaNeeded ex)
            {
                logAddInfo("Капча ВК " + ex.Message, true);
                return;
            }
            catch (Exception ex)
            {
                logAddInfo(ex.Message, true);
            }
            logAddInfo("Авторизация ВК > успешно", false);
            logAddInfo("Токен " + Program.VKtoken, false);
            // _name = Vk.name;
            _name = "error!";
        }


        private void startButton_Click(object sender, EventArgs e)
        {
            if (!checkBox1.Checked && !checkBox2.Checked && !checkBox3.Checked && !checkBox4.Checked)
            {
                MessageBox.Show("Э даун не выбрано ни одно задание", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            logAddInfo("Старт", false);
            groupBox1.Enabled = false;
            groupBox3.Enabled = false;
            startButton.Enabled = false;
            stopButton.Enabled = true;
            logBox.Focus();
            doTasks(null,null);
        }

        private void doTasks(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e!=null)
            {
                if (e.Cancelled)
                {
                    logBox.Focus();
                    groupBox1.Enabled = true;
                    groupBox3.Enabled = true;
                    startButton.Enabled = true;
                    stopButton.Enabled = false;
                    logAddInfo("Прервано", false);
                    tasks.Clear();
                    performTasks = 0;
                    return;
                }
            }

            doTasksBackgroundWorker = new BackgroundWorker();
            doTasksBackgroundWorker.WorkerSupportsCancellation = true;
            doTasksBackgroundWorker.DoWork += doTasksThread;
            doTasksBackgroundWorker.RunWorkerCompleted += doTasks;
            doTasksBackgroundWorker.RunWorkerAsync(null);
        }

        private void doTasksThread(object sender, DoWorkEventArgs e)
        {

            if (tasks == null || tasks.Count == 0)
            {
                int cout = 0;
                logAddInfo("Получаю выбраные задания", false);
                List<Task> Likes = null;
                List<Task> Friends = null;
                List<Task> Groups = null;
                List<Task> Reposts = null;
                try
                {
                    if (checkBox1.Checked)
                    {
                        Likes = AdSocial.getTasks("like");
                        logAddInfo("Тип заданий: like | Получено заданий: " + Likes.Count + " | " + "Делитель заданий: " + Likes[0].idDivider, false);
                        Thread.Sleep(1000);
                    }
                }
                catch (InvalidAdSocialAuthorization ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    logAddInfo(ex.Message, true);
                    e.Cancel = true;
                    return;
                }
                catch (SociaAdRobotCheck ex)
                {
                    logAddInfo("Подтвердите что вы не робот " + ex.Message, true);
                    e.Cancel = true;
                    return;
                }
                catch (Exception ex)
                {
                    logAddInfo(ex.Message, false);
                }

                try
                {
                    if (checkBox2.Checked)
                    {
                        Friends = AdSocial.getTasks("friend");
                        logAddInfo("Тип заданий: friend | Получено заданий: " + Friends.Count + " | " + "Делитель заданий: " + Friends[0].idDivider, false);
                        Thread.Sleep(1000);
                    }
                }
                catch (InvalidAdSocialAuthorization ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    logAddInfo(ex.Message, true);
                    e.Cancel = true;
                    return;
                }
                catch (SociaAdRobotCheck ex)
                {
                    logAddInfo("Подтвердите что вы не робот " + ex.Message, true);
                    e.Cancel = true;
                    return;
                }
                
                catch (Exception ex)
                {
                    logAddInfo(ex.Message, false);
                }

                try
                {
                    if (checkBox3.Checked)
                    {
                        Groups = AdSocial.getTasks("group");
                        logAddInfo("Тип заданий: group | Получено заданий: " + Groups.Count + " | " + "Делитель заданий: " + Groups[0].idDivider, false);
                        Thread.Sleep(1000);
                    }
                }
                catch (InvalidAdSocialAuthorization ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    logAddInfo(ex.Message, true);
                    e.Cancel = true;
                    return;
                }
                catch (SociaAdRobotCheck ex)
                {
                    logAddInfo("Подтвердите что вы не робот " + ex.Message, true);
                    e.Cancel = true;
                    return;
                }
                catch (Exception ex)
                {
                    logAddInfo(ex.Message, false);
                }

                if (doTasksBackgroundWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                if (Likes != null) cout += Likes.Count;
                if (Friends != null) cout += Friends.Count;
                if (Groups != null) cout += Groups.Count;
                if (Reposts != null) cout += Reposts.Count;
                if (cout > 0) logAddInfo("Построение заданий...", false);
                for (int i = 0; i < cout;)
                {
                    if (Likes != null && Likes.Count != 0)
                    {
                        tasks.Add(Likes.First());
                        Likes.Remove(Likes.First());
                        i++;
                    }
                    if (Friends != null && Friends.Count != 0)
                    {
                        tasks.Add(Friends.First());
                        Friends.Remove(Friends.First());
                        i++;
                    }
                    if (Groups != null && Groups.Count != 0)
                    {
                        tasks.Add(Groups.First());
                        Groups.Remove(Groups.First());
                        i++;
                    }
                    if (Reposts != null && Reposts.Count != 0)
                    {
                        tasks.Add(Reposts.First());
                        Reposts.Remove(Reposts.First());
                        i++;
                    }
                }
                _balance = AdSocial.Balance;
                if (cout > 0) logAddInfo("Список из " + tasks.Count + " заданий построен", false);
            }
            int wait = 0;
            if (doTasksBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            try
            {
                logAddInfo("Выполняем задание..." + tasks.First().taskId, false);
                string task_vk_url = AdSocial.openTask(tasks.First());
                logAddInfo("VK URL: " + task_vk_url, false);
                if (doTasksBackgroundWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                switch (tasks.First().taskType)
                {
                    case "like":
                        logAddInfo("VK likes.add request: " + Vk.AddLike(task_vk_url), false);
                        break;
                    case "friend":
                        logAddInfo("VK friends.add request: " + Vk.AddFriend(task_vk_url), false);
                        break;
                    case "group":
                        logAddInfo("VK groups.join request: " + Vk.JoinGroup(task_vk_url), false);
                        break;
                    case "repost":
                        logAddInfo("VK wall.repost request: " + Vk.AddRepost(task_vk_url),false);
                       break;
                }
                logAddInfo("Ad-social checkTask request: " + AdSocial.checkTask(tasks.First()),false);
                switch (tasks.First().taskType)
                {
                    case "like":
                        _likesCounter++;
                        break;
                    case "friend":
                        _friendsCounter++;
                        break;
                    case "group":
                        _groupsCounter++;
                        break;
                    case "repost":
                        _repostsCounter++;
                        break;
                }

                _balance = AdSocial.Balance;
                tasks.Remove(tasks.First());
                performTasks++;
            }
            catch (VKCaptchaNeeded ex)
            {
                logAddInfo("Капча ВК" + ex.Message, true);
                e.Cancel = true;
                return;
            }
            catch (SociaAdRobotCheck ex)
            {
                logAddInfo("Подтвердите что вы не робот " + ex.Message, true);
                e.Cancel = true;
                return;
            }
            catch (InvalidOperationException ex)
            {
                wait = 60;
            }
            catch (Exception ex)
            {
                logAddInfo(ex.Message, false);
                tasks.Remove(tasks.First());
            }
            if (doTasksBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            if(_limitation!=0)
            {
                if(performTasks >= _limitation)
                {
                    logAddInfo("Задание количество заданий (" + _limitation + ") выполнено", true);
                    e.Cancel = true;
                    return;
                }
            }
            try
            {
                Random rd = new Random();
                int delay = rd.Next(Convert.ToInt32(delayFrom.Value), Convert.ToInt32(delayTo.Value));
                logAddInfo("Ожидаем " + (delay + wait) + " сек", false);
                for(int i=0; i<50; i++)
                {
                    if (doTasksBackgroundWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    Thread.Sleep((delay + wait) * 1000 / 50);
                }
            } catch(ArgumentOutOfRangeException ex)
            {
                logAddInfo("Неправельно настроена задержка. " + delayFrom.Value + ">" + delayTo.Value, true);
                e.Cancel = true;
                return;
            }
        }

        public void logAddInfo(string text, bool toolTip)
        {
            if (logBox.TextLength > 8000)
            {
                logBox.Clear();
                logAddInfo("Очищено", false);
            }
            if (toolTip)
            {
                if (this.WindowState == FormWindowState.Minimized) notifyIcon1.ShowBalloonTip(5000, "Info", text, ToolTipIcon.Info);
            }

            logBox.Text += "[" + DateTime.Now.ToLongTimeString() + "] " + text + Environment.NewLine;
        }

        private void logBox_TextChanged(object sender, EventArgs e)
        {
            logBox.SelectionStart = logBox.Text.Length;
            logBox.ScrollToCaret();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            logAddInfo("Прерываем текущие операции...",false);
            if (doTasksBackgroundWorker.IsBusy)
            {
                doTasksBackgroundWorker.CancelAsync();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            if(doTasksBackgroundWorker!=null)
            { 
                if (doTasksBackgroundWorker.IsBusy)
                {
                    doTasksBackgroundWorker.CancelAsync();
                }
            }
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.Visible = true;
                this.ShowInTaskbar = false;
                this.Hide();
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = true;
                this.Show();
                this.Activate();
                this.Focus();
                WindowState = FormWindowState.Normal;
                notifyIcon1.Visible = false;
                logBox.SelectionStart = logBox.Text.Length;
                logBox.ScrollToCaret();
            }
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            notifyIcon1_Click(null,null);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
