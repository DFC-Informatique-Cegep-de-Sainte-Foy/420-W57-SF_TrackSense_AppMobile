using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackSense.Models;

namespace TrackSense.Entities
{
    public class CompletedRide
    {
        public string UserLogin { get; set; } = null!;
        public Guid CompletedRideId { get; set; } = Guid.Empty!;
        public Guid PlannedRideId { get; set; }
        [JsonIgnore]
        public PlannedRide? PlannedRide { get; set; } = null;
        public List<CompletedRidePoint> CompletedRidePoints { get; set; } = null!;
        public CompletedRideStatistics? Statistics { get; set; } = null;
      
    }
}
