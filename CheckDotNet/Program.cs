using System;

using System.Collections.Generic;
using System.Windows.Forms;

namespace CheckDotNet
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {
            if(Environment.Version.Major < 3 || Environment.Version.Major == 3 && Environment.Version.Minor < 5)
                Application.Run(new CheckUpgradeForm());
            //System.Diagnostics.Process.Start("PockeTwit.exe", "");
        }
    }
}