using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackSense.Models
{
    public class PlannedRide
    {
        public Guid PlannedRideId { get; set; }
        public Guid PlannedRideName { get; set; }
        public List<PlannedRidePoint> PlannedRidePoints { get; set; }
        public PlannedRideStatistics Statistics { get; set; }
        public PlannedRide(Entities.PlannedRide entite)
        {
            if (entite is null)
            {
                throw new ArgumentNullException(nameof(entite));
            }

            this.PlannedRideId = entite.PlannedRideId;
            this.PlannedRidePoints = entite.PlannedRidePoints.Select(entite => new PlannedRidePoint(entite)).ToList();
            this.Statistics = new PlannedRideStatistics(entite.Statistics);
        }
    }
}
