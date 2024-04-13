using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackSense.Entities;

namespace TrackSense.Services.API.APIDTO
{
    public class PlannedRideSummaryDTO
    {
        public string PlannedRideId { get; set; }
        public string PlannedRideName { get; set; }
        //public TimeSpan AvgDuration { get; set; }
        public double Distance { get; set; }

        public PlannedRideSummaryDTO()
        {
            ;
        }


        public PlannedRideSummary ToEntity()
        {
            return new PlannedRideSummary()
            {
                PlannedRideId = new Guid(this.PlannedRideId),
                PlannedRideName = this.PlannedRideName,
                //AvgDuration = this.AvgDuration,
                Distance = this.Distance
            };  
        }
    }
}
