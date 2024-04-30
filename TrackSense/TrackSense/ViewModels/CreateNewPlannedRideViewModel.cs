using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrackSense.Configurations;
using TrackSense.Models;
using TrackSense.Services;

namespace TrackSense.ViewModels
{
    [QueryProperty("NewPlannedRide", "NewPlannedRide")]
    public partial class CreateNewPlannedRideViewModel : BaseViewModel
    {
        RideService _rideService;
        public IConnectivity _connectivity;
        IGeolocation _geolocation;

        [ObservableProperty]
        PlannedRide newPlannedRide = new();

        [ObservableProperty]
        bool isConnected;

        public CreateNewPlannedRideViewModel(IConnectivity connectivity, IGeolocation geolocation, RideService rideService)
        {
            Title = "Nouveau trajet planifié";
            _connectivity = connectivity;
            _geolocation = geolocation;
            _rideService = rideService;
        }

        [RelayCommand]
        async Task CreateNewPlannedRide()
        {
            // setup le PlannedRide de type Model --> possiblement executer dans CreateNewPlannedRidePage.xaml.cs
            Entities.PlannedRide plannedRideEntity = newPlannedRide.ToEntity();
            _rideService.PostPlannedRideAsync(plannedRideEntity);
        }

        [RelayCommand]
        public async Task<Location> GetCurrentLocation()
        {
            return _geolocation.GetLocationAsync().Result;
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
