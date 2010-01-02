namespace PockeTwit
{
    partial class ProfileViewSmartPhone
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProfileViewSmartPhone));
            this.avatarBox = new System.Windows.Forms.PictureBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblFavorites = new System.Windows.Forms.Label();
            this.llblTweets = new System.Windows.Forms.LinkLabel();
            this.lblFollowingNumber = new System.Windows.Forms.LinkLabel();
            this.llblFavorites = new System.Windows.Forms.LinkLabel();
            this.lblJoinedOnDate = new System.Windows.Forms.Label();
            this.panelBasicInfo = new System.Windows.Forms.Panel();
            this.lblURL = new System.Windows.Forms.LinkLabel();
            this.lblPosition = new System.Windows.Forms.LinkLabel();
            this.lblLocation = new System.Windows.Forms.Label();
            this.lblJoinedOn = new System.Windows.Forms.Label();
            this.lblUserName = new System.Windows.Forms.Label();
            this.lblFullName = new System.Windows.Forms.Label();
            this.panelNumbers = new System.Windows.Forms.Panel();
            this.lblFollowers = new System.Windows.Forms.Label();
            this.lblFollowersNumber = new System.Windows.Forms.LinkLabel();
            this.lblFollowing = new System.Windows.Forms.Label();
            this.lblTweets = new System.Windows.Forms.Label();
            this.panelDescription = new System.Windows.Forms.Panel();
            this.panelNumbers2 = new System.Windows.Forms.Panel();
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuView = new System.Windows.Forms.MenuItem();
            this.menuBasic = new System.Windows.Forms.MenuItem();
            this.menuFollow = new System.Windows.Forms.MenuItem();
            this.menuTweet = new System.Windows.Forms.MenuItem();
            this.menuDescription = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuClose = new System.Windows.Forms.MenuItem();
            this.panelJoinedLocation = new System.Windows.Forms.Panel();
            this.menuLocationJoined = new System.Windows.Forms.MenuItem();
            this.panelBasicInfo.SuspendLayout();
            this.panelNumbers.SuspendLayout();
            this.panelDescription.SuspendLayout();
            this.panelNumbers2.SuspendLayout();
            this.panelJoinedLocation.SuspendLayout();
            this.SuspendLayout();
            // 
            // avatarBox
            // 
            this.avatarBox.Location = new System.Drawing.Point(0, 0);
            this.avatarBox.Name = "avatarBox";
            this.avatarBox.Size = new System.Drawing.Size(83, 68);
            this.avatarBox.Click += new System.EventHandler(this.avatarBox_Click);
            // 
            // lblDescription
            // 
            this.lblDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDescription.Location = new System.Drawing.Point(0, 0);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(240, 217);
            this.lblDescription.Text = resources.GetString("lblDescription.Text");
            // 
            // lblFavorites
            // 
            this.lblFavorites.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFavorites.Location = new System.Drawing.Point(0, 42);
            this.lblFavorites.Name = "lblFavorites";
            this.lblFavorites.Size = new System.Drawing.Size(240, 21);
            this.lblFavorites.Text = "Favorites: ";
            // 
            // llblTweets
            // 
            this.llblTweets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.llblTweets.Location = new System.Drawing.Point(0, 21);
            this.llblTweets.Name = "llblTweets";
            this.llblTweets.Size = new System.Drawing.Size(240, 21);
            this.llblTweets.TabIndex = 21;
            this.llblTweets.Text = "10";
            this.llblTweets.Click += new System.EventHandler(this.llblTweets_Click);
            // 
            // lblFollowingNumber
            // 
            this.lblFollowingNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFollowingNumber.Location = new System.Drawing.Point(3, 21);
            this.lblFollowingNumber.Name = "lblFollowingNumber";
            this.lblFollowingNumber.Size = new System.Drawing.Size(225, 21);
            this.lblFollowingNumber.TabIndex = 22;
            this.lblFollowingNumber.Text = "10";
            // 
            // llblFavorites
            // 
            this.llblFavorites.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.llblFavorites.Location = new System.Drawing.Point(0, 63);
            this.llblFavorites.Name = "llblFavorites";
            this.llblFavorites.Size = new System.Drawing.Size(240, 21);
            this.llblFavorites.TabIndex = 23;
            this.llblFavorites.Text = "10";
            this.llblFavorites.Click += new System.EventHandler(this.llblFavorites_Click);
            // 
            // lblJoinedOnDate
            // 
            this.lblJoinedOnDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblJoinedOnDate.Location = new System.Drawing.Point(3, 63);
            this.lblJoinedOnDate.Name = "lblJoinedOnDate";
            this.lblJoinedOnDate.Size = new System.Drawing.Size(234, 21);
            this.lblJoinedOnDate.Text = "01/01/2009";
            // 
            // panelBasicInfo
            // 
            this.panelBasicInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBasicInfo.AutoScroll = true;
            this.panelBasicInfo.Controls.Add(this.lblURL);
            this.panelBasicInfo.Controls.Add(this.lblUserName);
            this.panelBasicInfo.Controls.Add(this.lblFullName);
            this.panelBasicInfo.Location = new System.Drawing.Point(0, 74);
            this.panelBasicInfo.Name = "panelBasicInfo";
            this.panelBasicInfo.Size = new System.Drawing.Size(240, 217);
            this.panelBasicInfo.Visible = false;
            // 
            // lblURL
            // 
            this.lblURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblURL.Location = new System.Drawing.Point(3, 41);
            this.lblURL.Name = "lblURL";
            this.lblURL.Size = new System.Drawing.Size(225, 20);
            this.lblURL.TabIndex = 18;
            this.lblURL.Text = "URL";
            this.lblURL.Click += new System.EventHandler(this.lblURL_Click);
            // 
            // lblPosition
            // 
            this.lblPosition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPosition.Location = new System.Drawing.Point(3, 22);
            this.lblPosition.Name = "lblPosition";
            this.lblPosition.Size = new System.Drawing.Size(234, 20);
            this.lblPosition.TabIndex = 17;
            this.lblPosition.Text = "Somewhere";
            this.lblPosition.Click += new System.EventHandler(this.lblPosition_Click);
            // 
            // lblLocation
            // 
            this.lblLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLocation.Location = new System.Drawing.Point(3, 2);
            this.lblLocation.Name = "lblLocation";
            this.lblLocation.Size = new System.Drawing.Size(234, 20);
            this.lblLocation.Text = "Location: ";
            // 
            // lblJoinedOn
            // 
            this.lblJoinedOn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblJoinedOn.Location = new System.Drawing.Point(3, 43);
            this.lblJoinedOn.Name = "lblJoinedOn";
            this.lblJoinedOn.Size = new System.Drawing.Size(234, 20);
            this.lblJoinedOn.Text = "Joined on: ";
            // 
            // lblUserName
            // 
            this.lblUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblUserName.Location = new System.Drawing.Point(3, 21);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(234, 20);
            this.lblUserName.Text = "@jakes";
            // 
            // lblFullName
            // 
            this.lblFullName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFullName.Location = new System.Drawing.Point(3, 1);
            this.lblFullName.Name = "lblFullName";
            this.lblFullName.Size = new System.Drawing.Size(234, 20);
            this.lblFullName.Text = "Jake Stevenson";
            // 
            // panelNumbers
            // 
            this.panelNumbers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelNumbers.Controls.Add(this.lblFollowers);
            this.panelNumbers.Controls.Add(this.lblFollowersNumber);
            this.panelNumbers.Controls.Add(this.lblFollowingNumber);
            this.panelNumbers.Controls.Add(this.lblFollowing);
            this.panelNumbers.Location = new System.Drawing.Point(0, 74);
            this.panelNumbers.Name = "panelNumbers";
            this.panelNumbers.Size = new System.Drawing.Size(240, 194);
            this.panelNumbers.Visible = false;
            // 
            // lblFollowers
            // 
            this.lblFollowers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFollowers.Location = new System.Drawing.Point(3, 42);
            this.lblFollowers.Name = "lblFollowers";
            this.lblFollowers.Size = new System.Drawing.Size(225, 21);
            this.lblFollowers.Text = "Followers: ";
            // 
            // lblFollowersNumber
            // 
            this.lblFollowersNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFollowersNumber.Location = new System.Drawing.Point(3, 63);
            this.lblFollowersNumber.Name = "lblFollowersNumber";
            this.lblFollowersNumber.Size = new System.Drawing.Size(225, 21);
            this.lblFollowersNumber.TabIndex = 24;
            this.lblFollowersNumber.Text = "10";
            // 
            // lblFollowing
            // 
            this.lblFollowing.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFollowing.Location = new System.Drawing.Point(3, 0);
            this.lblFollowing.Name = "lblFollowing";
            this.lblFollowing.Size = new System.Drawing.Size(225, 21);
            this.lblFollowing.Text = "Following: ";
            // 
            // lblTweets
            // 
            this.lblTweets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTweets.Location = new System.Drawing.Point(0, 0);
            this.lblTweets.Name = "lblTweets";
            this.lblTweets.Size = new System.Drawing.Size(240, 21);
            this.lblTweets.Text = "Tweets: ";
            // 
            // panelDescription
            // 
            this.panelDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelDescription.Controls.Add(this.lblDescription);
            this.panelDescription.Location = new System.Drawing.Point(0, 71);
            this.panelDescription.Name = "panelDescription";
            this.panelDescription.Size = new System.Drawing.Size(240, 217);
            // 
            // panelNumbers2
            // 
            this.panelNumbers2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelNumbers2.Controls.Add(this.llblFavorites);
            this.panelNumbers2.Controls.Add(this.lblFavorites);
            this.panelNumbers2.Controls.Add(this.lblTweets);
            this.panelNumbers2.Controls.Add(this.llblTweets);
            this.panelNumbers2.Location = new System.Drawing.Point(0, 74);
            this.panelNumbers2.Name = "panelNumbers2";
            this.panelNumbers2.Size = new System.Drawing.Size(243, 191);
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.menuView);
            this.mainMenu1.MenuItems.Add(this.menuClose);
            // 
            // menuView
            // 
            this.menuView.MenuItems.Add(this.menuBasic);
            this.menuView.MenuItems.Add(this.menuLocationJoined);
            this.menuView.MenuItems.Add(this.menuFollow);
            this.menuView.MenuItems.Add(this.menuTweet);
            this.menuView.MenuItems.Add(this.menuDescription);
            this.menuView.MenuItems.Add(this.menuItem1);
            this.menuView.Text = "View";
            // 
            // menuBasic
            // 
            this.menuBasic.Text = "Basic";
            this.menuBasic.Click += new System.EventHandler(this.menuBasic_Click);
            // 
            // menuFollow
            // 
            this.menuFollow.Text = "Follow Info";
            this.menuFollow.Click += new System.EventHandler(this.menuFollow_Click);
            // 
            // menuTweet
            // 
            this.menuTweet.Text = "Tweet Info";
            this.menuTweet.Click += new System.EventHandler(this.menuTweet_Click);
            // 
            // menuDescription
            // 
            this.menuDescription.Text = "Description";
            this.menuDescription.Click += new System.EventHandler(this.menuDescription_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Text = "Full Avatar";
            this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
            // 
            // menuClose
            // 
            this.menuClose.Text = "Close";
            this.menuClose.Click += new System.EventHandler(this.menuClose_Click);
            // 
            // panelJoinedLocation
            // 
            this.panelJoinedLocation.Controls.Add(this.lblLocation);
            this.panelJoinedLocation.Controls.Add(this.lblPosition);
            this.panelJoinedLocation.Controls.Add(this.lblJoinedOnDate);
            this.panelJoinedLocation.Controls.Add(this.lblJoinedOn);
            this.panelJoinedLocation.Location = new System.Drawing.Point(0, 74);
            this.panelJoinedLocation.Name = "panelJoinedLocation";
            this.panelJoinedLocation.Size = new System.Drawing.Size(240, 194);
            // 
            // menuLocationJoined
            // 
            this.menuLocationJoined.Text = "Location/Joined";
            this.menuLocationJoined.Click += new System.EventHandler(this.menuLocationJoined_Click);
            // 
            // ProfileViewSmartPhone
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.panelJoinedLocation);
            this.Controls.Add(this.panelNumbers);
            this.Controls.Add(this.panelBasicInfo);
            this.Controls.Add(this.panelDescription);
            this.Controls.Add(this.panelNumbers2);
            this.Controls.Add(this.avatarBox);
            this.Menu = this.mainMenu1;
            this.Name = "ProfileViewSmartPhone";
            this.Text = "ProfileView";
            this.panelBasicInfo.ResumeLayout(false);
            this.panelNumbers.ResumeLayout(false);
            this.panelDescription.ResumeLayout(false);
            this.panelNumbers2.ResumeLayout(false);
            this.panelJoinedLocation.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox avatarBox;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblFavorites;
        private System.Windows.Forms.LinkLabel llblTweets;
        private System.Windows.Forms.LinkLabel lblFollowingNumber;
        private System.Windows.Forms.LinkLabel llblFavorites;
        private System.Windows.Forms.Label lblJoinedOnDate;
        private System.Windows.Forms.Panel panelBasicInfo;
        private System.Windows.Forms.LinkLabel lblURL;
        private System.Windows.Forms.LinkLabel lblPosition;
        private System.Windows.Forms.Label lblLocation;
        private System.Windows.Forms.Label lblJoinedOn;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.Label lblFullName;
        private System.Windows.Forms.Panel panelNumbers;
        private System.Windows.Forms.Label lblFollowers;
        private System.Windows.Forms.LinkLabel lblFollowersNumber;
        private System.Windows.Forms.Label lblTweets;
        private System.Windows.Forms.Label lblFollowing;
        private System.Windows.Forms.Panel panelDescription;
        private System.Windows.Forms.Panel panelNumbers2;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuView;
        private System.Windows.Forms.MenuItem menuBasic;
        private System.Windows.Forms.MenuItem menuFollow;
        private System.Windows.Forms.MenuItem menuTweet;
        private System.Windows.Forms.MenuItem menuDescription;
        private System.Windows.Forms.MenuItem menuClose;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuLocationJoined;
        private System.Windows.Forms.Panel panelJoinedLocation;
    }
}