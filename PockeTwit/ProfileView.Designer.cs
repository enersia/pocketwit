namespace PockeTwit
{
    partial class ProfileView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblUserName = new System.Windows.Forms.Label();
            this.avatarBox = new System.Windows.Forms.PictureBox();
            this.lblFullName = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblPosition = new System.Windows.Forms.LinkLabel();
            this.lblURL = new System.Windows.Forms.LinkLabel();
            this.lblText1 = new System.Windows.Forms.Label();
            this.lblText2 = new System.Windows.Forms.Label();
            this.lblText3 = new System.Windows.Forms.Label();
            this.lblText4 = new System.Windows.Forms.Label();
            this.llblFollowers = new System.Windows.Forms.LinkLabel();
            this.llblTweets = new System.Windows.Forms.LinkLabel();
            this.llblFollowing = new System.Windows.Forms.LinkLabel();
            this.llblFavorites = new System.Windows.Forms.LinkLabel();
            this.lblText5 = new System.Windows.Forms.Label();
            this.lblText6 = new System.Windows.Forms.Label();
            this.lblJoinedOn = new System.Windows.Forms.Label();
            this.pContent = new System.Windows.Forms.Panel();
            this.pViewer = new System.Windows.Forms.Panel();
            this.timeranimate = new System.Windows.Forms.Timer();
            this.pContent.SuspendLayout();
            this.pViewer.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblUserName
            // 
            this.lblUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblUserName.Location = new System.Drawing.Point(92, 23);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(149, 20);
            this.lblUserName.Text = "label1";
            // 
            // avatarBox
            // 
            this.avatarBox.Location = new System.Drawing.Point(3, 3);
            this.avatarBox.Name = "avatarBox";
            this.avatarBox.Size = new System.Drawing.Size(83, 71);
            this.avatarBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.avatarBox.Click += new System.EventHandler(this.avatarBox_Click);
            // 
            // lblFullName
            // 
            this.lblFullName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFullName.Location = new System.Drawing.Point(92, 3);
            this.lblFullName.Name = "lblFullName";
            this.lblFullName.Size = new System.Drawing.Size(149, 20);
            this.lblFullName.Text = "label1";
            // 
            // lblDescription
            // 
            this.lblDescription.Location = new System.Drawing.Point(0, 161);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(242, 128);
            this.lblDescription.Text = "label1";
            // 
            // lblPosition
            // 
            this.lblPosition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPosition.Location = new System.Drawing.Point(66, 77);
            this.lblPosition.Name = "lblPosition";
            this.lblPosition.Size = new System.Drawing.Size(171, 20);
            this.lblPosition.TabIndex = 7;
            this.lblPosition.TabStop = false;
            this.lblPosition.Click += new System.EventHandler(this.lblPosition_Click);
            // 
            // lblURL
            // 
            this.lblURL.Location = new System.Drawing.Point(92, 43);
            this.lblURL.Name = "lblURL";
            this.lblURL.Size = new System.Drawing.Size(229, 20);
            this.lblURL.TabIndex = 12;
            this.lblURL.TabStop = false;
            this.lblURL.Click += new System.EventHandler(this.lblURL_Click);
            // 
            // lblText1
            // 
            this.lblText1.Location = new System.Drawing.Point(0, 99);
            this.lblText1.Name = "lblText1";
            this.lblText1.Size = new System.Drawing.Size(64, 20);
            this.lblText1.Text = "Followers:";
            // 
            // lblText2
            // 
            this.lblText2.Location = new System.Drawing.Point(122, 99);
            this.lblText2.Name = "lblText2";
            this.lblText2.Size = new System.Drawing.Size(64, 20);
            this.lblText2.Text = "Following:";
            // 
            // lblText3
            // 
            this.lblText3.Location = new System.Drawing.Point(0, 121);
            this.lblText3.Name = "lblText3";
            this.lblText3.Size = new System.Drawing.Size(64, 20);
            this.lblText3.Text = "Tweets:";
            // 
            // lblText4
            // 
            this.lblText4.Location = new System.Drawing.Point(122, 121);
            this.lblText4.Name = "lblText4";
            this.lblText4.Size = new System.Drawing.Size(64, 20);
            this.lblText4.Text = "Favorites:";
            // 
            // llblFollowers
            // 
            this.llblFollowers.Location = new System.Drawing.Point(66, 99);
            this.llblFollowers.Name = "llblFollowers";
            this.llblFollowers.Size = new System.Drawing.Size(53, 20);
            this.llblFollowers.TabIndex = 20;
            this.llblFollowers.TabStop = false;
            // 
            // llblTweets
            // 
            this.llblTweets.Location = new System.Drawing.Point(66, 121);
            this.llblTweets.Name = "llblTweets";
            this.llblTweets.Size = new System.Drawing.Size(53, 20);
            this.llblTweets.TabIndex = 21;
            this.llblTweets.TabStop = false;
            this.llblTweets.Click += new System.EventHandler(this.llblTweets_Click);
            // 
            // llblFollowing
            // 
            this.llblFollowing.Location = new System.Drawing.Point(187, 99);
            this.llblFollowing.Name = "llblFollowing";
            this.llblFollowing.Size = new System.Drawing.Size(53, 20);
            this.llblFollowing.TabIndex = 22;
            this.llblFollowing.TabStop = false;
            // 
            // llblFavorites
            // 
            this.llblFavorites.Location = new System.Drawing.Point(187, 121);
            this.llblFavorites.Name = "llblFavorites";
            this.llblFavorites.Size = new System.Drawing.Size(53, 20);
            this.llblFavorites.TabIndex = 23;
            this.llblFavorites.TabStop = false;
            this.llblFavorites.Click += new System.EventHandler(this.llblFavorites_Click);
            // 
            // lblText5
            // 
            this.lblText5.Location = new System.Drawing.Point(0, 77);
            this.lblText5.Name = "lblText5";
            this.lblText5.Size = new System.Drawing.Size(64, 20);
            this.lblText5.Text = "Location:";
            // 
            // lblText6
            // 
            this.lblText6.Location = new System.Drawing.Point(0, 141);
            this.lblText6.Name = "lblText6";
            this.lblText6.Size = new System.Drawing.Size(64, 20);
            this.lblText6.Text = "Joined on:";
            // 
            // lblJoinedOn
            // 
            this.lblJoinedOn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblJoinedOn.Location = new System.Drawing.Point(66, 141);
            this.lblJoinedOn.Name = "lblJoinedOn";
            this.lblJoinedOn.Size = new System.Drawing.Size(149, 20);
            // 
            // pContent
            // 
            this.pContent.Controls.Add(this.lblJoinedOn);
            this.pContent.Controls.Add(this.lblText6);
            this.pContent.Controls.Add(this.lblText5);
            this.pContent.Controls.Add(this.llblFavorites);
            this.pContent.Controls.Add(this.llblFollowing);
            this.pContent.Controls.Add(this.llblTweets);
            this.pContent.Controls.Add(this.llblFollowers);
            this.pContent.Controls.Add(this.lblText4);
            this.pContent.Controls.Add(this.lblText3);
            this.pContent.Controls.Add(this.lblText2);
            this.pContent.Controls.Add(this.lblText1);
            this.pContent.Controls.Add(this.lblURL);
            this.pContent.Controls.Add(this.lblPosition);
            this.pContent.Controls.Add(this.lblDescription);
            this.pContent.Controls.Add(this.lblFullName);
            this.pContent.Controls.Add(this.lblUserName);
            this.pContent.Controls.Add(this.avatarBox);
            this.pContent.Location = new System.Drawing.Point(0, 0);
            this.pContent.Name = "pContent";
            this.pContent.Size = new System.Drawing.Size(300, 500);
            this.pContent.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pContent_MouseMove);
            this.pContent.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pContent_MouseDown);
            this.pContent.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pContent_MouseUp);
            // 
            // pViewer
            // 
            this.pViewer.Controls.Add(this.pContent);
            this.pViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pViewer.Location = new System.Drawing.Point(0, 0);
            this.pViewer.Name = "pViewer";
            this.pViewer.Size = new System.Drawing.Size(240, 294);
            // 
            // timeranimate
            // 
            this.timeranimate.Tick += new System.EventHandler(this.timeranimate_Tick);
            // 
            // ProfileView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(240, 294);
            this.Controls.Add(this.pViewer);
            this.Name = "ProfileView";
            this.Text = "ProfileView";
            this.pContent.ResumeLayout(false);
            this.pViewer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.PictureBox avatarBox;
        private System.Windows.Forms.Label lblFullName;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.LinkLabel lblPosition;
        private System.Windows.Forms.LinkLabel lblURL;
        private System.Windows.Forms.Label lblText1;
        private System.Windows.Forms.Label lblText2;
        private System.Windows.Forms.Label lblText3;
        private System.Windows.Forms.Label lblText4;
        private System.Windows.Forms.LinkLabel llblFollowers;
        private System.Windows.Forms.LinkLabel llblTweets;
        private System.Windows.Forms.LinkLabel llblFollowing;
        private System.Windows.Forms.LinkLabel llblFavorites;
        private System.Windows.Forms.Label lblText5;
        private System.Windows.Forms.Label lblText6;
        private System.Windows.Forms.Label lblJoinedOn;
        private System.Windows.Forms.Panel pContent;
        private System.Windows.Forms.Panel pViewer;
        private System.Windows.Forms.Timer timeranimate;
    }
}