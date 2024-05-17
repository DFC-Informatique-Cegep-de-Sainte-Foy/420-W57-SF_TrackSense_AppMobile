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
        public double? Distance { get; set; }
        public List<PlannedRidePoint> PlannedRidePoints { get; set; }

        public PlannedRide()
        {
            PlannedRideId = new Guid();
            PlannedRideName = "";
            Distance = 0.0;
            PlannedRidePoints = new List<PlannedRidePoint>();
        }

        public PlannedRide(Entities.PlannedRide entite)
        {
            if (entite is null)
            {
                throw new ArgumentNullException(nameof(entite));
            }

            this.PlannedRideId = entite.PlannedRideId;
            this.PlannedRideName= entite.PlannedRideName;
            this.Distance = entite.Distance;
            this.PlannedRidePoints = entite.PlannedRidePoints.Select(entite => new PlannedRidePoint(entite)).ToList();
        }

        public Entities.PlannedRide ToEntity()
        {
            return new Entities.PlannedRide()
            {
                PlannedRideId = this.PlannedRideId,
                PlannedRideName = this.PlannedRideName,
                Distance = this.Distance,
                PlannedRidePoints = this.PlannedRidePoints.Select(p => p.ToEntity()).ToList(),
            };
        }

        public override string ToString()
        {
            return $"\nPlanned Ride : \nID : {PlannedRideId}\nNom : {PlannedRideName}\nDistance : {Distance}\nNombre de points : {PlannedRidePoints.Count}";
        }
    }
}
