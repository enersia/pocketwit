using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using PockeTwit.OtherServices;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.InteropServices;
using Timer=System.Threading.Timer;

namespace PockeTwit
{
    public partial class ProfileView : Form, IProfileViewer
    {
        private PockeTwit.Library.User _User;

        
        private PictureBox pb;
        private Panel p;
        private Button b = new Button();

        public ProfileAction selectedAction { get; set; }
        public String selectedUser { get; set; }

        public ProfileView(PockeTwit.Library.User User)
        {
            
            if (User.needsFetching)
            {
                User = FetchTheUser(User);
            }
            _User = User;
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Themes.FormColors.SetColors(pContent);
            pContent.BackColor = ClientSettings.BackColor;
            pViewer.BackColor = ClientSettings.BackColor;
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
                llblFollowers.Text = User.followers_count;
            }
            if (!string.IsNullOrEmpty(User.friends_count))
            {
                llblFollowing.Text = User.friends_count;
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
                lblJoinedOn.Text = d.ToShortDateString();
            }

            if (!string.IsNullOrEmpty(User.verified) && User.verified == "true")
            {
                //TODO
            }


            PockeTwit.ThrottledArtGrabber.NewArtWasDownloaded += new ThrottledArtGrabber.ArtIsReady(ThrottledArtGrabber_NewArtWasDownloaded);

            b.Text = "Close";
            //b.Anchor = AnchorStyles.Bottom;
            b.Click += new EventHandler(btnClose_Click);
            if (!ClientSettings.IsMaximized)
            {
                b.Location = new Point((Screen.PrimaryScreen.Bounds.Width / 2) - (b.Width / 2), Screen.PrimaryScreen.WorkingArea.Height - b.Height);
            }
            else
            {
                b.Location = new Point((Screen.PrimaryScreen.Bounds.Width / 2) - (b.Width / 2), Screen.PrimaryScreen.Bounds.Height - b.Height);
            }
            this.Controls.Add(b);
            b.BringToFront();

            pContent.Size = new Size(pContent.Size.Width, lblDescription.Location.Y + lblDescription.Size.Height + 5);

            //TODO: landscape mode - close button position
        }

        private Library.User FetchTheUser(PockeTwit.Library.User User)
        {
            return Library.User.FromId(User.screen_name, ClientSettings.AccountsList[0]);
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
            selectedUser = _User.screen_name;

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
                    b.BringToFront();
                    Cursor.Current = Cursors.Default;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    b.Hide();
                    pb.Hide();
                }
            }
            
            b.Location = new Point((Screen.PrimaryScreen.Bounds.Width / 2) - (b.Width / 2), Screen.PrimaryScreen.Bounds.Height - b.Height);
        }

        private void pb_Click(object sender, EventArgs e)
        {
            pb.Hide();
            p.Hide();

            if (!ClientSettings.IsMaximized)
            {
                b.Location = new Point((Screen.PrimaryScreen.Bounds.Width / 2) - (b.Width / 2), Screen.PrimaryScreen.WorkingArea.Height - b.Height);
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                b.Location = new Point((Screen.PrimaryScreen.Bounds.Width / 2) - (b.Width / 2), Screen.PrimaryScreen.Bounds.Height - b.Height);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            closeForm();
        }

        private void llblFavorites_Click(object sender, EventArgs e)
        {
            selectedAction = ProfileAction.Favorites;
            selectedUser = _User.screen_name;

            closeForm();
        }

        private void downArrow_Click(object sender, EventArgs e)
        {
            if (pContent.Top > -(pContent.Size.Height))
            {
                pContent.Top -= 40;
            }
        }

        private void upArrow_Click(object sender, EventArgs e)
        {
            if (pContent.Top < 0)
            {
                pContent.Top += 40;
            }
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
            timeranimate.Enabled = false;
            this.Close();
        }

        private int mouseDownOn = 0;
        
        private void pContent_MouseMove(object sender, MouseEventArgs e)
        {
            int mouseMoved = e.Y - mouseDownOn;
            pContent.Top = pContent.Top + mouseMoved;
        }

        private void pContent_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDownOn = e.Y;
        }

        private void pContent_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDownOn = 0;
            timeranimate.Enabled = true;
        }

        private void timeranimate_Tick(object sender, EventArgs e)
        {
            int offSet = pContent.Top;
            int step = offSet/4;
            if (step < 5 && step > 0) {
                step = 5; 
            }
            if(step >-5 && step <0)
            {
                step = -5;
            }
            if (step == 0) 
            {
                timeranimate.Enabled = false;
                return;
            }
            if(offSet>0)
            {
                pContent.Top = pContent.Top - step;
                if(pContent.Top<0)
                {
                    pContent.Top = 0;
                    timeranimate.Enabled = false;
                }
            }
            else
            {
                pContent.Top = pContent.Top - step;
                if (pContent.Top > 0)
                {
                    pContent.Top = 0;
                    timeranimate.Enabled = false;
                }
            }
        }

    }
}