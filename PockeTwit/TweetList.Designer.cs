﻿namespace PockeTwit
{
    partial class TweetList
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu1;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.statusList = new FingerUI.KListControl();
            this.tmrautoUpdate = new System.Windows.Forms.Timer();
            this.SuspendLayout();
            // 
            // statusList
            // 
            this.statusList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusList.Location = new System.Drawing.Point(0, 0);
            this.statusList.Name = "statusList";
            this.statusList.Size = new System.Drawing.Size(240, 268);
            this.statusList.TabIndex = 0;
            // 
            // tmrautoUpdate
            // 
            this.tmrautoUpdate.Tick += new System.EventHandler(this.tmrautoUpdate_Tick);
            // 
            // TweetList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.statusList);
            this.Menu = this.mainMenu1;
            this.Name = "TweetList";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private FingerUI.KListControl statusList;
        private System.Windows.Forms.Timer tmrautoUpdate;
    }
}

