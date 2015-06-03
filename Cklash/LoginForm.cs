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
    public partial class LoginForm : Form
    {
        public SlackApi Slack { get; set; }

        public LoginForm()
        {
            InitializeComponent();
        }

        private void EnterButton_Click(object sender, EventArgs e)
        {
            string code = CodeText.Text;
            if (code.IndexOf("code=") >= 0)
            {
                code = code.Substring(code.IndexOf("code=") + 5);
            }
            if (code.IndexOf("&") >= 0)
            {
                code = code.Substring(0, code.IndexOf("&"));
            }
            if (!Slack.OauthAccess(code))
            {
                MessageBox.Show("Login failed.");
                return;
            }
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            Slack.OpenAuthorizeUrl();
        }
    }
}
