using System;

using System.Collections.Generic;
using System.Text;
using PockeTwit.Position;

namespace PockeTwit
{
    class LocationManager
    {
        //Ummm, just in case :)
        //http://maps.google.com/staticmap?center=37.393891,-122.066517&markers=37.400465,-122.073003,red&path=rgba:0x0000FF80,weight:5|37.40489,-122.05261&zoom=13&size=500x300&key=ABQIAAAA-6_yG-9k0X8KgfnjWXnHpBRtiFeQlxP7WphYAvKAZsrpIYWXFxQBOiC3o3mV0Wc7L8i2JnuMvxLdRQ
        //private const GoogleAPIKey = "ABQIAAAA-6_yG-9k0X8KgfnjWXnHpBRtiFeQlxP7WphYAvKAZsrpIYWXFxQBOiC3o3mV0Wc7L8i2JnuMvxLdRQ";

        public enum LocationSource
        {
            RIL,
            GPS
        };

        public delegate void delLocationReady(GeoCoord Location, LocationSource Source);
        public event delLocationReady LocationReady;
        private PockeTwit.Position.GeoCoord position = null;
        private GPS.Gps gps = new PockeTwit.GPS.Gps();

        private IPositionProvider ril = null;

        public LocationManager()
        {
            try
            {
                // Try to load the RILPositionProvider - will fail if no RIL.dll
                ril = System.Reflection.Assembly.GetExecutingAssembly().CreateInstance("PockeTwit.Position.RILPositionProvider") as IPositionProvider;
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }

        }

        public void GetGPS()
        {
            //if (ClientSettings.UseGPS)
            {
                StartGPS();
            }
           
        }

        public void StartGPS()
        {
            position = null;
            if (ClientSettings.UseGPS)
            {
                gps.LocationChanged += new PockeTwit.GPS.LocationChangedEventHandler(gps_LocationChanged);
                if (!gps.Opened)
                {
                    gps.Open();
                }
            }
            if (ril != null)
            {
                ril.PositionChanged += new EventHandler<PositionEventArgs>(RILPositionChanged);
                ril.Enabled = true;
            }
        }
        public void StopGPS()
        {
            gps.LocationChanged -= new PockeTwit.GPS.LocationChangedEventHandler(gps_LocationChanged);
            if (gps.Opened)
            {
                gps.Close();
            }
            if (ril != null)
            {
                ril.PositionChanged -= new EventHandler<PositionEventArgs>(RILPositionChanged);
                ril.Enabled = false;
            }
        }

        private void RILPositionChanged(object sender, PositionEventArgs pe)
        {
            PositionChanged(sender, pe);
            ril.Enabled = false; // got a location through RIL, stop before we loop again
        }

        private void PositionChanged(object sender, PositionEventArgs pe)
        {
            if (LocationReady != null)
            {
                if (pe.status == PositionEventArgs.PositionStatus.Valid)
                {
                    position = pe.position;
                    LocationReady(position, sender== ril ? LocationSource.RIL : LocationSource.GPS);
                }
            }
        }

        void gps_LocationChanged(object sender, PockeTwit.GPS.LocationChangedEventArgs args)
        {
            PositionEventArgs.PositionStatus stat = PositionEventArgs.PositionStatus.Invalid;
            PockeTwit.Position.GeoCoord le = new PockeTwit.Position.GeoCoord(0, 0);

           
            if (gps.Opened)
            {
                try
                {
                    if (args.Position == null) { stat = PositionEventArgs.PositionStatus.Invalid; }
                    if (args.Position.LatitudeValid && args.Position.LongitudeValid)
                    {
                        if (!Double.IsNaN(args.Position.Longitude) && !Double.IsNaN(args.Position.Latitude))
                        {
                            le = new PockeTwit.Position.GeoCoord(args.Position.Latitude, args.Position.Longitude);
                            stat = PositionEventArgs.PositionStatus.Valid;
                        }
                    }
                }
                catch (DivideByZeroException)
                {
                    stat = PositionEventArgs.PositionStatus.Error;
                }

            }
            // once we know where we are precisely, stop looking
            if (stat == PositionEventArgs.PositionStatus.Valid && ril != null)
                ril.Enabled = false; // turn RIL off immediately
            PositionChanged(sender, new PositionEventArgs(stat, le, DateTime.Now));
            if (stat == PositionEventArgs.PositionStatus.Valid)
                StopGPS(); // and stop GPS
        }
    }
}
