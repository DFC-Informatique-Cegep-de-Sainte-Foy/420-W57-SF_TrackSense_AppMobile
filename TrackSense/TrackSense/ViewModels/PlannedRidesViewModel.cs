using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TrackSense.Models;
using TrackSense.Services;
using TrackSense.Services.Bluetooth;
using TrackSense.Views;

namespace TrackSense.ViewModels;

public partial class PlannedRidesViewModel : BaseViewModel
{
    BluetoothService _bluetoothService;
    RideService _rideService;
    IConnectivity _connectivity;
    public ObservableCollection<PlannedRideSummary> PlannedRideSummaries { get; } = new();

    [ObservableProperty]
    bool isConnected;

    [ObservableProperty]
    bool isReceivingData;

    [ObservableProperty]
    bool isRefreshing;

    public PlannedRidesViewModel(BluetoothService btService, RideService rideService, IConnectivity connectivity)
    {
        Title = "Tracksense";
        _bluetoothService = btService;
        _rideService = rideService;
        _connectivity = connectivity;

        BluetoothObserver bluetoothObserver = new BluetoothObserver(this._bluetoothService,
            async (value) =>
            {
                switch (value.Type)
                {
                    case BluetoothEventType.CONNECTION:
                        IsConnected = true;
                        break;
                    case BluetoothEventType.DECONNECTION:
                        IsConnected = false;
                        this._rideService.InterruptReception();
                        break;
                    case BluetoothEventType.SENDING_RIDE_STATS:
                        await this._rideService.ReceiveRideDataFromDevice(value.RideData);
                        IsReceivingData = this._bluetoothService.IsReceiving;
                        break;
                    case BluetoothEventType.SENDING_RIDE_POINT:
                        await this._rideService.ReceivePointDataFromDevice(value.RidePoint);
                        IsReceivingData = this._bluetoothService.IsReceiving;
                        break;
                    default:
                        break;
                }
            });

        InitializeDisplay();
    }

    private async void InitializeDisplay()
    {
        await this.GetPlannedRidesAsync();
    }

    [RelayCommand]
    async Task PostPlannedRideAsync(Entities.PlannedRide p_plannedRide)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            
            IsBusy = true;

            var result = await _rideService.PostPlannedRideAsync(p_plannedRide);

            if (result.IsSuccessStatusCode)
            {
                // Handle success, e.g., show a success message to the user
                await Shell.Current.DisplayAlert("Success", "Trajet publié avec succès.", "Ok");
            }
            else
            {
                // Handle failure, e.g., show an error message to the user
                await Shell.Current.DisplayAlert("Error", "Échec de la publication du trajet.", "Ok");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);

            // Handle and display the exception to the user
            await Shell.Current.DisplayAlert("Error", $"An error occurred while posting the planned ride: {ex.Message}", "Ok");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    async Task GoToSettingsAsync()
    {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }

    [RelayCommand]
    async Task GoToTrackSenseDevices()
    {
        this.IsConnected = this._bluetoothService.GetConnectedDevice() is not null;

        if (!this.IsConnected)
        {
            await Shell.Current.GoToAsync(nameof(TrackSenseDevicesPage));
        }
    }

    [RelayCommand]
    async Task GoToDetailsAsync(PlannedRideSummary rideSummary)
    {
        if (rideSummary is null)
        {
            return;
        }

        try
        {
            if (await CheckInternetConnection())
            {
                
                Entities.PlannedRide plannedRide = await _rideService.GetPlannedRide(rideSummary.PlannedRideId);
                //Entities.CompletedRide completedRide = GenerateFakeCompletedRide();
                //Entities.CompletedRide completedRide = _rideService.GetCompletedRideFromLocalStorage(rideSummary.CompletedRideId

                //Référence le shell, donc pas bonne pratique, il faudrait une interface.

                // ICI EST LA REDIRECTION A LA PAGE STATISTIQUES QUAND ON CLICK SUR LE SUMMARY (A AJOUTER DANS NEXT USERSTORY)
                /*await Shell.Current.GoToAsync($"{nameof(PlannedRideStatisticsPage)}", true,
                    new Dictionary<string, object>
                    {
                            {"PlannedRide", new Models.PlannedRide(plannedRide) }
                    });*/
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            await Shell.Current.DisplayAlert("Oups", "Une erreur est survenue lors de la récupération du trajet sauvegarder", "Ok");
        }

    }

    [RelayCommand]
    public async Task GetPlannedRidesAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {

            if (await CheckInternetConnection())
            {
                IsBusy = true;
                IsRefreshing = true;

                List<TrackSense.Entities.PlannedRideSummary> plannedRides = await _rideService.GetUserPlannedRides();
                //List<TrackSense.Entities.CompletedRideSummary> completedRides = _rideService.GetCompletedRideSummariesFromLocalStorage();


                if (PlannedRideSummaries.Count != 0)
                {
                    PlannedRideSummaries.Clear();
                }
                
                foreach (TrackSense.Entities.PlannedRideSummary ride in plannedRides)
                {
                    PlannedRideSummaries.Add(new PlannedRideSummary(ride)); // si on a trop de données, ne pas utiliser cette méthode car lève un évenement pour chaque ajout
                    // si on a trop de données, créer une nouvelle liste ou une nouvelle ObservableCollection et l'assigner à CompletedRides ou trouver des
                    //library helpers qui ont des observableRange collections qui feront de l'ajout de batch
                }
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            await Shell.Current.DisplayAlert("Erreur", $"Une erreur est survenue lors de la récupération des trajets: {ex.Message}", "Ok");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
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
