using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrackSense.Models;
using TrackSense.Services.Bluetooth;

namespace TrackSense.ViewModels
{
    [QueryProperty("PlannedRide", "PlannedRide")]
    public partial class PlannedRideStatisticsViewModel : BaseViewModel
    {
        public BluetoothService _bluetoothService;

        [ObservableProperty]
        PlannedRide plannedRide;

        public PlannedRideStatisticsViewModel(BluetoothService p_BLEService)
        {
            Title = "Détail Trajet Planifié";
            _bluetoothService = p_BLEService;
        }

        [RelayCommand]
        async Task Envoyer()
        {
            Trajet trajet = Trajet.FromPlannedRide2Trajet(this.PlannedRide);
            string nom = trajet.nom;
            string json = trajet.FromTrajet2Json();
            await Shell.Current.DisplayAlert(nom, json, "OK"); //
                                                               //TODO: envoyer JSON par BLE

        }

        [RelayCommand]
        async Task EnvoyerJSON()
        {
            //Preparer json
            Trajet trajet = Trajet.FromPlannedRide2Trajet(this.PlannedRide);
            string nom = trajet.nom;
            string json = trajet.FromTrajet2Json();
            await Shell.Current.DisplayAlert(nom, json, "OK");
            //Verifier connection BLE
            if (!_bluetoothService.BluetoothIsOn())
            {
                return;
            }

            if (_bluetoothService.GetConnectedDevice() is null)
            {
                return;
            }

            bool isConfirmed = false;

            while (!isConfirmed) 
            {
                //Essayer d'envoyer json
                isConfirmed = await _bluetoothService.EnvoyerTrajet(json); 
            }

            if (isConfirmed)
            {
                 await Shell.Current.DisplayAlert("Success", "Trajet est envoyé!", "OK");

            }
        }



    }
}
