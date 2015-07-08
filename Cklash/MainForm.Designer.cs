namespace Cklash
{
    partial class MainForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.MessageList = new System.Windows.Forms.ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.detailToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openURLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PostText = new System.Windows.Forms.TextBox();
            this.PostButton = new System.Windows.Forms.Button();
            this.reloadTimer = new System.Windows.Forms.Timer(this.components);
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.ChanellCombo = new System.Windows.Forms.ComboBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.applicationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.slackHomeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDirectMessageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reloadDmTimer = new System.Windows.Forms.Timer(this.components);
            this.contextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // MessageList
            // 
            this.MessageList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2,
            this.columnHeader1,
            this.columnHeader3,
            this.columnHeader4});
            this.MessageList.ContextMenuStrip = this.contextMenu;
            this.MessageList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageList.FullRowSelect = true;
            this.MessageList.Location = new System.Drawing.Point(0, 0);
            this.MessageList.Name = "MessageList";
            this.MessageList.ShowItemToolTips = true;
            this.MessageList.Size = new System.Drawing.Size(784, 483);
            this.MessageList.TabIndex = 0;
            this.MessageList.UseCompatibleStateImageBehavior = false;
            this.MessageList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "User";
            this.columnHeader2.Width = 115;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Message";
            this.columnHeader1.Width = 353;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Channel";
            this.columnHeader3.Width = 147;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Date";
            this.columnHeader4.Width = 130;
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.detailToolStripMenuItem,
            this.replyToolStripMenuItem,
            this.openURLToolStripMenuItem});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new System.Drawing.Size(133, 70);
            // 
            // detailToolStripMenuItem
            // 
            this.detailToolStripMenuItem.Name = "detailToolStripMenuItem";
            this.detailToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.detailToolStripMenuItem.Text = "Detail";
            this.detailToolStripMenuItem.Click += new System.EventHandler(this.detailToolStripMenuItem_Click);
            // 
            // replyToolStripMenuItem
            // 
            this.replyToolStripMenuItem.Name = "replyToolStripMenuItem";
            this.replyToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.replyToolStripMenuItem.Text = "Reply";
            this.replyToolStripMenuItem.Click += new System.EventHandler(this.replyToolStripMenuItem_Click);
            // 
            // openURLToolStripMenuItem
            // 
            this.openURLToolStripMenuItem.Name = "openURLToolStripMenuItem";
            this.openURLToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.openURLToolStripMenuItem.Text = "Open URL";
            this.openURLToolStripMenuItem.Click += new System.EventHandler(this.openURLToolStripMenuItem_Click);
            // 
            // PostText
            // 
            this.PostText.Dock = System.Windows.Forms.DockStyle.Top;
            this.PostText.Location = new System.Drawing.Point(163, 3);
            this.PostText.Name = "PostText";
            this.PostText.Size = new System.Drawing.Size(503, 19);
            this.PostText.TabIndex = 1;
            // 
            // PostButton
            // 
            this.PostButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.PostButton.Location = new System.Drawing.Point(672, 3);
            this.PostButton.Name = "PostButton";
            this.PostButton.Size = new System.Drawing.Size(109, 23);
            this.PostButton.TabIndex = 2;
            this.PostButton.Text = "Post";
            this.PostButton.UseVisualStyleBackColor = true;
            this.PostButton.Click += new System.EventHandler(this.PostButton_Click);
            // 
            // reloadTimer
            // 
            this.reloadTimer.Interval = 60000;
            this.reloadTimer.Tick += new System.EventHandler(this.reloadTimer_Tick);
            // 
            // trayIcon
            // 
            this.trayIcon.Text = "Cklash";
            this.trayIcon.Visible = true;
            this.trayIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.trayIcon_MouseClick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.MessageList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer1.Size = new System.Drawing.Size(784, 517);
            this.splitContainer1.SplitterDistance = 483;
            this.splitContainer1.TabIndex = 3;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 23.97661F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 76.02339F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 114F));
            this.tableLayoutPanel1.Controls.Add(this.PostText, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.PostButton, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.ChanellCombo, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(784, 30);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // ChanellCombo
            // 
            this.ChanellCombo.Dock = System.Windows.Forms.DockStyle.Top;
            this.ChanellCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ChanellCombo.FormattingEnabled = true;
            this.ChanellCombo.Location = new System.Drawing.Point(3, 3);
            this.ChanellCombo.Name = "ChanellCombo";
            this.ChanellCombo.Size = new System.Drawing.Size(154, 20);
            this.ChanellCombo.TabIndex = 3;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.applicationToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(784, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "mainMenu";
            // 
            // applicationToolStripMenuItem
            // 
            this.applicationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.slackHomeToolStripMenuItem,
            this.showDirectMessageToolStripMenuItem,
            this.quitToolStripMenuItem});
            this.applicationToolStripMenuItem.Name = "applicationToolStripMenuItem";
            this.applicationToolStripMenuItem.Size = new System.Drawing.Size(82, 20);
            this.applicationToolStripMenuItem.Text = "Application";
            // 
            // slackHomeToolStripMenuItem
            // 
            this.slackHomeToolStripMenuItem.Name = "slackHomeToolStripMenuItem";
            this.slackHomeToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.slackHomeToolStripMenuItem.Text = "SlackHome";
            this.slackHomeToolStripMenuItem.Click += new System.EventHandler(this.slackHomeToolStripMenuItem_Click);
            // 
            // showDirectMessageToolStripMenuItem
            // 
            this.showDirectMessageToolStripMenuItem.Name = "showDirectMessageToolStripMenuItem";
            this.showDirectMessageToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.showDirectMessageToolStripMenuItem.Text = "ShowDirectMessage";
            this.showDirectMessageToolStripMenuItem.Click += new System.EventHandler(this.showDirectMessageToolStripMenuItem_Click);
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.quitToolStripMenuItem.Text = "Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // reloadDmTimer
            // 
            this.reloadDmTimer.Interval = 60000;
            this.reloadDmTimer.Tick += new System.EventHandler(this.reloadDmTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 541);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Cklash";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.contextMenu.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView MessageList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.TextBox PostText;
        private System.Windows.Forms.Button PostButton;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Timer reloadTimer;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox ChanellCombo;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem applicationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showDirectMessageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem slackHomeToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem replyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openURLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem detailToolStripMenuItem;
        private System.Windows.Forms.Timer reloadDmTimer;
    }
}

