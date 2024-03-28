using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackSense.Models
{
    public class PlannedRideSummary
    {
        public Guid PlannedRideId { get; set; }
        public string PlannedRideName { get; set; }
        public TimeSpan? AvgDuration { get; set; }
        public double Distance { get; set; }

        public PlannedRideSummary(Entities.PlannedRideSummary entite)
        {
            if (entite is null)
            {
                throw new ArgumentNullException(nameof(entite));
            }

            PlannedRideId = entite.PlannedRideId;
            PlannedRideName = entite.PlannedRideName;
            AvgDuration = entite.AvgDuration;
            Distance = entite.Distance;
        }
    }
}
