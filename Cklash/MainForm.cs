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
using System.Diagnostics;
using Library;
using System.Threading;
using System.Text.RegularExpressions;

namespace Cklash
{
    public partial class MainForm : Form
    {

        private SlackApiCache Slack = new SlackApiCache();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ChannelList = new List<SlackApi.ChannelInfo>();
            MessageList.SmallImageList = Slack.UserIconList;
            trayIcon.Icon = this.Icon;

            lastMessageTs = Properties.Settings.Default.lastMessageTs;
            lastReplyTs = Properties.Settings.Default.lastReplyTs;

            Slack.ClientId = Properties.Settings.Default.ClientId;
            Slack.ClientSecret = Properties.Settings.Default.ClientSecret;
            Slack.AccessToken = Properties.Settings.Default.AccessToken;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.UserClientId))
            {
                Slack.ClientId = Properties.Settings.Default.UserClientId;
            }
            if (!string.IsNullOrEmpty(Properties.Settings.Default.UserClientSecret))
            {
                Slack.ClientSecret = Properties.Settings.Default.UserClientSecret;
            }
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(Slack.ClientId));

            string appdataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            appdataDir = Path.Combine(appdataDir, Application.ProductName);
            if (!Directory.Exists(appdataDir))
            {
                Directory.CreateDirectory(appdataDir);
            }

            Slack.IconCacheDir = Path.Combine(appdataDir, "~IconCache");
            if (!Directory.Exists(Slack.IconCacheDir))
            {
                Directory.CreateDirectory(Slack.IconCacheDir);
            }

            if (!string.IsNullOrEmpty(Slack.AccessToken))
            {
                if (!Slack.Auth.Test())
                {
                    Slack.AccessToken = null;
                }
            }

            if (string.IsNullOrEmpty(Slack.AccessToken))
            {
                Visible = true;
                if (!Login())
                {
                    Close();
                    return;
                }
            }

            Visible = true;

            ThreadPool.QueueUserWorkItem(StartupCallback);

        }

        protected void StartupCallback(Object threadContext)
        {
            if (!Slack.IsLogin)
            {
                return;
            }

            UseWaitCursor = true;

            List<SlackApi.ChannelInfo> channels = Slack.ReloadChannel();
            ChannelList.Clear();
            ChannelList.Add(new SlackApi.ChannelInfo() { Name = "" });
            ChannelList.AddRange(channels);

            Slack.LoadUsers();
            Slack.LoadDirectMessage();

            Invoke(new EventHandler(StartupRefreshCallback));

            UseWaitCursor = false;
        }

        protected void StartupRefreshCallback(object sender, System.EventArgs e)
        {
            foreach (var item in ChannelList)
            {
                ChanellCombo.Items.Add(item.Name);
            }

            ThreadPool.QueueUserWorkItem(ReloadCallback);
            reloadTimer.Start();
        }

        protected List<SlackApiCache.MessageCacheInfo> reloadList = new List<SlackApiCache.MessageCacheInfo>();
        protected int directMessageCount;

        protected void ReloadCallback(Object threadContext)
        {
            if (!Slack.IsLogin)
            {
                return;
            }

            //UseWaitCursor = true;
            directMessageCount = Slack.LoadDirectMessage();

            List<SlackApiCache.MessageCacheInfo> tempList = new List<SlackApiCache.MessageCacheInfo>();

            tempList = Slack.LoadChannelHistory();

            lock (reloadList)
            {
                reloadList.AddRange(tempList);
            }

            Invoke(new EventHandler(ReloadRefreshCallback));

            //UseWaitCursor = false;
        }

        protected string lastMessageTs;
        protected string lastReplyTs;

        public void ReloadRefreshCallback(object sender, System.EventArgs e)
        {
            List<SlackApiCache.MessageCacheInfo> tempList = new List<SlackApiCache.MessageCacheInfo>();

            lock (reloadList)
            {
                tempList.AddRange(reloadList);
                reloadList.Clear();
            }

            UseWaitCursor = true;

            tempList = new List<SlackApiCache.MessageCacheInfo>(tempList.OrderByDescending(item => item.Ts));
            SlackApiCache.MessageCacheInfo replyMessage = null;

            foreach (var messageInfo in tempList)
            {
                int index = MessageList.Items.Count;
                for (int i = 0; i < MessageList.Items.Count; i++)
                {
                    SlackApi.MessageInfo info2 = (SlackApi.MessageInfo)MessageList.Items[i].Tag;
                    if (info2.TsTimestamp < messageInfo.TsTimestamp)
                    {
                        index = i;
                        break;
                    }
                }

                var row = MessageList.Items.Insert(index, "");
                row.Text = messageInfo.DisplayRealName;
                string username2 = "";
                if (messageInfo.UserDetail != null)
                {
                    username2 = "(@" + messageInfo.UserDetail.Name + ")";
                }
                row.ToolTipText = messageInfo.DisplayRealName + " " + username2 + "\r\n" + messageInfo.DisplayText;
                string text = messageInfo.DisplayText;
                row.SubItems.Add(text);
                row.SubItems.Add(messageInfo.ChannelDetail.Name);
                row.SubItems.Add(messageInfo.TsTimestamp.ToString("yyyy/MM/dd HH:mm:ss"));
                row.ImageKey = messageInfo.IconKey;
                row.Tag = messageInfo;
                if (messageInfo.Text != null && messageInfo.Text.IndexOf("<@" + Slack.UserId) >= 0)
                {
                    replyMessage = messageInfo;
                }
            }

            UseWaitCursor = false;

            if (directMessageCount > 0)
            {
                Visible = true;
                directMessageCount = 0;
                Activate();
                trayIcon.ShowBalloonTip(30000, Application.ProductName, "ダイレクトメッセージがあります。", ToolTipIcon.Warning);
                MessageBox.Show("ダイレクトメッセージがあります。");
            }
            else if (replyMessage != null && lastReplyTs != replyMessage.Ts)
            {
                trayIcon.ShowBalloonTip(30000, Application.ProductName, replyMessage.DisplayRealName + "\r\n" + replyMessage.DisplayText, ToolTipIcon.Warning);
                lastReplyTs = replyMessage.Ts;
            }
            else if (tempList.Count > 0 && lastMessageTs != tempList[0].Ts)
            {
                trayIcon.ShowBalloonTip(30000, Application.ProductName, tempList[0].DisplayRealName + "\r\n" + tempList[0].DisplayText, ToolTipIcon.Info);
                lastMessageTs = tempList[0].Ts;
            }

            if (tempList.Count > 0)
            {
                if (MessageList.Items.Count > 0)
                {
                    MessageList.Items[0].EnsureVisible();
                }
            }

        }

        public bool Login()
        {
            LoginForm form = new LoginForm();
            form.Slack = Slack;
            if (form.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
            {
                return false;
            }

            Properties.Settings.Default.AccessToken = Slack.AccessToken;
            Properties.Settings.Default.Save();

            return true;
        }

        private List<SlackApi.ChannelInfo> ChannelList = new List<SlackApi.ChannelInfo>();

        private void reloadTimer_Tick(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(ReloadCallback);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized && Visible)
            {
                Visible = false;
            }
        }

        private void trayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Visible = false;
                Visible = true;
                WindowState = FormWindowState.Normal;
                Activate();
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                Close();
            }
        }

        private void PostButton_Click(object sender, EventArgs e)
        {
            if (!Slack.IsLogin)
            {
                return;
            }

            if (PostText.Text == "")
            {
                UseWaitCursor = true;
                ThreadPool.QueueUserWorkItem(ReloadCallback);
                //ReloadData();
                UseWaitCursor = false;
                return;
            }

            if (ChanellCombo.SelectedIndex <= 0)
            {
                MessageBox.Show("チャネルを選択してください。");
                return;
            }

            UseWaitCursor = true;

            string chanell = ChannelList[ChanellCombo.SelectedIndex].Id;
            if (!Slack.Chat.PostMessage(chanell, PostText.Text))
            {
                MessageBox.Show("エラー：投稿に失敗しました。");
                UseWaitCursor = false;
                return;
            }

            PostText.Text = "";

            //ReloadData();
            ThreadPool.QueueUserWorkItem(ReloadCallback);
            UseWaitCursor = false;
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void showDirectMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DirectMessageForm form = new DirectMessageForm();
            form.Slack = Slack;
            form.ShowDialog(this);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Slack.SaveMessageCache();
            Properties.Settings.Default.lastMessageTs = lastMessageTs;
            Properties.Settings.Default.lastReplyTs = lastReplyTs;
            Properties.Settings.Default.Save();
        }

        private void slackHomeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://" + Slack.Team + ".slack.com/");
        }

        private void replyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageList.SelectedIndices.Count != 1)
            {
                return;
            }
            var item = MessageList.SelectedItems[0];
            var message = (SlackApiCache.MessageCacheInfo)item.Tag;
            if (message.UserDetail == null)
            {
                return;
            }
            PostText.Text = "@" + message.UserDetail.Name + ": ";

            string channel = item.SubItems[columnHeader3.Index].Text;
            for (int i = 0; i < ChanellCombo.Items.Count; i++)
            {
                if (((string)ChanellCombo.Items[i]) == channel)
                {
                    ChanellCombo.SelectedIndex = i;
                    break;
                }
            }
        }

        private void openURLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageList.SelectedIndices.Count != 1)
            {
                return;
            }
            var item = MessageList.SelectedItems[0];
            var message = (SlackApiCache.MessageCacheInfo)item.Tag;

            string text = message.Text;
            if (text == null)
            {
                return;
            }
            Match match = Regex.Match(text, "http[s]?://[^ |]+");
            if (!match.Success)
            {
                return;
            }
            string url = match.Value;
            System.Diagnostics.Debug.WriteLine(url);
            Process.Start(url);
        }

        private void detailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageList.SelectedIndices.Count != 1)
            {
                return;
            }
            var item = MessageList.SelectedItems[0];
            var message = (SlackApiCache.MessageCacheInfo)item.Tag;

            string url = "https://" + Slack.Team + ".slack.com/archives/" + message.ChannelDetail.Name + "/p" + message.Ts.Replace(".", "");
            System.Diagnostics.Debug.WriteLine(url);
            Process.Start(url);
        }

    }
}
