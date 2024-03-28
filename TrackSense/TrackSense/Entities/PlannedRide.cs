using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackSense.Entities
{
    public class PlannedRide
    {
        public string UserLogin { get; set; } = null!;
        public Guid PlannedRideId { get; set; } = Guid.Empty!;
        public List<PlannedRidePoint> PlannedRidePoints { get; set; } = null!;
        //public PlannedRideStatistics? Statistics { get; set; } = null;
    }
}
