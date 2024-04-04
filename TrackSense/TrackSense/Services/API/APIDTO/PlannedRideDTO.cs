using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using TrackSense.Entities;

namespace TrackSense.Services.API.APIDTO
{
    public class PlannedRideDTO
    {
        public string UserLogin { get; set; }
        public string? PlannedRideId { get; set; }
        public List<PlannedRidePointDTO> PlannedRidePoints { get; set; }
        public PlannedRideStatisticsDTO Statistics { get; set; }

        public PlannedRideDTO()
        {
            ;
        }
        public PlannedRideDTO(Entities.PlannedRide p_plannedRide)
        {
            if (p_plannedRide == null)
            {
                throw new NullReferenceException(nameof(p_plannedRide));
            }
            if (p_plannedRide.UserLogin == null)
            {
                throw new NullReferenceException(nameof(p_plannedRide.UserLogin));
            }
            if (p_plannedRide.PlannedRideId == Guid.Empty)
            {
                throw new InvalidOperationException("Id du CompletedRide ne doit pas être null ni vide");
            }

            this.UserLogin = p_plannedRide.UserLogin;
            this.PlannedRideId = p_plannedRide.PlannedRideId.ToString();

            this.PlannedRidePoints = p_plannedRide.PlannedRidePoints.Select(entite =>
                                                                                    new PlannedRidePointDTO(entite)
                                                                                    {
                                                                                        CompletedRideId = this.CompletedRideId
                                                                                    })
                                                                           .ToList();
        }

        public Entities.PlannedRide ToEntity()
        {
            return new PlannedRide()
            {
                PlannedRideId = new Guid(this.PlannedRideId!),
                PlannedRidePoints = this.PlannedRidePoints.Select(p => p.ToEntity()).ToList(),
                UserLogin = this.UserLogin,
                Statistics = this.Statistics?.ToEntity(),
            };
        }
    }
}
