using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        [RelayCommand]
        async Task Envoyer()
        {
            Trajet trajet = Trajet.FromPlannedRide2Trajet(this.PlannedRide);
            string nom = trajet.Nom;
            string json = trajet.FromTrajet2Json();
            await Shell.Current.DisplayAlert(nom, json, "OK"); //
            //TODO: envoyer JSON par BLE
           
        }

    }
}
