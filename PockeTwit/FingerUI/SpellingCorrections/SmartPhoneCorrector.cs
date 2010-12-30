using System;
using System.Drawing;
using System.Windows.Forms;
using PockeTwit.Themes;

namespace PockeTwit.FingerUI.SpellingCorrections
{
    class SmartPhoneCorrector : UserControl, ISpellingCorrector
    {
        public SmartPhoneCorrector()
        {
            InitializeComponent();
            FormColors.SetColors(this);
        }
        private Panel panelSuggestions;
        private LinkLabel linkChoose;
        private Label lblOriginal;
        #region ISpellingCorrector Members

        public Control AppliesTo 
        { 
            set
            {
                value.Parent.Controls.Add(this);   
            }
        }

        public string[] Suggestions
        {
            set
            {
                ClearHandlers();
                panelSuggestions.Controls.Clear();
                int top = 0;
                int count = 1;
                foreach (var suggestion in value)
                {
                    var linkSuggestion = new LinkLabel
                                             {
                                                 Text = string.Format("{0}: {1}", count, suggestion)
                                             };
                    linkSuggestion.Location=new Point(0,top);
                    linkSuggestion.Width = panelSuggestions.Width;
                    linkSuggestion.Height = ClientSettings.TextSize;
                    linkSuggestion.Click += linkSuggestion_Click;
                    linkSuggestion.Tag = suggestion;
                    top = top + ClientSettings.TextSize + 2;
                    panelSuggestions.Controls.Add(linkSuggestion);
                    count++;
                }
            }
        }

        private void ClearHandlers()
        {
            foreach (LinkLabel control in panelSuggestions.Controls)
            {
                control.Click -= linkSuggestion_Click;
            }
        }

        void linkSuggestion_Click(object sender, EventArgs e)
        {
            ItemSelected((string)((LinkLabel)sender).Tag);
            Visible = false;
            Invalidate();
        }

        private void SetOriginalLabel()
        {
            lblOriginal.Text = string.Format("Original: {0}", _original);
        }

        private string _original;
        public string Original
        {
            set
            {
                _original = value;
                lblOriginal.Text = string.Format("Original: {0}", _original);
            }
            get
            {
                return _original;
            }
        }

        public event delItemSelected ItemSelected;
        public void Display()
        {
            this.BringToFront();
            this.Visible = true;
            this.Focus();
            Invalidate();
        }
        #endregion

        private void InitializeComponent()
        {
            this.lblOriginal = new System.Windows.Forms.Label();
            this.panelSuggestions = new System.Windows.Forms.Panel();
            this.linkChoose = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // lblOriginal
            // 
            this.lblOriginal.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblOriginal.Location = new System.Drawing.Point(0, 0);
            this.lblOriginal.Name = "lblOriginal";
            this.lblOriginal.Size = new System.Drawing.Size(981, 20);
            SetOriginalLabel();
            // 
            // listSuggestions
            // 
            this.panelSuggestions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSuggestions.Location = new System.Drawing.Point(0, 20);
            this.panelSuggestions.Name = "panelSuggestions";
            this.panelSuggestions.Size = new System.Drawing.Size(981, 814);
            //if (DetectDevice.DeviceType == DeviceType.Professional)
            //{
            //    this.panelSuggestions.TabIndex = 1;
            //}
            // 
            // linkChoose
            // 
            this.linkChoose.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.linkChoose.Location = new System.Drawing.Point(0, 826);
            this.linkChoose.Name = "linkChoose";
            this.linkChoose.Size = new System.Drawing.Size(981, 20);
            this.linkChoose.TabIndex = 3;
            this.linkChoose.Text = "Cancel";
            this.linkChoose.Click += new EventHandler(linkChoose_Click);
            // 
            // SmartPhoneCorrector
            // 
            this.KeyPress += new KeyPressEventHandler(SmartPhoneCorrector_KeyPress);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.linkChoose);
            this.Controls.Add(this.panelSuggestions);
            this.Controls.Add(this.lblOriginal);
            this.Name = "SmartPhoneCorrector";
            this.Dock = DockStyle.Fill;
            this.Visible = false;
            this.ResumeLayout(false);

        }

        void SmartPhoneCorrector_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar>='0' && e.KeyChar<='9')
            {
                var pressed = int.Parse(e.KeyChar.ToString());
                if(panelSuggestions.Controls.Count<pressed)
                {
                    return;
                }
                linkSuggestion_Click(panelSuggestions.Controls[pressed], new EventArgs());
            }
        }

        void linkChoose_Click(object sender, EventArgs e)
        {
            ItemSelected(Original);
            Visible = false;
            Parent.Focus();
        }

    }
}
