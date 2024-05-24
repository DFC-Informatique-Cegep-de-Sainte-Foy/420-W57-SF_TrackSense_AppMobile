using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackSense.Entities
{
    public class PlannedRideSummary
    {
        public Guid PlannedRideId { get; set; }
        public string PlannedRideName { get; set; }
        public TimeSpan? AvgDuration { get; set; }
        public double Distance { get; set; }
    }
}
