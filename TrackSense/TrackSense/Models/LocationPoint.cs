using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackSense.Models
{
    public class LocationPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public LocationPoint(double p_latitude,double p_longitude)
        {
            this.Latitude = p_latitude;
            this.Longitude = p_longitude;
        }


    }
}
