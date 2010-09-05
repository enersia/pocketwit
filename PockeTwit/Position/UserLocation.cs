using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace PockeTwit.Position
{
    public enum PlaceType
    {
        Poi, // place of interest
        Neighbourhood, // Neighbourhood
        City, // City 
        Administrative, // Administrative division
        Country // Country
    }

    public class Place
    {
        public virtual string DisplayName { get; set; }
        public virtual GeoCoord Position { get; set; }
        public virtual PlaceType Type { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }

    }

    public class UserLocation
    {
        // The current "place", if the server supports it, else null
        public Place Location { get; set; }

        public GeoCoord Position { get; set; }

    }
}
