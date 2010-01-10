using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit.FingerUI.SpellingCorrections
{
    internal delegate void delItemSelected(string text);
    interface ISpellingCorrector
    {
        string[] Suggestions { set; }
        string Original { set; }

        event delItemSelected ItemSelected;

        Control AppliesTo { set; }

        void Display();

    }
}
