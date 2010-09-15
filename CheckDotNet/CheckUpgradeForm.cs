using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;

using System.Runtime.InteropServices;

namespace CheckDotNet
{
    public partial class CheckUpgradeForm : Form
    {

        [DllImport("coredll.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("coredll.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("coredll.dll")]
        private static extern int GetClassName(IntPtr hWnd,
        StringBuilder lpClassName, int nMaxCount);

        [DllImport("coredll.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out IntPtr ProcessId);

        private const uint GW_HWNDLAST = 1;
        private const uint GW_HWNDPREV = 3;
        private const string szWceLoadClass = "MSWCELOAD";



        private HttpWebRequest request;
        private HttpWebResponse response;
        private FileStream filestream;

        private int pbVal, maxVal;

        // Data buffer for stream operations
        private byte[] dataBuffer;
        private const int DataBlockSize = 65536;

        public CheckUpgradeForm()
        {
            InitializeComponent();
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            menuItem1.Click -= menuItem1_Click;
            menuItem1.Click += menuItem1_CancelClick;

            PerformUpdate();
        }

        private void menuItem1_CancelClick(object sender, EventArgs e)
        {
            menuItem2.Enabled = true;
            menuItem1.Click -= menuItem1_CancelClick;
            menuItem1.Click += menuItem1_Click;
            menuItem1.Text = "Yes";
            if (request != null)
            {
                request.Abort();
                request = null;
                
            }
            menuItem2_Click(sender, e);
        }

        private void menuItem2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("PockeTwit will not run without this upgrade. Please visit the PockeTwit website to upgrade manually.", "Cancel Upgrade", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            Close();
        }

        private delegate void delNothing();
        private void PerformUpdate()
        {
            if (InvokeRequired)
            {
                delNothing d = PerformUpdate;
                Invoke(d);
            }
            else
            {
                pbVal = 0;
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Update");
                request = (HttpWebRequest)HttpWebRequest.Create("http://pocketwit.googlecode.com/files/NETCFv35.wm.armv4i.cab");
                request.BeginGetResponse((ResponseReceived), null);
                //EnableMenu(false);

                menuItem1.Text = "Cancel";
                menuItem2.Enabled = false;

                lblDownloading.Visible = true;
                progressDownload.Visible = true;
            }

            Cursor.Current = Cursors.WaitCursor;
        }

        public void SetProgressMax(object sender, EventArgs e)
        {
            progressDownload.Maximum = maxVal;
            Application.DoEvents();
        }
        public void UpdateProgressValue(object sender, EventArgs e)
        {
            progressDownload.Value = pbVal;
            Application.DoEvents();
        }

        void ResponseReceived(IAsyncResult res)
        {
            try
            {
                response = (HttpWebResponse)request.EndGetResponse(res);
                // Allocate data buffer
                dataBuffer = new byte[DataBlockSize];
                // Set up progrees bar
                maxVal = (int)response.ContentLength;
                progressDownload.Invoke(new EventHandler(SetProgressMax));

                // Open file stream to save received data
                filestream = new FileStream(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Update\\NetUpgrade.cab", FileMode.Create);
                // Request the first chunk
                response.GetResponseStream().BeginRead(dataBuffer, 0, DataBlockSize, new AsyncCallback(OnDataRead), this);
            }
            catch(Exception e)
            {
                Invoke(new EventHandler(Error));
            }
        }

        public void Error(object sender, EventArgs e)
        {
            if (MessageBox.Show("There was an error downloading the upgrade.  Would you like to try again?", "PockeTwit", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                PerformUpdate();
            }
            else
            {
                MessageBox.Show("You can download the upgrade manually from http://code.google.com/p/pocketwit/");
                Close();
            }
        }

        void OnDataRead(IAsyncResult res)
        {
            try
            {
                // How many bytes did we get this time
                int nBytes = response.GetResponseStream().EndRead(res);
                // Write buffer
                filestream.Write(dataBuffer, 0, nBytes);
                // Update progress bar using Invoke()
                pbVal += nBytes;

                progressDownload.Invoke(new EventHandler(UpdateProgressValue));

                // Are we done yet?
                if (nBytes > 0)
                {
                    // No, keep reading
                    response.GetResponseStream().BeginRead(dataBuffer, 0, DataBlockSize, new AsyncCallback(OnDataRead), this);
                }
                else
                {
                    if (response.StatusCode != HttpStatusCode.Unused)
                    {
                        // Yes, perform cleanup and update UI.
                        filestream.Close();
                        filestream = null;
                        DoneDownloading();
                    }
                    else
                    {

                    }
                }
            }
            catch (WebException ex)
            {
                Invoke(new EventHandler(Error));
            }
            catch (Exception ex)
            {
                Invoke(new EventHandler(Error));
            }
        }

        void DoneDownloading()
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Update\\NetUpgrade.cab";
            p.StartInfo.UseShellExecute = true;
            p.Start();
            Application.Exit();
        }

        private void CabLoader_Exited(object sender, EventArgs e)
        {
            Show();
        }
        private void CheckUpgradeForm_Load(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start("PockeTwit.exe", "");
            IntPtr hwnd = GetWindow(GetDesktopWindow(), GW_HWNDLAST);
            StringBuilder className = new StringBuilder(50);
            IntPtr processID;
            do
            {
                // Look for the loader and if it is running wait for it to finish 
                GetClassName(hwnd, className, 50);
                if (className.ToString() == szWceLoadClass)
                {
                    GetWindowThreadProcessId(hwnd, out processID);
                    System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(processID.ToInt32());
                    p.Kill();
                    //Hide();
                    //p.EnableRaisingEvents = true;
                    //p.Exited += CabLoader_Exited;
                    p.WaitForExit();
                    
                }

                hwnd = GetWindow(hwnd, GW_HWNDPREV);
            } while (hwnd != IntPtr.Zero);

        }

    }
}