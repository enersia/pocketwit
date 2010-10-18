using System.Windows.Forms;
namespace PockeTwit
{
    partial class PostUpdate
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu1;

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
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuCancel = new System.Windows.Forms.MenuItem();
            this.menuSubmit = new System.Windows.Forms.MenuItem();
            this.pnlSipSize = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtStatusUpdate = new System.Windows.Forms.TextBox();
            this.pnlStatus = new System.Windows.Forms.Panel();
            this.cmbPlaces = new System.Windows.Forms.ComboBox();
            this.lblGPS = new System.Windows.Forms.Label();
            this.lblCharsLeft = new System.Windows.Forms.Label();
            this.pictureLocation = new System.Windows.Forms.PictureBox();
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.picInsertVideo = new System.Windows.Forms.PictureBox();
            this.picAttachments = new System.Windows.Forms.PictureBox();
            this.picInsertGPSLink = new System.Windows.Forms.PictureBox();
            this.picAddressBook = new System.Windows.Forms.PictureBox();
            this.pictureFromCamers = new System.Windows.Forms.PictureBox();
            this.pictureFromStorage = new System.Windows.Forms.PictureBox();
            this.pictureURL = new System.Windows.Forms.PictureBox();
            this.pnlAccounts = new System.Windows.Forms.Panel();
            this.lblFromAccount = new System.Windows.Forms.Label();
            this.cmbAccount = new System.Windows.Forms.ComboBox();
            this.userListControl1 = new PockeTwit.userListControl();
            this.pnlSipSize.SuspendLayout();
            this.panel1.SuspendLayout();
            this.pnlStatus.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.pnlAccounts.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuCancel
            // 
            this.menuCancel.Text = "Cancel";
            this.menuCancel.Click += new System.EventHandler(this.menuCancel_Click);
            // 
            // menuSubmit
            // 
            this.menuSubmit.Text = "Submit";
            this.menuSubmit.Click += new System.EventHandler(this.menuSubmit_Click);
            // 
            // pnlSipSize
            // 
            this.pnlSipSize.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlSipSize.Controls.Add(this.panel1);
            this.pnlSipSize.Controls.Add(this.pnlStatus);
            this.pnlSipSize.Controls.Add(this.pnlToolbar);
            this.pnlSipSize.Controls.Add(this.pnlAccounts);
            this.pnlSipSize.Location = new System.Drawing.Point(0, 0);
            this.pnlSipSize.Name = "pnlSipSize";
            this.pnlSipSize.Size = new System.Drawing.Size(240, 268);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.userListControl1);
            this.panel1.Controls.Add(this.txtStatusUpdate);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 66);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(240, 166);
            // 
            // txtStatusUpdate
            // 
            this.txtStatusUpdate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtStatusUpdate.Location = new System.Drawing.Point(0, 0);
            this.txtStatusUpdate.Multiline = true;
            this.txtStatusUpdate.Name = "txtStatusUpdate";
            this.txtStatusUpdate.Size = new System.Drawing.Size(240, 166);
            this.txtStatusUpdate.TabIndex = 2;
            this.txtStatusUpdate.TextChanged += new System.EventHandler(this.txtStatusUpdate_TextChanged);
            this.txtStatusUpdate.GotFocus += new System.EventHandler(this.txtStatusUpdate_GotFocus);
            // 
            // pnlStatus
            // 
            this.pnlStatus.Controls.Add(this.cmbPlaces);
            this.pnlStatus.Controls.Add(this.lblGPS);
            this.pnlStatus.Controls.Add(this.lblCharsLeft);
            this.pnlStatus.Controls.Add(this.pictureLocation);
            this.pnlStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStatus.Location = new System.Drawing.Point(0, 232);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Size = new System.Drawing.Size(240, 36);
            // 
            // cmbPlaces
            // 
            this.cmbPlaces.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbPlaces.Location = new System.Drawing.Point(40, 8);
            this.cmbPlaces.Name = "cmbPlaces";
            this.cmbPlaces.Size = new System.Drawing.Size(161, 22);
            this.cmbPlaces.TabIndex = 2;
            this.cmbPlaces.Visible = false;
            // 
            // lblGPS
            // 
            this.lblGPS.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGPS.Location = new System.Drawing.Point(44, 10);
            this.lblGPS.Name = "lblGPS";
            this.lblGPS.Size = new System.Drawing.Size(154, 20);
            this.lblGPS.Text = "No location set";
            // 
            // lblCharsLeft
            // 
            this.lblCharsLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCharsLeft.Location = new System.Drawing.Point(204, 10);
            this.lblCharsLeft.Name = "lblCharsLeft";
            this.lblCharsLeft.Size = new System.Drawing.Size(30, 22);
            this.lblCharsLeft.Text = "140";
            this.lblCharsLeft.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // pictureLocation
            // 
            this.pictureLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pictureLocation.Location = new System.Drawing.Point(4, 2);
            this.pictureLocation.Name = "pictureLocation";
            this.pictureLocation.Size = new System.Drawing.Size(32, 32);
            this.pictureLocation.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.Controls.Add(this.label1);
            this.pnlToolbar.Controls.Add(this.picInsertVideo);
            this.pnlToolbar.Controls.Add(this.picAttachments);
            this.pnlToolbar.Controls.Add(this.picInsertGPSLink);
            this.pnlToolbar.Controls.Add(this.picAddressBook);
            this.pnlToolbar.Controls.Add(this.pictureFromCamers);
            this.pnlToolbar.Controls.Add(this.pictureFromStorage);
            this.pnlToolbar.Controls.Add(this.pictureURL);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 30);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Size = new System.Drawing.Size(240, 36);
            this.pnlToolbar.Visible = false;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular);
            this.label1.Location = new System.Drawing.Point(181, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(11, 11);
            this.label1.Text = "8";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.label1.Visible = false;
            // 
            // picInsertVideo
            // 
            this.picInsertVideo.Location = new System.Drawing.Point(132, 2);
            this.picInsertVideo.Name = "picInsertVideo";
            this.picInsertVideo.Size = new System.Drawing.Size(32, 32);
            this.picInsertVideo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picInsertVideo.Visible = false;
            // 
            // picAttachments
            // 
            this.picAttachments.Location = new System.Drawing.Point(164, 2);
            this.picAttachments.Name = "picAttachments";
            this.picAttachments.Size = new System.Drawing.Size(32, 32);
            this.picAttachments.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picAttachments.Visible = false;
            // 
            // picInsertGPSLink
            // 
            this.picInsertGPSLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picInsertGPSLink.Location = new System.Drawing.Point(204, 2);
            this.picInsertGPSLink.Name = "picInsertGPSLink";
            this.picInsertGPSLink.Size = new System.Drawing.Size(32, 32);
            this.picInsertGPSLink.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picInsertGPSLink.Visible = false;
            // 
            // picAddressBook
            // 
            this.picAddressBook.Location = new System.Drawing.Point(36, 2);
            this.picAddressBook.Name = "picAddressBook";
            this.picAddressBook.Size = new System.Drawing.Size(32, 32);
            this.picAddressBook.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            // 
            // pictureFromCamers
            // 
            this.pictureFromCamers.Location = new System.Drawing.Point(100, 2);
            this.pictureFromCamers.Name = "pictureFromCamers";
            this.pictureFromCamers.Size = new System.Drawing.Size(32, 32);
            this.pictureFromCamers.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            // 
            // pictureFromStorage
            // 
            this.pictureFromStorage.Location = new System.Drawing.Point(68, 2);
            this.pictureFromStorage.Name = "pictureFromStorage";
            this.pictureFromStorage.Size = new System.Drawing.Size(32, 32);
            this.pictureFromStorage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            // 
            // pictureURL
            // 
            this.pictureURL.Location = new System.Drawing.Point(4, 2);
            this.pictureURL.Name = "pictureURL";
            this.pictureURL.Size = new System.Drawing.Size(32, 32);
            this.pictureURL.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            // 
            // pnlAccounts
            // 
            this.pnlAccounts.Controls.Add(this.lblFromAccount);
            this.pnlAccounts.Controls.Add(this.cmbAccount);
            this.pnlAccounts.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAccounts.Location = new System.Drawing.Point(0, 0);
            this.pnlAccounts.Name = "pnlAccounts";
            this.pnlAccounts.Size = new System.Drawing.Size(240, 30);
            this.pnlAccounts.Visible = false;
            // 
            // lblFromAccount
            // 
            this.lblFromAccount.Location = new System.Drawing.Point(4, 6);
            this.lblFromAccount.Name = "lblFromAccount";
            this.lblFromAccount.Size = new System.Drawing.Size(94, 20);
            this.lblFromAccount.Text = "From Account:";
            // 
            // cmbAccount
            // 
            this.cmbAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbAccount.Location = new System.Drawing.Point(104, 4);
            this.cmbAccount.Name = "cmbAccount";
            this.cmbAccount.Size = new System.Drawing.Size(133, 22);
            this.cmbAccount.TabIndex = 3;
            this.cmbAccount.SelectedIndexChanged += new System.EventHandler(this.cmbAccount_SelectedIndexChanged);
            // 
            // userListControl1
            // 
            this.userListControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.userListControl1.Location = new System.Drawing.Point(33, 6);
            this.userListControl1.Name = "userListControl1";
            this.userListControl1.Size = new System.Drawing.Size(175, 154);
            this.userListControl1.TabIndex = 32;
            this.userListControl1.Visible = false;
            this.userListControl1.GotFocus += new System.EventHandler(this.txtStatusUpdate_GotFocus);
            // 
            // PostUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.pnlSipSize);
            this.Menu = this.mainMenu1;
            this.Name = "PostUpdate";
            this.Text = "Post Update";
            this.Deactivate += new System.EventHandler(this.inputPanel_EnabledChanged);
            this.Closed += new System.EventHandler(this.PostUpdate_Closed);
            this.Activated += new System.EventHandler(this.inputPanel_EnabledChanged);
            this.Resize += new System.EventHandler(this.PostUpdate_Resize);
            this.pnlSipSize.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.pnlStatus.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.pnlAccounts.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuItem menuSubmit;
        private System.Windows.Forms.MenuItem menuCancel;
        private Panel pnlSipSize;
        private Panel pnlToolbar;
        private PictureBox picAddressBook;
        private PictureBox pictureFromCamers;
        private PictureBox pictureFromStorage;
        private PictureBox pictureURL;
        private Panel pnlAccounts;
        private Label lblFromAccount;
        private ComboBox cmbAccount;
        private Panel pnlStatus;
        private ComboBox cmbPlaces;
        private Label lblGPS;
        private Label lblCharsLeft;
        private PictureBox pictureLocation;
        private Panel panel1;
        private userListControl userListControl1;
        private TextBox txtStatusUpdate;
        private PictureBox picInsertGPSLink;
        private PictureBox picAttachments;
        private PictureBox picInsertVideo;
        private Label label1;
    }
}
