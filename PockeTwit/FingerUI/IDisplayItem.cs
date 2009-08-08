using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace PockeTwit.FingerUI
{
    public interface IDisplayItem
    {
        KListControl Parent { get; set; }
        int Index { get; set; }
        Graphics ParentGraphics { set; }
        Rectangle Bounds { get; set; }
        bool Selected { get; set; }

        void Render(Graphics g, Rectangle bounds);
        void OnMouseClick(Point p);
    }
}
