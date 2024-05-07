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
        public string PlannedRideName { get; set; }
        public List<PlannedRidePoint> PlannedRidePoints { get; set; }

        public PlannedRide()
        {
            PlannedRideId = new Guid();
            PlannedRideName = "";
            PlannedRidePoints = new List<PlannedRidePoint>();
        }

        public PlannedRide(Entities.PlannedRide entite)
        {
            if (entite is null)
            {
                throw new ArgumentNullException(nameof(entite));
            }

            this.PlannedRideId = entite.PlannedRideId;
            this.PlannedRidePoints = entite.PlannedRidePoints.Select(entite => new PlannedRidePoint(entite)).ToList();
        }

        public Entities.PlannedRide ToEntity()
        {
            return new Entities.PlannedRide()
            {
                PlannedRideId = this.PlannedRideId,
                PlannedRideName = this.PlannedRideName,
                PlannedRidePoints = this.PlannedRidePoints.Select(p => p.ToEntity()).ToList(),
            };
        }
    }
}
