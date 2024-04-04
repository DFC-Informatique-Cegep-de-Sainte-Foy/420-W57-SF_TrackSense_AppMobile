using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackSense.Entities
{
    public interface IPlannedRideLocalData
    {
        void AddPlannedRide(PlannedRide plannedRide);
        List<PlannedRide> ListPlannedRides();
        PlannedRide GetPlannedRideById(Guid plannedRideId);
        void DeletePlannedRideById(Guid plannedRideId);
        void DeleteAllPlannedRides();

    }
}
