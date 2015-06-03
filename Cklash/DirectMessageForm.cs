using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Library;

namespace Cklash
{
    public partial class DirectMessageForm : Form
    {
        public SlackApiCache Slack { get; set; }

        public DirectMessageForm()
        {
            InitializeComponent();
        }

        private void DirectMessageForm_Load(object sender, EventArgs e)
        {
            List<SlackApi.IMObject> imlist = Slack.Im.List();
            List<SlackApi.MessageInfo> dmlist = new List<SlackApi.MessageInfo>();
            foreach (var imobj in imlist)
            {
                List<SlackApi.MessageInfo> list = Slack.Im.History(imobj.Id);
                dmlist.AddRange(list);
            }

            foreach (var messageInfo in dmlist)
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

                string username = messageInfo.User;
                var user = Slack.GetUser(messageInfo.User);
                if (user != null && user.Profile.RealName != null)
                {
                    username = user.Profile.RealName;
                }
                var row = MessageList.Items.Insert(index, "");
                row.Text = username;
                row.ToolTipText = username + "\r\n" + messageInfo.Text;
                row.SubItems.Add(messageInfo.Text);
                row.SubItems.Add(messageInfo.TsTimestamp.ToString("yyyy/MM/dd HH:mm:ss"));
                row.Tag = messageInfo;
            }

        }
    }
}
