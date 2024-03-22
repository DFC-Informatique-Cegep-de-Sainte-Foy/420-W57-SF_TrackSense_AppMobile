using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackSense.Entities
{
    public class SavedRideSummary
    {
        public Guid SavedRideId { get; set; }
        public string SavedRideName { get; set; }
        public TimeSpan AvgDuration { get; set; }
        public double Distance { get; set; }
    }
}
