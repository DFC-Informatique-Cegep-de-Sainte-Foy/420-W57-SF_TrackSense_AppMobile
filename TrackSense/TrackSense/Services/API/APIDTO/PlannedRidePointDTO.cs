using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackSense.Entities;

namespace TrackSense.Services.API.APIDTO
{
    public class PlannedRidePointDTO
    {
        public string PlannedRideId { get; set; }=string.Empty!;
        public LocationDTO Location { get; set; } = null!;
        public int RideStep { get; set; } =0;
        public double? Temperature { get; set; } = 0;
        public DateTime DateTime { get; set; }
        public PlannedRidePointDTO(PlannedRidePoint entite)
        {
            RideStep = entite.RideStep;
            Location = new LocationDTO( entite.Location);
            Temperature = entite.Temperature;
            DateTime = entite.Location.Timestamp.DateTime;
        }

        public PlannedRidePointDTO()
        {
            ;
        }

        public PlannedRidePoint ToEntity()
        {
            return new PlannedRidePoint()
            {
                Location = this.Location.ToEntity(),
                RideStep= this.RideStep,
                Temperature = this.Temperature,
            };
        }
    }
}
