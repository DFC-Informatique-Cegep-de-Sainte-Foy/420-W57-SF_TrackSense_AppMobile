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
        public string? PlannedRideName { get; set; }
        public double? Distance { get; set; }
        public List<PlannedRidePointDTO> PlannedRidePoints { get; set; }

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
                throw new InvalidOperationException("Id du PlannedRide ne doit pas être null ni vide");
            }
            if (p_plannedRide.PlannedRideName == String.Empty)
            {
                throw new InvalidOperationException("Nom du PlannedRide ne doit pas être null ni vide");
            }

            this.UserLogin = p_plannedRide.UserLogin;
            this.PlannedRideId = p_plannedRide.PlannedRideId.ToString();
            this.PlannedRideName = p_plannedRide.PlannedRideName;

            this.PlannedRidePoints = p_plannedRide.PlannedRidePoints.Select(entite =>
                                                                                    new PlannedRidePointDTO(entite)
                                                                                    {
                                                                                        PlannedRideId = this.PlannedRideId
                                                                                    })
                                                                           .ToList();
        }

        public Entities.PlannedRide ToEntity()
        {
            return new PlannedRide()
            {
                PlannedRideId = new Guid(this.PlannedRideId!),
                PlannedRideName = this.PlannedRideName,
                Distance = this.Distance,
                PlannedRidePoints = this.PlannedRidePoints.Select(p => p.ToEntity()).ToList(),
                UserLogin = this.UserLogin,
            };
        }
    }
}
