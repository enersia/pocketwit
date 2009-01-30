using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit
{
    public partial class OtherSettings : Form
    {

        #region�Constructors�(1)�

        public OtherSettings()
        {
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }

            PopulateForm();
        }

		#endregion�Constructors�

		#region�Properties�(1)�

        public bool NeedsReset { get; set; }

		#endregion�Properties�

		#region�Methods�(4)�


		//�Private�Methods�(4)�

        

        private void menuAccept_Click(object sender, EventArgs e)
        {
            IFormatProvider format = new System.Globalization.CultureInfo(1033);
            ClientSettings.UseGPS = chkGPS.Checked;
            ClientSettings.CheckVersion = chkVersion.Checked;
            ClientSettings.AutoTranslate = chkTranslate.Checked;
            if (ClientSettings.UpdateMinutes != int.Parse(txtUpdate.Text, format))
            {
                MessageBox.Show("You will need to restart PockeTwit for the update interval to change.", "PockeTwit");
                ClientSettings.UpdateMinutes = int.Parse(txtUpdate.Text, format);
            }
            ClientSettings.SaveSettings();
            
            this.DialogResult = DialogResult.OK;
            this.Close();

        }

        private void menuCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void PopulateForm()
        {
            chkVersion.Checked = ClientSettings.CheckVersion;
            chkGPS.Checked = ClientSettings.UseGPS;
            txtUpdate.Text = ClientSettings.UpdateMinutes.ToString();
            chkTranslate.Checked = ClientSettings.AutoTranslate;
            chkTranslate.Text = "Auto-translate to " + ClientSettings.TranslationLanguage;
            this.DialogResult = DialogResult.Cancel;
        }


		#endregion�Methods�

        private void linkLabel1_Click(object sender, EventArgs e)
        {
            if (DetectDevice.DeviceType == DeviceType.Professional)
            {
                System.Diagnostics.ProcessStartInfo ps = new System.Diagnostics.ProcessStartInfo("\\Windows\\ctlpnl.exe", "cplmain.cpl,9,1");
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo = ps;
                p.Start();
            }
            else
            {
                SettingsHandler.NotificationSettings n = new PockeTwit.SettingsHandler.NotificationSettings();
                n.ShowDialog();
            }
        }

    }
}