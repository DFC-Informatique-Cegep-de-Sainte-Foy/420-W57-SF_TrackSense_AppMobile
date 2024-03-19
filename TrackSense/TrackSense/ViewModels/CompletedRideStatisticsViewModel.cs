using CommunityToolkit.Mvvm.ComponentModel;
using TrackSense.Models;

namespace TrackSense.ViewModels
{
    [QueryProperty("CompletedRide", "CompletedRide")]
    public partial class CompletedRideStatisticsViewModel : BaseViewModel
    {
        [ObservableProperty]
        CompletedRide completedRide;

        public CompletedRideStatisticsViewModel()
        {
            Title = "Statistiques";
        }
    }
}
