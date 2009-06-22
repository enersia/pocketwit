using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit.SettingsHandler
{
    public partial class AdvancedForm : BaseSettingsForm
    {
        public AdvancedForm()
        {
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            SetRenderingMethod();
        }

        private void SetRenderingMethod()
        {
            if (ClientSettings.UseDIB)
            {
                lblRenderingMethod.Text = "使用DIB模式";
                chkDIB.Checked = true;
            }
            else
            {
                chkDIB.Checked = false;
                lblRenderingMethod.Text = "使用DDB模式";
            }
        }
        
        private void lnkClearCaches_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("你确认要删除所有消息缓存吗？", "PockeTwit", MessageBoxButtons.YesNo, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                LocalStorage.DataBaseUtility.CleanDB(0);
            }
        }


        private void lnkClearSettings_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("你确认要删除所有应用程序的设置吗？", "PockeTwit", MessageBoxButtons.YesNo, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                System.IO.File.Delete(ClientSettings.AppPath + "\\app.config");
            }
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void chkDIB_CheckStateChanged(object sender, EventArgs e)
        {
            ClientSettings.UseDIB = chkDIB.Checked;
            ClientSettings.SaveSettings();
            this.NeedsReset = true;
            SetRenderingMethod();
        }

        private void lblCompact_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("压缩数据库需要几分钟时间。\n\n继续吗？", "压缩数据库", MessageBoxButtons.YesNo,MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                LocalStorage.DataBaseUtility.VacuumDB();
                Cursor.Current = Cursors.Default;
            }
        }

    }
}