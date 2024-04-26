using CommunityToolkit.Mvvm.ComponentModel;
using TrackSense.Configurations;
using TrackSense.Models;
using TrackSense.Services;

namespace TrackSense.ViewModels
{
    [QueryProperty("CreateNewPlannedRide", "CreateNewPlannedRide")]
    public partial class CreateNewPlannedRideViewModel : BaseViewModel
    {
        RideService _rideService;
        IConnectivity _connectivity;
        PlannedRide plannedRide;

        [ObservableProperty]
        bool isConnected;


        public CreateNewPlannedRideViewModel(IConnectivity connectivity, RideService rideService)
        {
            Title = "CreateNewPlannedRide";
            _connectivity = connectivity;
            _rideService = rideService;
        }


        async Task CreateNewPlannedRide()
        {
            // setup le PlannedRide de type Model
            Entities.PlannedRide plannedRideEntity = plannedRide.ToEntity();
            _rideService.PostPlannedRideAsync(plannedRideEntity);
        }

        private async Task<bool> CheckInternetConnection()
        {
            bool internetIsAvailable = _connectivity.NetworkAccess == NetworkAccess.Internet;

            if (!internetIsAvailable)
            {
                await Shell.Current.DisplayAlert("Problème de connexion à internet", "Veuillez vérifier votre connexion à internet puis réessayer", "Ok");
            }

            return internetIsAvailable;
        }
    }
}
