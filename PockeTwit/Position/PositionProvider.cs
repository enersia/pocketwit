using System;
using System.Collections.Generic;
using System.Text;

namespace PockeTwit.Position
{
    abstract class PositionProvider : IPositionProvider
    {
        public event EventHandler<PositionEventArgs> PositionChanged;
        protected GeoCoord LastPos = null;
        protected virtual void OnPositionChanged(PositionEventArgs e)
        {
            if (e.status == PositionEventArgs.PositionStatus.Valid)
                LastPos = e.position;
            else
                LastPos = null;
            if (PositionChanged != null)
                PositionChanged(this, e);
        }

        public virtual GeoCoord GetCurrentPosition()
        {
            return LastPos;
        }
        public bool Enabled
        {
            get { return FEnabled;}
            set {
                    if(!FEnabled && value) { Enable(); }
                    else if(FEnabled && !value) { Disable(); }
                    FEnabled = value;
            }
        }
        private bool FEnabled;
        protected abstract void Enable();
        protected abstract void Disable();
    }
}
