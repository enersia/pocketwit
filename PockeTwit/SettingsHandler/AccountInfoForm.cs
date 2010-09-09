using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OAuth;
using System.Threading;

namespace PockeTwit
{
    public partial class AccountInfoForm : Form
    {
        private Yedda.Twitter.Account _AccountInfo = new Yedda.Twitter.Account();
        private string RequestToken = string.Empty;
        private string RequestTokenSecret = string.Empty;
        private string urlToLaunch = string.Empty;

        public Yedda.Twitter.Account AccountInfo 
        {
            get
            {
                return _AccountInfo;
            } 
            set
            {
                _AccountInfo = value;
                PopulateForm();
            }
        }

        private void SetupProfessional()
        {
            this.copyPasteMenu = new System.Windows.Forms.ContextMenu();
            this.PasteItem = new System.Windows.Forms.MenuItem();
            PasteItem.Text = "Paste";
            copyPasteMenu.MenuItems.Add(PasteItem);
            PasteItem.Click += new System.EventHandler(PasteItem_Click);
        }
        private void SetupStandard()
        {
            Microsoft.WindowsCE.Forms.InputModeEditor.SetInputMode(txtUserName, Microsoft.WindowsCE.Forms.InputMode.AlphaCurrent);
            Microsoft.WindowsCE.Forms.InputModeEditor.SetInputMode(txtPassword, Microsoft.WindowsCE.Forms.InputMode.AlphaCurrent);

        }
        public AccountInfoForm()
        {
            InitializeComponent();
            
            if (DetectDevice.DeviceType == DeviceType.Professional)
            {
                SetupProfessional();
            }
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            _AccountInfo.Enabled = true;
            FillServerList();
        }

        public AccountInfoForm(Yedda.Twitter.Account Account)
        {
            _AccountInfo = Account;
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);

            FillServerList();
            PopulateForm();
        }

        private void PopulateForm()
        {
            txtUserName.Text = _AccountInfo.UserName;
            txtPassword.Text = _AccountInfo.Password;
            cmbServers.SelectedItem = _AccountInfo.ServerURL.Name;
            chkDefault.Checked = _AccountInfo == ClientSettings.DefaultAccount;
        }
        private void FillServerList()
        {
            foreach (string ServerName in Yedda.Servers.ServerList.Keys)
            {
                cmbServers.Items.Add(ServerName);
            }
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void menuItem2_Click(object sender, EventArgs e)
        {
            if (cmbServers.SelectedItem == null)
            {
                return;
            }
            Cursor.Current = Cursors.WaitCursor;
            Application.DoEvents();
            lblError.Visible = false;
            _AccountInfo.UserName = txtUserName.Text;
            _AccountInfo.Password = txtPassword.Text;
            _AccountInfo.ServerURL = Yedda.Servers.ServerList[(string)cmbServers.SelectedItem];
            //_AccountInfo.Enabled = (_AccountInfo.ServerURL.ServerType != Yedda.Twitter.TwitterServer.pingfm && _AccountInfo.ServerURL.ServerType != Yedda.Twitter.TwitterServer.brightkite);
            _AccountInfo.Enabled = true;
            _AccountInfo.IsDefault = chkDefault.Checked;
            Yedda.Twitter T = Yedda.Servers.CreateConnection(_AccountInfo);
            Cursor.Current = Cursors.Default;
            if ((string)cmbServers.SelectedItem == "twitter")
            {
                OAuthAuthorizer authorizer = new OAuthAuthorizer();

                if (string.IsNullOrEmpty(TbPin.Text))
                {
                    lblError.Text = "Please enter pin provided by twitter.";
                    lblError.Visible = true;
                    return;
                }

                //access
                authorizer.AuthorizationToken = RequestToken;
                authorizer.AuthorizationVerifier = TbPin.Text;
                authorizer.AcquireAccessToken();

                _AccountInfo.OAuth_token = authorizer.AccessToken;
                _AccountInfo.OAuth_token_secret = authorizer.AccessTokenSecret;

                if (string.IsNullOrEmpty(_AccountInfo.OAuth_token))
                {
                    lblError.Text = "Unable to get access token from Twitter.";
                    lblError.Visible = true;
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(txtUserName.Text)) { return; }
                if (!T.Verify())
                {
                    lblError.Text = "Invalid credentials or network unavailable.";
                    lblError.Visible = true;
                    return;
                }
            }

            this.DialogResult = DialogResult.OK;
        }

        private void cmbServers_SelectedIndexChanged(object sender, EventArgs e)
        {
            string server = (string)cmbServers.SelectedItem;
            if (server == "ping.fm")
            {
                txtPassword.Visible = false;
                lblPassword.Visible = false;
                txtPassword.Text = ClientSettings.PingApi;
                lblUser.Text = "Ping.FM Key";
                linkLabel1.Visible = true;
                if (DetectDevice.DeviceType == DeviceType.Professional)
                {
                    txtUserName.ContextMenu = copyPasteMenu;
                }
                Ll_Twitter.Visible = false;
                lPin.Visible = false;
                TbPin.Visible = false;
                ImTwitter.Visible = false;
            }
            else if (server == "twitter")
            {
                ImTwitter.Visible = true;
                lPin.Visible = true;
                TbPin.Visible = true;
                Ll_Twitter.Visible = true;
                 
                txtPassword.Text = "";
                txtPassword.Visible = false;
                lblPassword.Visible = false;
                 
                linkLabel1.Visible = false;
                lblUser.Text = "User";
                if (DetectDevice.DeviceType == DeviceType.Professional)
                {
                     txtUserName.ContextMenu = null;
                }
            }
            else
            {
                txtPassword.Text = "";
                txtPassword.Visible = true;
                lblPassword.Visible = true;

                linkLabel1.Visible = false;
                Ll_Twitter.Visible = false;
                lblUser.Text = "User";
                if (DetectDevice.DeviceType == DeviceType.Professional)
                {
                    txtUserName.ContextMenu = null;
                }
                ImTwitter.Visible = false;
                lPin.Visible = false;
                TbPin.Visible = false;
            }
        }

        void PasteItem_Click(object sender, System.EventArgs e)
        {
            IDataObject iData = Clipboard.GetDataObject();
            if (iData.GetDataPresent(DataFormats.Text))
            {
                txtUserName.Text = (string)iData.GetData(DataFormats.Text);
            }
        }

        private void linkLabel1_Click(object sender, EventArgs e)
        {
            LaunchSite("http://ping.fm/m/key/");
        }

        private void LaunchSite(string URL)
        {
            System.Diagnostics.ProcessStartInfo pi = new System.Diagnostics.ProcessStartInfo();
            pi.FileName = URL;
            pi.UseShellExecute = true;
            System.Diagnostics.Process p = System.Diagnostics.Process.Start(pi);
        }

      
        private void LaunchBrowserLink()
        {
            System.Diagnostics.ProcessStartInfo pi = new System.Diagnostics.ProcessStartInfo();
            pi.FileName = urlToLaunch;
            pi.UseShellExecute = true;
            System.Diagnostics.Process p = System.Diagnostics.Process.Start(pi);
        }

        private void Ll_Twitter_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;
            
            OAuthAuthorizer authorizer = new OAuthAuthorizer();

            //request
            authorizer.AcquireRequestToken();
            RequestToken = authorizer.RequestToken;
            RequestTokenSecret = authorizer.RequestTokenSecret;

            if (string.IsNullOrEmpty(RequestToken))
            {
                Localization.LocalizedMessageBox.Show("Unable to retrieve token, try again later.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, null);
                return; //don't try the rest...
            }

            Localization.LocalizedMessageBox.Show("Your browser will be opened to authorise PockeTwit with Twitter. Please note the pincode on the webpage.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, null);

            //auth
            Uri url = new Uri(OAuthConfig.AuthorizeUrl + "?oauth_token=" + RequestToken);
            urlToLaunch = url.ToString();
            Thread t = new Thread(new ThreadStart(LaunchBrowserLink));
            t.Start();

        }

        private void TbPin_GotFocus(object sender, EventArgs e)
        {
            //inputPanel1.Enabled = true;
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {

        }
    }
}