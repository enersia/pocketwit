using System;
using System.Collections.Generic;
using System.Text;

namespace PockeTwit.Position
{
    class PositionEventArgs : EventArgs
    {
        public enum PositionStatus { Valid, Invalid, Error };
        
        public PositionEventArgs(PositionStatus status, GeoCoord position, DateTime time)
        {
            this.status = status;
            this.position = position;
            this.time = time;
        }
        public GeoCoord position;
        public PositionStatus status;
        public DateTime time;
    }
}
