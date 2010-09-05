using System;
using System.Collections.Generic;
using System.Text;

namespace PockeTwit.Position
{
    interface IPositionProvider
    {
        GeoCoord GetCurrentPosition();
        event EventHandler<PositionEventArgs> PositionChanged;
        bool Enabled
        {
            get;
            set;
        }
    }
}
