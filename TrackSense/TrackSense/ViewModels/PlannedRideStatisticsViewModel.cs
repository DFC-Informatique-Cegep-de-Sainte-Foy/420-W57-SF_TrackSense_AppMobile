using CommunityToolkit.Mvvm.ComponentModel;
using TrackSense.Models;

namespace TrackSense.ViewModels
{
    [QueryProperty("PlannedRide", "PlannedRide")]
    public partial class PlannedRideStatisticsViewModel : BaseViewModel
    {
        [ObservableProperty]
        PlannedRide plannedRide;

        public PlannedRideStatisticsViewModel()
        {
            Title = "Détail Trajet Planifié";
        }
    }
}
