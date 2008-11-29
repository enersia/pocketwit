using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit
{
    public partial class SetStatus : Form
    {

		#region�Fields�(2)�
        private delegate void delUpdateText(string text);
        public string TwitPicFile = null;
        public bool UseTwitPic = false;
        public string GPSLocation = null;
        private LocationManager Locator = new LocationManager();
        #endregion�Fields�

		#region�Constructors�(1)�

        public SetStatus()
        {
            Locator.LocationReady += new LocationManager.delLocationReady(Locator_LocationReady);
            InitializeComponent();
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            lblCharsLeft.Text = "140";
            PopulateAccountList();
            
            if (ClientSettings.UseGPS)
            {
                Locator.StartGPS();
            }
            textBox1.Focus();
        }

        void Locator_LocationReady(string GPSLocation)
        {
            SwitchOnGPS();
            this.GPSLocation = GPSLocation;
        }
        
        private delegate void delSwitchOnGPS();
        private void SwitchOnGPS()
        {
            if (InvokeRequired)
            {
                delSwitchOnGPS d = new delSwitchOnGPS(SwitchOnGPS);
                this.Invoke(d, null);
            }
            else
            {
                lblGPS.Visible = false;
                chkGPS.Visible = true;
            }
        }

        

        
        private void PopulateAccountList()
        {
            foreach (Yedda.Twitter.Account Account in ClientSettings.AccountsList)
            {
                cmbAccount.Items.Add(Account);
            }
        }
		#endregion�Constructors�

		#region�Properties�(2)�

        private Yedda.Twitter.Account _AccountToSet = ClientSettings.AccountsList[0];
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
                if (DetectDevice.DeviceType == DeviceType.Professional)
                {
                    pictureBox1.Visible = value;
                    pictureBox2.Visible = value;
                }
                else
                {
                    menuCamera.Enabled = value;
                    menuExist.Enabled = value;
                }
            }
        }

        public string StatusText
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                textBox1.Text = value;
                this.textBox1.SelectionStart = this.textBox1.Text.Length;
            }
        }

		#endregion�Properties�

		#region�Methods�(12)�


		//�Private�Methods�(12)�

        private void InsertPictureFromCamera()
        {
            using (Microsoft.WindowsMobile.Forms.CameraCaptureDialog c = new Microsoft.WindowsMobile.Forms.CameraCaptureDialog())
            {
                try
                {
                    if (c.ShowDialog() == DialogResult.OK)
                    {
                        TwitPicFile = c.FileName;
                        UseTwitPic = true;
                        //if (pictureBox1.Visible)
                        //{
                            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetStatus));
                            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
                            if (DetectDevice.DeviceType == DeviceType.Standard)
                            {
                                this.pictureBox2.Visible = false;
                            }
                            AddPictureToForm(c.FileName, pictureBox1);
                        //}
                    }
                }
                catch
                {
                    MessageBox.Show("The camera is not available.", "PockeTwit");
                }
            }
        }

        private void InsertPictureFromFile()
        {
            Microsoft.WindowsMobile.Forms.SelectPictureDialog s = new Microsoft.WindowsMobile.Forms.SelectPictureDialog();
            if (s.ShowDialog() == DialogResult.OK)
            {
                TwitPicFile = s.FileName;
                UseTwitPic = true;
                //if (pictureBox1.Visible)
                //{
                    System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetStatus));
                    this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
                    if (DetectDevice.DeviceType == DeviceType.Standard)
                    {
                        this.pictureBox1.Visible = false;
                    }
                    AddPictureToForm(s.FileName, pictureBox2);
                //}
            }
        }

        private void AddPictureToForm(string ImageFile, PictureBox BoxToUpdate)
        {
            try
            {
                BoxToUpdate.Image = new System.Drawing.Bitmap(ImageFile);
                BoxToUpdate.Visible = true;
            }
            catch (OutOfMemoryException)
            {
            }
        }

        private void InsertURL()
        {
            URLForm f = new URLForm();
            if (f.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = textBox1.Text + " " + f.URL;
            }
            this.Show();
            f.Close();
        }

        private void menuCancel_Click(object sender, EventArgs e)
        {
            if (ClientSettings.UseGPS)
            {
                Locator.StopGPS();
            }
            this.DialogResult = DialogResult.Cancel;
        }

        void menuExists_Click(object sender, EventArgs e)
        {
            InsertPictureFromFile();
        }
        void menuCamera_Click(object sender, EventArgs e)
        {
            InsertPictureFromCamera();
        }

        private void menuSubmit_Click(object sender, EventArgs e)
        {
            if (ClientSettings.UseGPS)
            {
                Locator.StopGPS();
            }
            if (!chkGPS.Checked)
            {
                this.GPSLocation = null;   
            }
            this.DialogResult = DialogResult.OK;
        }

        void menuURL_Click(object sender, EventArgs e)
        {
            InsertURL();
        }

        
        private void textBox1_GotFocus(object sender, EventArgs e)
        {
            if (textBox1.Text == "Set Status")
            {
                textBox1.SelectionStart = 0;
                textBox1.SelectionLength = textBox1.Text.Length;
            }
            else
            {
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.SelectionLength = 0;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int lengthLeft = 140-textBox1.Text.Length;
            lblCharsLeft.Text = lengthLeft.ToString();
        }


		#endregion�Methods�

        private void cmbAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.AccountToSet = (Yedda.Twitter.Account)cmbAccount.SelectedItem;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            InsertPictureFromCamera();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            InsertPictureFromFile();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            InsertURL();
        }

        void PasteItem_Click(object sender, System.EventArgs e)
        {
            IDataObject iData = Clipboard.GetDataObject();
            if (iData.GetDataPresent(DataFormats.Text))
            {
                textBox1.SelectedText=(string)iData.GetData(DataFormats.Text);
            }
        }
        void CopyItem_Click(object sender, System.EventArgs e)
        {
            string selText = textBox1.SelectedText;
            if (!string.IsNullOrEmpty(selText))
            {
                Clipboard.SetDataObject(selText);
            }
        }
    }
}