using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace PockeTwit.FingerUI
{
    public interface IDisplayItem
    {
        Graphics ParentGraphics { set; }
        void Render(Graphics g, Rectangle bounds);
        Rectangle Bounds { get; set; }
        bool Selected { get; set; }
    }
}
