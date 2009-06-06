using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Yedda;
using PockeTwit.MediaServices;

namespace PockeTwit
{
    public partial class MediaService : BaseSettingsForm
    {
        public MediaService()
        {
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Themes.FormColors.SetColors(this.pnlCapabilites);

            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }

            setMediaService(ClientSettings.MediaService);
            cbPreUpload.Checked = !ClientSettings.SendMessageToMediaService;
            if(!ClientSettings.DoNotNotifyDefaultMedia)
            {
                ShowSupportMessage();
            }
        }

        private void menuAccept_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cmbMediaService.Items[cmbMediaService.SelectedIndex].ToString()))
            {
                ClientSettings.MediaService = "TwitPic";
            }
            else
            {
                ClientSettings.MediaService = cmbMediaService.Items[cmbMediaService.SelectedIndex].ToString();
            }
            ClientSettings.SendMessageToMediaService = !cbPreUpload.Checked;

            ClientSettings.SaveSettings();

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void menuCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void setMediaService(string value)
        {
            foreach (string serviceName in PictureServiceFactory.Instance.GetServiceNames())
            {
                cmbMediaService.Items.Add(serviceName);
            }

            foreach (string comboValue in cmbMediaService.Items)
            {
                if (comboValue == value)
                {
                    cmbMediaService.SelectedItem = value;
                    return;
                }
            }
        }

        private void ShowSupportMessage()
        {
            const string message = "Please consider choosing TweetPhoto as your media host." + 
                 "TweetPhoto has agreed to provide advertising revenue to PockeTwit "+
                 "for the photos uploaded using this application.  This support will be greatly appreciated.";
            MessageBox.Show(message, "Support PockeTwit", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
            ClientSettings.DoNotNotifyDefaultMedia = true;
            ClientSettings.SaveSettings();
        }

        private void cmbMediaService_SelectedValueChanged(object sender, EventArgs e)
        {
            string ServiceName = (string) cmbMediaService.SelectedItem;
            IPictureService service = PictureServiceFactory.Instance.GetServiceByName(ServiceName);

            chkMessage.Checked = service.CanUploadMessage;
            chkGPS.Checked = service.CanUploadGPS;
            this.lblMediaLabel.Text = ServiceName + " can: ";
        }
    }
}