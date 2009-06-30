using System;

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using PockeTwit.OtherServices.TextShrinkers;
using PockeTwit.Themes;
using Yedda;
using PockeTwit.MediaServices;

namespace PockeTwit
{
    public partial class PostUpdate : Form
    {
        private System.Windows.Forms.ContextMenu copyPasteMenu;
        private System.Windows.Forms.MenuItem PasteItem;
        private System.Windows.Forms.MenuItem CopyItem;
		
        public string GPSLocation = null;
        private LocationManager LocationFinder = new LocationManager();
        private bool _StandAlone;
        private delegate void delUpdateText(string text);

        private IPictureService pictureService;
        private string uploadedPictureOrigin = string.Empty;
        private string uploadedPictureURL = string.Empty;
        private bool uploadingPicture = false;
        private bool pictureUsed = true;
        private bool localPictureEventsSet = false;
        private string picturePath = string.Empty;

        private int availableChars = 140; //should retrieve this from a blogservice.

        public delegate void delAddPicture(string ImageFile, PictureBox BoxToUpdate);
        public delegate void delUpdatePictureData(string pictureUrl, bool uploadingPicture);

        #region Properties
        private Yedda.Twitter.Account _AccountToSet = ClientSettings.DefaultAccount;
        public Yedda.Twitter.Account AccountToSet
        {
            get
            {
                return _AccountToSet;
            }
            set
            {
                _AccountToSet = value;
                cmbAccount.SelectedItem = _AccountToSet;
                Yedda.Twitter t = new Yedda.Twitter();
                t.AccountInfo = _AccountToSet;
                AllowTwitPic = t.AllowTwitPic;
            }
        }

        public bool AllowTwitPic
        {
            set
            {
                pictureFromCamers.Visible = value;
                pictureFromStorage.Visible = value;
            }
        }

        public string StatusText
        {
            get
            {
                return txtStatusUpdate.Text;
            }
            set
            {
                txtStatusUpdate.Text = value;
                txtStatusUpdate.SelectionStart = txtStatusUpdate.Text.Length;
            }
        }

        public string in_reply_to_status_id { get; set; }

        #endregion


        public PostUpdate(bool Standalone)
        {
            _StandAlone = Standalone;
            InitializeComponent();
            SetImages();
            if (ClientSettings.AutoCompleteAddressBook)
            {
                userListControl1.HookTextBoxKeyPress(txtStatusUpdate);
            }
            FormColors.SetColors(this);
            if (ClientSettings.IsMaximized)
            {
                WindowState = FormWindowState.Maximized;
            }

            lblCharsLeft.Text = "140";
            PopulateAccountList();
            if (DetectDevice.DeviceType == DeviceType.Standard)
            {
                SmartPhoneMenu();
            }
            else
            {
                SetupTouchScreen();
            }
            mainMenu1.MenuItems.Add(menuCancel);
            ResumeLayout(false);

            LocationFinder.LocationReady += new LocationManager.delLocationReady(l_LocationReady);

            txtStatusUpdate.Focus();
            userListControl1.ItemChosen += new userListControl.delItemChose(userListControl1_ItemChosen);
        }

        private void SetupTouchScreen()
        {
            mainMenu1.MenuItems.Add(menuSubmit);
            pictureFromCamers.Click += new EventHandler(pictureFromCamers_Click);
            pictureFromStorage.Click += new EventHandler(pictureFromStorage_Click);
            pictureURL.Click += new EventHandler(pictureURL_Click);
            pictureLocation.Click += new EventHandler(pictureLocation_Click);

            picAddressBook.Click += new EventHandler(picAddressBook_Click);

            copyPasteMenu = new System.Windows.Forms.ContextMenu();

            PasteItem = new System.Windows.Forms.MenuItem();
            PasteItem.Text = "粘贴";

            CopyItem = new MenuItem();
            CopyItem.Text = "复制";

            copyPasteMenu.MenuItems.Add(CopyItem);
            copyPasteMenu.MenuItems.Add(PasteItem);
            txtStatusUpdate.ContextMenu = copyPasteMenu;

            CopyItem.Click += new EventHandler(CopyItem_Click);
            PasteItem.Click += new EventHandler(PasteItem_Click);

        }

        void picAddressBook_Click(object sender, EventArgs e)
        {
            txtStatusUpdate.Text = txtStatusUpdate.Text + "@";
            userListControl1.Visible = true;
            userListControl1.Focus();
        }

        void userListControl1_ItemChosen(string itemText)
        {
            txtStatusUpdate.Text = txtStatusUpdate.Text + itemText;
            txtStatusUpdate.SelectionStart = txtStatusUpdate.Text.Length;
            txtStatusUpdate.Focus();
        }

        void PasteItem_Click(object sender, EventArgs e)
        {
            IDataObject iData = Clipboard.GetDataObject();
            if (iData.GetDataPresent(DataFormats.Text))
            {
                txtStatusUpdate.SelectedText = (string)iData.GetData(DataFormats.Text);
            }
        }
        void CopyItem_Click(object sender, EventArgs e)
        {
            string selText = txtStatusUpdate.SelectedText;
            if (!string.IsNullOrEmpty(selText))
            {
                Clipboard.SetDataObject(selText);
            }
        }

        
        void l_LocationReady(string Location)
        {
            try
            {
                if (InvokeRequired)
                {
                    delUpdateText d = new delUpdateText(l_LocationReady);
                    BeginInvoke(d, Location);
                }
                else
                {
                    if (!string.IsNullOrEmpty(Location))
                    {
                        LocationFinder.StopGPS();
                        GPSLocation = Location;
                        lblGPS.Text = "找到位置";
                        if (DetectDevice.DeviceType == DeviceType.Standard)
                        {
                            // just enable the menuItem
                            if (null != menuGPSInsert)
                            {
                                menuGPSInsert.Enabled = true;
                            }
                        }
                        else
                        {
                            // hide the label, add a new button
                            lblGPS.Visible = false;
                            LinkLabel llGPS = new LinkLabel();
                            llGPS.Text = "插入GPS链接";
                            llGPS.ForeColor = Color.White;
                            llGPS.Left = lblGPS.Left;
                            llGPS.Top = lblGPS.Top;
                            llGPS.Height = lblGPS.Height;
                            llGPS.Width = lblGPS.Width;
                            llGPS.Click += new EventHandler(llGPS_Click);
                            Controls.Add(llGPS);
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        void llGPS_Click(object sender, EventArgs e)
        {
            InsertGpsLocation();
        }

        private void StartLocating()
        {   
            //StartAnimation
            LocationFinder.StartGPS();
            pictureLocation.Visible = false;
            lblGPS.Visible = true;
        }

        

        private System.Windows.Forms.MenuItem menuExist;
        private System.Windows.Forms.MenuItem menuCamera;
        private System.Windows.Forms.MenuItem menuURL;
        private System.Windows.Forms.MenuItem menuGPS;
        private System.Windows.Forms.MenuItem menuGPSInsert;
        private System.Windows.Forms.MenuItem menuAddressBook;
        private System.Windows.Forms.MenuItem menuItem1;
        private void SmartPhoneMenu()
        {
            lblGPS.Left = 5;
            pictureFromCamers.Visible = false;
            pictureFromStorage.Visible = false;
            pictureLocation.Visible = false;
            pictureURL.Visible = false;
            picAddressBook.Visible = false;
            menuExist = new MenuItem();
            menuExist.Text = "现存图片";
            menuExist.Click += new EventHandler(menuExist_Click);

            menuCamera = new MenuItem();
            menuCamera.Text = "拍摄图片";
            menuCamera.Click += new EventHandler(menuCamera_Click);
            
            menuURL = new MenuItem();
            menuURL.Text = "链接...";
            menuURL.Click += new EventHandler(menuURL_Click);

            menuGPS = new MenuItem();
            menuGPS.Text = "更新位置";
            menuGPS.Click += new EventHandler(menuGPS_Click);

            menuGPSInsert = new MenuItem();
            menuGPSInsert.Text = "插入GPS位置";
            menuGPSInsert.Click += new EventHandler(menuGPSInsert_Click);
            menuGPSInsert.Enabled = false;

            menuAddressBook = new MenuItem();
            menuAddressBook.Text = "地址薄";
            menuAddressBook.Click += new EventHandler(menuAddressBook_Click);
            menuAddressBook.Enabled = true;

            PasteItem = new MenuItem();
            PasteItem.Text = "粘贴";
            PasteItem.Click += new EventHandler(PasteItem_Click);

            menuItem1 = new System.Windows.Forms.MenuItem();
            menuItem1.Text = "动作";

            menuItem1.MenuItems.Add(menuSubmit);
            menuItem1.MenuItems.Add(menuAddressBook);
            menuItem1.MenuItems.Add(PasteItem);
            menuItem1.MenuItems.Add(menuURL);
            menuItem1.MenuItems.Add(menuExist);
            menuItem1.MenuItems.Add(menuCamera);
            menuItem1.MenuItems.Add(menuGPS);
            menuItem1.MenuItems.Add(menuGPSInsert);
            mainMenu1.MenuItems.Add(menuItem1);
        }

        void menuAddressBook_Click(object sender, EventArgs e)
        {
            txtStatusUpdate.Text = txtStatusUpdate.Text + "@";
            userListControl1.Visible = true;
            userListControl1.Focus();
        }

        void menuGPSInsert_Click(object sender, EventArgs e)
        {
            InsertGpsLocation();
        }

        private void InsertGpsLocation()
        {
            // google maps url format
            // http://maps.google.com/maps?f=q&source=s_q&hl=en&geocode=&q=41.4043197631836,-1.28760504722595
            Cursor.Current = Cursors.WaitCursor;
            string sUrl = string.Format(@"http://maps.google.com/maps?q={0}", GPSLocation);
            string gpsUrl = isgd.ShortenURL(sUrl);
            if (string.IsNullOrEmpty(gpsUrl))
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("试图缩短URL时遇到连接故障。请稍后再试。");
                return;
            }
            txtStatusUpdate.Text = txtStatusUpdate.Text + " " + gpsUrl;
            Cursor.Current = Cursors.Default;
        }

        void menuGPS_Click(object sender, EventArgs e)
        {
            StartLocating();
        }

        void menuURL_Click(object sender, EventArgs e)
        {
            InsertURL();
        }

        void menuCamera_Click(object sender, EventArgs e)
        {
            InsertPictureFromCamera();
        }

        void menuExist_Click(object sender, EventArgs e)
        {
            InsertPictureFromFile();
        }

        private void SetImages()
        {
            pictureFromCamers.Image = PockeTwit.Themes.FormColors.GetThemeIcon("takepicture.png");
            pictureFromStorage.Image = PockeTwit.Themes.FormColors.GetThemeIcon("existingimage.png");
            pictureURL.Image = PockeTwit.Themes.FormColors.GetThemeIcon("url.png");
            pictureLocation.Image = PockeTwit.Themes.FormColors.GetThemeIcon("map.png");
            picAddressBook.Image = PockeTwit.Themes.FormColors.GetThemeIcon("address.png");
        }

        private void PopulateAccountList()
        {
            foreach (Yedda.Twitter.Account Account in ClientSettings.AccountsList)
            {
                cmbAccount.Items.Add(Account);
            }
        }

        #region Methods
        private void InsertURL()
        {
            using (URLForm f = new URLForm())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    txtStatusUpdate.Text = txtStatusUpdate.Text + " " + f.URL;
                }
                Show();
                f.Close();
            }
        }

        private void InsertPictureFromCamera()
        {
            if (!uploadingPicture)
            {
                String pictureUrl = string.Empty;
                String filename = string.Empty;
                try
                {
                    using (Microsoft.WindowsMobile.Forms.CameraCaptureDialog c = new Microsoft.WindowsMobile.Forms.CameraCaptureDialog())
                    {
                        if (c.ShowDialog() == DialogResult.OK)
                        {
                            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PostUpdate));
                            pictureFromStorage.Image = PockeTwit.Themes.FormColors.GetThemeIcon("existingimage.png");
                            if (DetectDevice.DeviceType == DeviceType.Standard)
                            {
                                pictureFromStorage.Visible = false;
                            }
                            uploadedPictureOrigin = "camera";
                            filename = c.FileName;                            
                        }
                        else //cancelled
                        {
                            pictureUsed = true;
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("照像设备不存在，不能拍摄照片。", "PockeTwit");
                    return;
                }
                if (string.IsNullOrEmpty(filename))
                {
                    return; //no file selected, so don't upload
                }
                try
                {
                    pictureService = GetMediaService(AccountToSet.ServerURL.ServerType);
                    StartUpload(pictureService, filename);
                }
                catch
                {
                    MessageBox.Show("无法上传图片。", "PockeTwit");
                }
            }
            else
            {
                MessageBox.Show("正在上传图片...");
            }
        }

        private void InsertPictureFromFile()
        {
            if (!uploadingPicture)
            {
                String pictureUrl = string.Empty;
                String filename = string.Empty;
                //using (Microsoft.WindowsMobile.Forms.SelectPictureDialog s = new Microsoft.WindowsMobile.Forms.SelectPictureDialog())
                try
                {
                    pictureService = GetMediaService(AccountToSet.ServerURL.ServerType);
                    filename = SelectFileVisual(pictureService.FileFilter(MediaTypeGroup.ALL));

                    //if (pictureService.CanUploadOtherMedia)
                    //{
                    //    if (MessageBox.Show("Upload a picture (yes) or a file (no)?", "PockeTwit", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                    //    {
                    //        filename = SelectFileVisual(pictureService.FileFilter(MediaTypeGroup.PICTURE));
                    //    }
                    //    else
                    //    {
                    //        filename = SelectFileNormal(pictureService.FileFilter(MediaTypeGroup.ALL));
                    //    }
                    //}
                    //else
                    //{
                    //    filename = SelectFileVisual(pictureService.FileFilter(MediaTypeGroup.PICTURE));
                    //}

                    ComponentResourceManager resources = new ComponentResourceManager(typeof(PostUpdate));
                    pictureFromCamers.Image = FormColors.GetThemeIcon("takepicture.png");
                    if (DetectDevice.DeviceType == DeviceType.Standard)
                    {
                        pictureFromCamers.Visible = false;
                    }
                }
                catch
                {
                    MessageBox.Show("无法选择图片。", "PockeTwit");
                }
                if  (string.IsNullOrEmpty(filename))
                {
                    pictureUsed = true;
                    return;
                }
                try
                {
                    uploadedPictureOrigin = "file";
                    StartUpload(pictureService, filename);
                }
                catch
                {
                    MessageBox.Show("无法上传图片。", "PockeTwit");
                } 
            }
            else
            {
                MessageBox.Show("正在上传图片...");
            }
        }

        private string SelectFileVisual(String fileFilter)
        {
            string filename = string.Empty;
            using (Microsoft.WindowsMobile.Forms.SelectPictureDialog fileDialog = new Microsoft.WindowsMobile.Forms.SelectPictureDialog())
            {
                fileDialog.Filter = fileFilter;
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    filename = fileDialog.FileName;
                }
            }
            return filename;
        }

        private string SelectFileNormal(String fileFilter)
        {
            string filename = string.Empty;
            using (System.Windows.Forms.OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = fileFilter;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    filename = fileDialog.FileName;
                }
            }
            return filename;
        }

        private void StartUpload(IPictureService mediaService, String fileName)
        {
            if (mediaService.CanUploadMessage && ClientSettings.SendMessageToMediaService)
            {
                AddPictureToForm(fileName, pictureFromStorage);
                picturePath = fileName;
                //Reduce length of message 140-pictureService.UrlLength
                pictureUsed = true;
            }
            else
            {
                uploadingPicture = true;
                AddPictureToForm(FormColors.GetThemeIconPath("wait.png"), pictureFromStorage);
                using (PicturePostObject ppo = new PicturePostObject())
                {
                    ppo.Filename = fileName;
                    ppo.Username = AccountToSet.UserName;
                    ppo.Password = AccountToSet.Password;
                    ppo.UseAsync = false;
                    Cursor.Current = Cursors.WaitCursor;
                    mediaService.PostPicture(ppo);
                }
            }
        }

        /// <summary>
        /// Set all the event handlers for the chosen picture service.
        /// Aparently after posting, event set are lost so these have to be set again.
        /// </summary>
        /// <param name="pictureService">Picture service on which the event handlers should be set.</param>
        private void SetPictureEventHandlers(IPictureService pictureService, bool addEvents)
        {
            if (!localPictureEventsSet && addEvents)
            {
                //No need to set finish upload event when posting to it
                if (!pictureService.CanUploadMessage || !ClientSettings.SendMessageToMediaService)
                {
                    pictureService.UploadFinish += new UploadFinishEventHandler(pictureService_UploadFinish);
                }
                pictureService.MessageReady += new MessageReadyEventHandler(pictureService_MessageReady);
                pictureService.ErrorOccured += new ErrorOccuredEventHandler(pictureService_ErrorOccured);
                localPictureEventsSet = true;
            }
            else if (localPictureEventsSet && !addEvents)
            {
                //No need to remove finish upload event when posting to it 
                if (!pictureService.CanUploadMessage || !ClientSettings.SendMessageToMediaService)
                {
                    pictureService.UploadFinish -= new UploadFinishEventHandler(pictureService_UploadFinish);
                }   
                pictureService.MessageReady -= new MessageReadyEventHandler(pictureService_MessageReady);
                pictureService.ErrorOccured -= new ErrorOccuredEventHandler(pictureService_ErrorOccured);
                localPictureEventsSet = false;
            }
        }

        private void pictureService_ErrorOccured(object sender, PictureServiceEventArgs eventArgs)
        {
            //Show the error message
            UpdatePictureData(string.Empty, false);
            
            if (uploadedPictureOrigin == "file")
            {
                AddPictureToForm(FormColors.GetThemeIconPath("existingimage.png"), pictureFromStorage);
            }
            else //camera
            {
                AddPictureToForm(FormColors.GetThemeIconPath("takepicture.png"), pictureFromCamers);
            }
            MessageBox.Show(eventArgs.ErrorMessage);
        }

        private void pictureService_MessageReady(object sender, PictureServiceEventArgs eventArgs)
        {
            //Show the message
            MessageBox.Show(eventArgs.ReturnMessage);

            if (uploadedPictureOrigin == "file")
            {
                AddPictureToForm(FormColors.GetThemeIconPath("existingimage.png"), pictureFromStorage);
            }
            else //camera
            {
                AddPictureToForm(FormColors.GetThemeIconPath("takepicture.png"), pictureFromCamers);
            }
            UpdatePictureData(string.Empty, false);

        }

        /// <summary>
        /// Event handling for when the upload is finished
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void pictureService_UploadFinish(object sender, PictureServiceEventArgs eventArgs)
        {
            if (uploadedPictureOrigin == "file")
            {
                AddPictureToForm(eventArgs.PictureFileName, pictureFromStorage);
            }
            else //camera
            {
                AddPictureToForm(eventArgs.PictureFileName, pictureFromCamers);
            }
            UpdatePictureData(eventArgs.ReturnMessage, false);
        }

        private IPictureService GetMediaService(Yedda.Twitter.TwitterServer server)
        {
            IPictureService service;
            if (server == Twitter.TwitterServer.fanfou)
                service = PictureServiceFactory.Instance.GetServiceByName("Fanfou");
            else
                service = PictureServiceFactory.Instance.GetServiceByName(ClientSettings.SelectedMediaService);
            SetPictureEventHandlers(service, true);

            return service;
        }

        /// <summary>
        /// Put the picture in the form.
        /// </summary>
        /// <param name="ImageFile"></param>
        /// <param name="BoxToUpdate"></param>
        private void AddPictureToForm(string ImageFile, PictureBox BoxToUpdate)
        {
            if (InvokeRequired)
            {
                delAddPicture d = new delAddPicture(AddPictureToForm);
                BeginInvoke(d, ImageFile, BoxToUpdate);
            }
            else
            {
                try
                {
                    BoxToUpdate.Image = new System.Drawing.Bitmap(ImageFile);
                    if (DetectDevice.DeviceType == DeviceType.Standard && lblGPS.Visible)
                    {
                        BoxToUpdate.Left = lblGPS.Right + 5;
                    }
                    BoxToUpdate.Visible = true;
                    txtStatusUpdate_TextChanged(null, new EventArgs());
                }
                catch (OutOfMemoryException)
                {
                    BoxToUpdate.Image = PockeTwit.Themes.FormColors.GetThemeIcon("insertlink.png");
                }
            }
        }

        private void UpdatePictureData(string pictureURL, bool uploadingPicture)
        {
            if (InvokeRequired)
            {
                delUpdatePictureData d = new delUpdatePictureData(UpdatePictureData);
                BeginInvoke(d, pictureURL, uploadingPicture);
            }
            else
            {
                try
                {
                    Cursor.Current = Cursors.Default;
                    pictureUsed = uploadingPicture;
                    //if (DetectDevice.DeviceType == DeviceType.Standard && !string.IsNullOrEmpty(pictureURL))
                    //{
                        if (txtStatusUpdate.Text.Length > 0)
                        {
                            txtStatusUpdate.Text = txtStatusUpdate.Text + ' ' + pictureURL;
                        }
                        else
                        {
                            txtStatusUpdate.Text = txtStatusUpdate.Text + pictureURL;
                        }
                        txtStatusUpdate.SelectionStart = txtStatusUpdate.Text.Length;
                        pictureUsed = true;
                    //}

                    uploadedPictureURL = pictureURL;
                    uploadingPicture = uploadingPicture;
                }
                catch (OutOfMemoryException)
                {
                }
            }
        }

        private string TrimTo140(string Original)
        {
            try
            {
                if (Original.Length > 140)
                {
                    Original = TryToShrinkWith140It(Original);
                    txtStatusUpdate.Text = Original;
                }
            }
            catch{}
            try
            {
                if (Original.Length > 140)
                {
                    Original = TryToUseShortText(Original);
                    txtStatusUpdate.Text = Original;
                }
            }
            catch{}
            return Original;
        }

        private static string TryToShrinkWith140It(string original)
        {
            if(MessageBox.Show("消息文本太长。你希望用一些缩写代替原来的文本以便缩短它的长度吗？", "消息文本太长", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1)== System.Windows.Forms.DialogResult.Yes)
            {
                var shrinker = new _140it();
                return shrinker.GetShortenedText(original);
            }
            return original;
        }

        private string TryToUseShortText(string original)
        {
            if (MessageBox.Show("消息文本太长。你希望用一个链接来代替这些文本吗？这个链接会指向你所输入的文本内容。", "消息文本太长", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
            {
                var shrinker = new ShortText();

                return shrinker.GetShortenedText(original);
            }
            return original;

        }

        private bool PostTheUpdate()
        {
            LocationFinder.StopGPS();
            if (!string.IsNullOrEmpty(StatusText) ||
                (!string.IsNullOrEmpty(picturePath) && pictureService.CanUploadMessage && ClientSettings.SendMessageToMediaService))
            {
                Cursor.Current = Cursors.WaitCursor;
                var updateText = TrimTo140(StatusText);

                if(updateText.Length>140)
                {
                    if (MessageBox.Show("消息文本依然太长，它将被截短到最大允许的长度后发布。继续吗？", "消息文本太长", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.No)
                    {
                        return false;
                    }
                }
                
                if (!string.IsNullOrEmpty(picturePath) && pictureService.CanUploadMessage && ClientSettings.SendMessageToMediaService )
                {
                    PicturePostObject ppo = new PicturePostObject();
                    ppo.Filename = picturePath;
                    ppo.Username = AccountToSet.UserName;
                    ppo.Password = AccountToSet.Password;
                    ppo.Message = StatusText;

                    if (pictureService.CanUploadGPS && GPSLocation != null)
                    {
                        try
                        {
                            ppo.Lat = GPSLocation.Split(',')[0];
                            ppo.Lon = GPSLocation.Split(',')[1];
                        }
                        catch { }
                    }

                    return pictureService.PostPictureMessage(ppo);
                }
                else
                {


                    Yedda.Twitter TwitterConn = new Yedda.Twitter();
                    TwitterConn.AccountInfo = AccountToSet;

                    try
                    {
                        if (GPSLocation != null)
                        {
                            TwitterConn.SetLocation(GPSLocation);
                        }
                    }
                    catch { }
                    string retValue = string.Empty;
                    string userID = IsDirectMessage(updateText);
                    if (string.IsNullOrEmpty(userID))
                        retValue = TwitterConn.Update(updateText, in_reply_to_status_id, Yedda.Twitter.OutputFormatType.XML);
                    else
                    {
                        retValue = TwitterConn.SendDirectMessage(userID, updateText.Substring(updateText.IndexOf(userID) + userID.Length + 1), in_reply_to_status_id, Yedda.Twitter.OutputFormatType.XML);
                    }
                    uploadedPictureURL = string.Empty;
                    uploadingPicture = false;

                    if (string.IsNullOrEmpty(retValue))
                    {
                        MessageBox.Show("无法发布消息：返回结果为空。请稍候再试。");
                        return false;
                    }
                    try
                    {
                        Library.status.DeserializeSingle(retValue, AccountToSet);
                    }
                    catch
                    {
                        MessageBox.Show("无法发布消息：返回结果非法。请稍候再试。");
                        return false;
                    }

                    return true;
                }
            }
            return true;
        }

        private string IsDirectMessage(string updateText)
        {
            if (updateText.StartsWith("d ", StringComparison.OrdinalIgnoreCase))
            {
                int start = 2;
                while (updateText[start] == ' ')
                    ++start;
                int end = updateText.IndexOf(' ', start);
                if ((end != -1) && (end != updateText.Length))
                {
                    return updateText.Substring(start, end - start);
                }
            }
            return null;
        }
        #endregion

        #region Events
        private void txtStatusUpdate_TextChanged(object sender, EventArgs e)
        {
            int charsAvail = 140;
            
            int lengthLeft = charsAvail - txtStatusUpdate.Text.Length;
            lblCharsLeft.Text = lengthLeft.ToString();
        }
        private void menuCancel_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtStatusUpdate.Text))
            {
                if (MessageBox.Show("确认要放弃更新吗？", "取消", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.No)
                {
                    return;
                }
            }

            SetPictureEventHandlers(pictureService, false);
            UpdatePictureData(string.Empty, false);

            //Making sure gps is stopped when a location is not found yet.
            LocationFinder.StopGPS();

            if (_StandAlone)
            {
                Close();
            }
            DialogResult = DialogResult.Cancel;
        }
        void pictureLocation_Click(object sender, EventArgs e)
        {
            StartLocating();
        }
        void pictureURL_Click(object sender, EventArgs e)
        {
            InsertURL();
        }

        void pictureFromStorage_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(uploadedPictureURL) || ClientSettings.SendMessageToMediaService)
            {
                pictureUsed = false;
                InsertPictureFromFile();
            }
            else
            {
                //Pre loading logic
                if (MessageBox.Show("把上传图片的链接添加到消息中吗？", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    txtStatusUpdate.Text += uploadedPictureURL;
                    txtStatusUpdate.SelectionStart = txtStatusUpdate.Text.Length;
                    pictureUsed = true;
                }
                else
                {
                    if (MessageBox.Show("加载一张新的图片？", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        uploadedPictureURL = string.Empty;
                        pictureUsed = false;
                        uploadingPicture = false;
                        InsertPictureFromFile();
                    }
                }
            }
            
        }
        void pictureFromCamers_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(uploadedPictureURL) || ClientSettings.SendMessageToMediaService)
            {
                //When not pre loading just select another picture.
                pictureUsed = false;
                InsertPictureFromCamera();
            }
            else
            {
                //Pre loading picture logic.
                if (MessageBox.Show("把上传图片的链接添加到消息中吗？", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    txtStatusUpdate.Text += uploadedPictureURL;
                    txtStatusUpdate.SelectionStart = txtStatusUpdate.Text.Length;
                    pictureUsed = true;
                }
                else
                {
                    if (MessageBox.Show("拍摄一张新的照片？", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        uploadedPictureURL = string.Empty;
                        pictureUsed = false;
                        uploadingPicture = false;
                        InsertPictureFromCamera();
                    }
                }
            }
        }
        
        private void menuSubmit_Click(object sender, EventArgs e)
        {
            if (!pictureUsed && !ClientSettings.SendMessageToMediaService)
            {
                //Only show message when pre-loading pictures is enabled.
                if (MessageBox.Show("图片己经上传，但未被使用。你确定要这样做吗？", "PockeTwit", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.No)
                {
                    return;
                }
            }
            bool Success = PostTheUpdate();

            Cursor.Current = Cursors.Default;
            if (Success)
            {
                UpdatePictureData(string.Empty, false);
                SetPictureEventHandlers(pictureService, false);
                if (_StandAlone)
                {
                    Close();
                }
                //Making sure gps is stopped when a location is not found yet.
                LocationFinder.StopGPS();
                DialogResult = DialogResult.OK;
            }
        }
        private void cmbAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            AccountToSet = (Yedda.Twitter.Account)cmbAccount.SelectedItem;
        }

        #endregion



        protected override void OnClosed(EventArgs e)
        {
            if (ClientSettings.AutoCompleteAddressBook)
            {
                userListControl1.UnHookTextBoxKeyPress();
            }
            base.OnClosed(e);
        }
    }
}
