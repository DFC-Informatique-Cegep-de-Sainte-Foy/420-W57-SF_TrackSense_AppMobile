namespace TrackSense.Entities
{
    public class PlannedRidePoint
    {
        public int RideStep { get; set; }
        public Location Location { get; set; }
        public double? Temperature { get; set; }
        public TimeSpan? EffectiveTime { get; set; }


        public PlannedRidePoint()
        {
            ;
        }
        public PlannedRidePoint(int rideStep, Location location, double? temperature, TimeSpan? effectiveTime)
        {
            RideStep = rideStep;
            Location = location;
            Temperature = temperature;
            EffectiveTime = effectiveTime;
        }
    }
}