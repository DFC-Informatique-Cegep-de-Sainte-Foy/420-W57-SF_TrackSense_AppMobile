namespace TrackSense.Models
{
    public class PlannedRideStatistics
    {
        public double AverageSpeed { get; set; }
        public double MaximumSpeed { get; set; }
        public double Distance { get; set; }
        public TimeSpan Duration { get; set; }
        public int NumberOfPoints { get; set; }
        public int Calories { get; set; }
        public int Falls { get; set; }


        public PlannedRideStatistics()
        {
            ;
        }

        public PlannedRideStatistics(TrackSense.Entities.PlannedRideStatistics entite)
        {
            MaximumSpeed = entite.MaximumSpeed;
            AverageSpeed = entite.AverageSpeed;
            Distance = entite.Distance;
            Duration = entite.Duration;
            NumberOfPoints = entite.NumberOfPoints;
            Calories = entite.Calories;
            Falls = entite.Falls;
        }

        internal PlannedRideStatistics ToEntity()
        {
            return new PlannedRideStatistics()
            {
                MaximumSpeed = this.MaximumSpeed,
                AverageSpeed = this.AverageSpeed,
                Distance = this.Distance,
                Duration = this.Duration,
                NumberOfPoints = this.NumberOfPoints,
                Calories = this.Calories,
                Falls = this.Falls
            };
        }
    }
}