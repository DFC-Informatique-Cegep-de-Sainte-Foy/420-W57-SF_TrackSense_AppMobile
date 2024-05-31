using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrackSense.Configurations;
using TrackSense.Models;
using TrackSense.Services;
using TrackSense.Views;

namespace TrackSense.ViewModels
{
    [QueryProperty("NewPlannedRide", "NewPlannedRide")]
    public partial class CreateNewPlannedRideViewModel : BaseViewModel
    {
        RideService _rideService;
        UserService _userService;
        IConnectivity _connectivity;
        LocationService _locationService;

        [ObservableProperty]
        PlannedRide newPlannedRide;

        [ObservableProperty]
        bool isConnected;

        public CreateNewPlannedRideViewModel(IConnectivity connectivity, RideService rideService, UserService userService)
        {
            Title = "Créer Nouveau Trajet";
            _connectivity = connectivity;
            _rideService = rideService;
            newPlannedRide = new PlannedRide();
            _userService = userService;
        }

        [RelayCommand]
        async Task CreateNewPlannedRideAsync()
        {
            Entities.PlannedRide plannedRideEntity = newPlannedRide.ToEntity();
            plannedRideEntity.PlannedRideId = Guid.NewGuid();
            plannedRideEntity.UserLogin = await _userService.GetCurrentUser();
            _rideService.PostPlannedRideAsync(plannedRideEntity);
        }

        [RelayCommand]
        async Task GoToMainMenuAsync()
        {
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
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
