using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit
{
    public partial class AvatarSettings : BaseSettingsForm
    {

        #region Constructors (1) 

        public AvatarSettings()
        {
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            PopulateForm();
        }

		#endregion Constructors 

		
		#region Methods (4) 
        // Private Methods (4) 
        private void menuAccept_Click(object sender, EventArgs e)
        {
            if (chkHighQuality.Checked != ClientSettings.HighQualityAvatars)
            {
                if (MessageBox.Show("You should clear the cache when switching avatars.  Would you like to do that now?", "PockeTwit", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    ClearCache();
                }
            }
            ClientSettings.ShowAvatars = chkAvatar.Checked;
            ClientSettings.HighQualityAvatars = chkHighQuality.Checked;
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
            chkAvatar.Checked = ClientSettings.ShowAvatars;
            chkHighQuality.Checked = ClientSettings.HighQualityAvatars;
            this.DialogResult = DialogResult.Cancel;
        }


		#endregion Methods 

        

        private void lnkClearAvatars_Click(object sender, EventArgs e)
        {
            ClearCache();
        }

        private static void ClearCache()
        {
            if (MessageBox.Show("清除头像缓存可能需要几分钟时间，确认继续吗？", "清除缓存", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    ThrottledArtGrabber.ClearAvatars();
                    MessageBox.Show("头像缓存已经清除完毕！", "PockeTwit");
                }
                catch
                {
					MessageBox.Show("清除头像缓存时发生了错误，请稍候重试。",
                                    "PockeTwit");
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }
    }
}