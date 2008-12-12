﻿using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit.SettingsHandler
{
    public partial class AdvancedForm : Form
    {
        public AdvancedForm()
        {
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            showBuffer();
        }

        private void showBuffer()
        {
            lblRenderBuffer.Text = "Current buffer size: " + ClientSettings.PortalSize.ToString();
        }

        private void lnkResetBuffer_Click(object sender, EventArgs e)
        {
            ClientSettings.PortalSize = ClientSettings.MaxTweets;
            ClientSettings.SaveSettings();
            showBuffer();
            MessageBox.Show("You must restart PockeTwit for the buffer to reset.", "PockeTwit");
        }

        private void lnkClearCaches_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all cached statuses?", "PockeTwit", MessageBoxButtons.YesNo, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                string[] Caches = System.IO.Directory.GetFiles(ClientSettings.AppPath, "*.xml");
                foreach (string Cache in Caches)
                {
                    try
                    {
                        System.IO.File.Delete(Cache);
                    }
                    catch { };
                }
            }
        }

        private void lnkClearSettings_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all cached statuses?", "PockeTwit", MessageBoxButtons.YesNo, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                System.IO.File.Delete(ClientSettings.AppPath + "\\app.config");
            }
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}