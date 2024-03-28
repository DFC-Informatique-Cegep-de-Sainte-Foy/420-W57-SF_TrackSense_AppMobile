using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackSense.Models
{
    public class CompletedRide
    {
        public Guid CompletedRideId { get; set; }
        public Guid PlannedRideId { get; set; }
        public List<CompletedRidePoint> CompletedRidePoints { get; set; }
        public CompletedRideStatistics Statistics { get; set; }

        public CompletedRide(Entities.CompletedRide entite)
        {
            if (entite is null)
            {
                throw new ArgumentNullException(nameof(entite));
            }

            this.CompletedRideId = entite.CompletedRideId;
            this.PlannedRideId = entite.PlannedRideId;
            this.CompletedRidePoints = entite.CompletedRidePoints.Select(entite => new CompletedRidePoint(entite)).ToList();
            this.Statistics = new CompletedRideStatistics(entite.Statistics);
        }
    }
}
