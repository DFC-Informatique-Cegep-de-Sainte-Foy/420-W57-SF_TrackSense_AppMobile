using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackSense.Models
{
    public class SavedRideSummary
    {
        public Guid SavedRideId { get; set; }
        public string SavedRideName { get; set; }
        public TimeSpan AvgDuration { get; set; }
        public double Distance { get; set; }

        public SavedRideSummary(Entities.SavedRideSummary entite)
        {
            if (entite is null)
            {
                throw new ArgumentNullException(nameof(entite));
            }

            SavedRideId = entite.SavedRideId;
            SavedRideName = entite.SavedRideName;
            AvgDuration = entite.AvgDuration;
            Distance = entite.Distance;
        }
    }
}
