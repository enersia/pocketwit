using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using PockeTwit.OtherServices;
using PockeTwit.OtherServices.TextShrinkers;

namespace FingerUI
{
    public partial class FullScreenTweet : UserControl
    {
        const string HTMLTagPattern = "<.*?>";

        public PockeTwit.Library.status Status;
        public Yedda.Twitter Conn;
        private bool _visible;
        public new bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                _visible = value;
                base.Visible = value;
                if (_visible)
                {
                    PockeTwit.ThrottledArtGrabber.NewArtWasDownloaded += ThrottledArtGrabberNewArtWasDownloaded;
                }
                else
                {
                    PockeTwit.ThrottledArtGrabber.NewArtWasDownloaded -= ThrottledArtGrabberNewArtWasDownloaded;
                    if (avatarBox.Image != null)
                    {
                        avatarBox.Image.Dispose();
                    }
                }
            }
        }
        public FullScreenTweet()
        {
            InitializeComponent();
            if (PockeTwit.DetectDevice.DeviceType == PockeTwit.DeviceType.Standard)
            {
                lnkDismiss.Visible = false;
            }
            _fontSize = lblText.Font.Size;
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);
            avatarBox.Width = ClientSettings.SmallArtSize;
            avatarBox.Height = ClientSettings.SmallArtSize;
        }

        public void ResetRendering()
        {
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);
        }

        private delegate void DelUpdateArt(string argument);
        void ThrottledArtGrabberNewArtWasDownloaded(string argument)
        {
            
            if (Status != null)
            {
                if (argument == Status.user.profile_image_url)
                {
                    if (InvokeRequired)
                    {
                        var d = new DelUpdateArt(ThrottledArtGrabberNewArtWasDownloaded);
                        Invoke(d, argument);
                    }
                    else
                    {
                        try
                        {
                            avatarBox.Image = PockeTwit.ThrottledArtGrabber.GetArt(Status.user.profile_image_url);
                        }
                        catch(ObjectDisposedException){}
                    }
                }
            }
        }

        private float _fontSize;
        public float FontSize
        {
            get { return _fontSize; }
            set
            {
                if (value > 5)
                {
                    _fontSize = value;
                    using (var textFont = new Font(FontFamily.GenericSansSerif, value, FontStyle.Regular))
                    {
                        lblText.Font = textFont;
                    }
                }
            }
        }
        public void Render()
        {
            if (Status != null)
            {
                Cursor.Current = Cursors.WaitCursor;

                PockeTwit.Library.status stat = Status;

                //if (Status.retweeted_status != null)
                //{
                //    //why won't this work? does retweeted_status need to 
                //    //be saved in the db?
                //    stat = Status.retweeted_status;
                //}

                //if it's a new retweet, make an API call to get orginal tweet
                //so we can show all 140 chars of the tweet
                if (Status.TypeofMessage == PockeTwit.Library.StatusTypes.Retweet)
                {
                    foreach (Yedda.Twitter.Account a in ClientSettings.AccountsList)
                    {
                        if (a.Equals(Status.Account))
                        {
                            Conn = new Yedda.Twitter();
                            Conn.AccountInfo.ServerURL = a.ServerURL;
                            Conn.AccountInfo.UserName = a.UserName;
                            Conn.AccountInfo.Password = a.Password;
                            Conn.AccountInfo.Enabled = a.Enabled;

                            //Get the tweet again from Twitter
                            stat = PockeTwit.Library.status.DeserializeSingle(Conn.ShowSingleStatus(stat.id), Conn.AccountInfo);
                            
                            //Set the tweet to the original tweet
                            stat = stat.retweeted_status;
                            break;
                        }
                    }
                }

                avatarBox.Image = PockeTwit.ThrottledArtGrabber.GetArt(stat.user.profile_image_url);
                lblUserName.Text = stat.user.screen_name;
                lblTime.Text = stat.TimeStamp;
                if (string.IsNullOrEmpty(stat.source))
                {
                    lblSource.Text = "";
                }
                else
                {

                    lblSource.Text = "from " + StripHTML(System.Web.HttpUtility.HtmlDecode(stat.source));
                }
                string fullText;
                if (ShortText.IsShortTextURL(stat.text))
                {
                    string[] splitup = stat.text.Split(new[] { ' ' });
                    fullText = ShortText.GetFullText(splitup[splitup.Length - 1]);
                }
                else
                {
                    fullText = stat.text;
                }
                if (ClientSettings.AutoTranslate)
                {
                    fullText = GoogleTranslate.GetTranslation(fullText);
                }
                lblText.Text = System.Web.HttpUtility.HtmlDecode(fullText).Replace("&", "&&");
                Cursor.Current = Cursors.Default;
            }
        }

        private void lnkDismiss_Click(object sender, EventArgs e)
        {
            Visible = false;
        }
        static string StripHTML(string inputString)
        {
            if (string.IsNullOrEmpty(inputString)) { return null; }
            return Regex.Replace
              (inputString, HTMLTagPattern, string.Empty);
        }
    }
}
