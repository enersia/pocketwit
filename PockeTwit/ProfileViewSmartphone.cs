using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PockeTwit.OtherServices;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.InteropServices;

namespace PockeTwit
{
    public partial class ProfileViewSmartPhone : Form, IProfileViewer
    {
        private PockeTwit.Library.User _User;
        private Yedda.Twitter.Account _Account;

        private PictureBox pb;
        private Panel p;

        public ProfileAction selectedAction { get; set; }
        public String selectedUser { get; set; }

        public ProfileViewSmartPhone(PockeTwit.Library.User User, Yedda.Twitter.Account account)
        {
            
            if (User.needsFetching)
            {
                User = FetchTheUser(User, account);
            }
            _User = User;
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Themes.FormColors.SetColors(panelBasicInfo);
            PockeTwit.Themes.FormColors.SetColors(panelDescription);
            PockeTwit.Themes.FormColors.SetColors(panelNumbers);
            PockeTwit.Themes.FormColors.SetColors(panelNumbers2);
            PockeTwit.Themes.FormColors.SetColors(panelJoinedLocation);

            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            avatarBox.Width = ClientSettings.SmallArtSize;
            avatarBox.Height = ClientSettings.SmallArtSize;

            avatarBox.Image = PockeTwit.ThrottledArtGrabber.GetArt(User.profile_image_url);
            
            lblUserName.Text = User.screen_name;
            
            if (string.IsNullOrEmpty(User.name)) { lblFullName.Visible = false; }
            else
            {
                lblFullName.Text = User.name;
            }
            if (string.IsNullOrEmpty(User.location)) { lblPosition.Visible = false; }
            else
            {
                _User.location = User.location.Replace("iPhone: ", "");
                Coordinate c = new Coordinate();
                if (Coordinate.tryParse(_User.location, out c))
                {
                    lblPosition.Text = Geocode.GetAddress(_User.location);
                }
                else
                {
                    lblPosition.Text = _User.location;
                }
            }
            if (string.IsNullOrEmpty(User.description))
            {
                lblDescription.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("No description available.");
            }
            else
            {
                lblDescription.Text = User.description;
            }

            if (!string.IsNullOrEmpty(User.url))
            {
                lblURL.Text = User.url;
            }
            if (!string.IsNullOrEmpty(User.followers_count))
            {
                lblFollowersNumber.Text = User.followers_count;
            }
            if (!string.IsNullOrEmpty(User.friends_count))
            {
                lblFollowingNumber.Text = User.friends_count;
            } 
            if (!string.IsNullOrEmpty(User.statuses_count))
            {
                llblTweets.Text = User.statuses_count;
            }
            if (!string.IsNullOrEmpty(User.favourites_count))
            {
                llblFavorites.Text = User.favourites_count;
            }

            if (!string.IsNullOrEmpty(User.created_at))
            {
                DateTime d = getDate(User.created_at);
                lblJoinedOnDate.Text = d.ToShortDateString();
            }

            if (!string.IsNullOrEmpty(User.verified) && User.verified == "true")
            {
                //TODO
            }


            PockeTwit.ThrottledArtGrabber.NewArtWasDownloaded += new ThrottledArtGrabber.ArtIsReady(ThrottledArtGrabber_NewArtWasDownloaded);

            SwitchToPanel(panelBasicInfo);
         
        }

        private Library.User FetchTheUser(PockeTwit.Library.User User, Yedda.Twitter.Account account)
        {
            return Library.User.FromId(account.ReturnUserID(User), account);
        }

        private delegate void delUpdateArt(string Argument);

        void ThrottledArtGrabber_NewArtWasDownloaded(string Argument)
        {
            if (InvokeRequired)
            {
                delUpdateArt d = new delUpdateArt(ThrottledArtGrabber_NewArtWasDownloaded);
                this.Invoke(d, Argument);
            }
            else
            {
                avatarBox.Image = PockeTwit.ThrottledArtGrabber.GetArt(_User.profile_image_url);
            }
        }

        private void lblPosition_Click(object sender, EventArgs e)
        {
            using (ProfileMap m = new ProfileMap())
            {
                m.Owner = this;
                m.Users = new List<PockeTwit.Library.User>(new Library.User[] { _User });
                m.ShowDialog();
                m.Dispose();
            }
        }

        private void lblURL_Click(object sender, EventArgs e)
        {
            String textClicked = lblURL.Text;

            System.Diagnostics.ProcessStartInfo pi = new System.Diagnostics.ProcessStartInfo();
            if (ClientSettings.UseSkweezer)
            {
                pi.FileName = Skweezer.GetSkweezerURL(textClicked);
            }
            else
            {
                pi.FileName = textClicked;
            }
            try
            {
                pi.UseShellExecute = true;
                System.Diagnostics.Process p = System.Diagnostics.Process.Start(pi);
            }
            catch
            {
                PockeTwit.Localization.LocalizedMessageBox.Show("There is no default web browser defined for the OS.");
            }
        }

        private void llblTweets_Click(object sender, EventArgs e)
        {
            selectedAction = ProfileAction.UserTimeline;
            selectedUser = _Account.ReturnUserID(_User);

            closeForm();
        }

        private void avatarBox_Click(object sender, EventArgs e)
        {
            if (pb != null)
            {
                this.WindowState = FormWindowState.Maximized;
                p.Show();
                pb.Show();
            }
            else
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    p = new Panel();
                    p.Location = new Point(0, 0);
                    
                    p.BackColor = Color.Black;
                    

                    pb = new PictureBox();
                    string url = _User.profile_image_url.Replace("_normal", "");

                    var request = WebRequestFactory.CreateHttpRequest(url);
                    var httpResponse = (HttpWebResponse)request.GetResponse();
                    Stream stream = httpResponse.GetResponseStream();

                    Image i = new Bitmap(stream);
                    pb.Image = i;
                    pb.SizeMode = PictureBoxSizeMode.StretchImage;

                    this.WindowState = FormWindowState.Maximized;

                    pb.Size = getImageSize(i.Size, Screen.PrimaryScreen.Bounds.Size);

                    pb.Location = new Point((Screen.PrimaryScreen.Bounds.Width / 2) - (pb.Size.Width / 2), (Screen.PrimaryScreen.Bounds.Height / 2) - (pb.Size.Height / 2));
                    
                    pb.Click += new EventHandler(pb_Click);
                    p.Click += new EventHandler(pb_Click);

                    p.Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                    p.Visible = true;
                    this.Controls.Add(p);
                    p.BringToFront();

                    this.Controls.Add(pb);
                    
                    pb.BringToFront();
                    Cursor.Current = Cursors.Default;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    pb.Hide();
                }
            }
            
        }

        private void pb_Click(object sender, EventArgs e)
        {
            pb.Hide();
            p.Hide();

            if (!ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
        }

        

        private void llblFavorites_Click(object sender, EventArgs e)
        {
            selectedAction = ProfileAction.Favorites;
            selectedUser = _Account.ReturnUserID(_User);

            closeForm();
        }

        private DateTime getDate(string date)
        {
            //code borrowed from:
            //http://blogs.microsoft.co.il/blogs/bursteg/archive/2008/11/15/twitter-api-from-c-getting-a-user-s-time-line.aspx

            string dayOfWeek = date.Substring(0, 3).Trim();

            string month = date.Substring(4, 3).Trim();

            string dayInMonth = date.Substring(8, 2).Trim();

            string time = date.Substring(11, 9).Trim();

            string offset = date.Substring(20, 5).Trim();

            string year = date.Substring(25, 5).Trim();

            string dateTime = string.Format("{0}-{1}-{2} {3}", dayInMonth, month, year, time);

            DateTime ret = DateTime.Parse(dateTime);

            return ret;

        }

        private Size getImageSize(Size imageSize, Size screenSize)
        {
            Console.WriteLine("Image: " + imageSize.Width + " x " + imageSize.Height);
            Console.WriteLine("Screen: " + screenSize.Width + " x " + screenSize.Height);

            //code borrowed from:
            //http://snippets.dzone.com/posts/show/4336

            int maxHeight = screenSize.Height;

            if (imageSize.Width <= screenSize.Width)
            {
                screenSize.Width = imageSize.Width;
            }

            screenSize.Height = imageSize.Height * screenSize.Width / imageSize.Width;

            if (screenSize.Height > maxHeight)
            {
                // Resize with height instead
                imageSize.Width = imageSize.Width * maxHeight / imageSize.Height;
                screenSize.Height = maxHeight;
            }

            Console.WriteLine("Scaled: " + screenSize.Width + " x " + screenSize.Height);

            return screenSize;
        }

        private void closeForm()
        {
            PockeTwit.ThrottledArtGrabber.NewArtWasDownloaded -= new ThrottledArtGrabber.ArtIsReady(ThrottledArtGrabber_NewArtWasDownloaded);
            this.Close();
        }

        private void SwitchToPanel(Panel panel)
        {
            panelBasicInfo.Visible = false;
            panelDescription.Visible = false;
            panelNumbers.Visible = false;
            panelNumbers2.Visible = false;
            panelJoinedLocation.Visible = false;
            panel.Visible = true;
        }

        private void menuBasic_Click(object sender, EventArgs e)
        {
            SwitchToPanel(panelBasicInfo);
        }

        private void menuFollow_Click(object sender, EventArgs e)
        {
            SwitchToPanel(panelNumbers);
        }

        private void menuTweet_Click(object sender, EventArgs e)
        {
            SwitchToPanel(panelNumbers2);
        }

        private void menuDescription_Click(object sender, EventArgs e)
        {
            SwitchToPanel(panelDescription);
        }

        private void menuClose_Click(object sender, EventArgs e)
        {
            closeForm();
        }

        private bool LargeAvatar;
        private void menuItem1_Click(object sender, EventArgs e)
        {
            if (LargeAvatar)
            {
                pb_Click(null,null);
            }
            else
            {
                avatarBox_Click(null, null);
            }
            LargeAvatar = !LargeAvatar;
        }

        private void menuLocationJoined_Click(object sender, EventArgs e)
        {
            SwitchToPanel(panelJoinedLocation);
        }

    }
}