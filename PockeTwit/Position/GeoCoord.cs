using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace PockeTwit.Position
{
    public class GeoCoord
    {
        public GeoCoord(double Lat, double Lon)
        {
            this.Lat = Lat;
            this.Lon = Lon;
            this.Accuracy = 0;
        }

        public GeoCoord(double Lat, double Lon, double Accuracy)
        {
            this.Lat = Lat;
            this.Lon = Lon;
            this.Accuracy = Accuracy;
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", Lat.ToString(CultureInfo.InvariantCulture), Lon.ToString(CultureInfo.InvariantCulture));
        }

        /*public string LatS
        {
            get
            {
                return Lat.ToString(CultureInfo.InvariantCulture);
            }
        }
        public string LonS
        {
            get
            {
                return Lon.ToString(CultureInfo.InvariantCulture);
            }
        }*/

        public double Lat;
        public double Lon;
        public double Accuracy;
    }
}
