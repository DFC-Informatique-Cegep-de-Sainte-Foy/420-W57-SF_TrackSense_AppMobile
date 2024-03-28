using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackSense.Models
{
    public class PlannedRidePoint
    {
        public int RideStep { get; set; }
        public Location Location { get; set; }
        public double? Temperature { get; set; }
        public TimeSpan? EffectiveTime { get; set; }

        public PlannedRidePoint(int rideStep, Location location, double temperature, TimeSpan effectiveTime)
        {
            RideStep = rideStep;
            Location = location;
            Temperature = temperature;
            EffectiveTime = effectiveTime;
        }
        public PlannedRidePoint()
        {
            ;
        }

        public PlannedRidePoint(TrackSense.Entities.PlannedRidePoint entite)
        {
            RideStep = entite.RideStep;
            Location = entite.Location;
            Temperature = entite.Temperature;
            EffectiveTime = entite.EffectiveTime;
        }
    }
}
