using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TrackSense.Configurations;
using TrackSense.Entities;
using TrackSense.Services;

namespace TrackSense.ViewModels
{
    public partial class GallerieImagesViewModel : BaseViewModel
    {
        private readonly IConfigurationManager _configuration;
        private readonly GallerieService _gallerieService;

        public ObservableCollection<string> ImagePaths { get; set; }
        public GallerieImagesViewModel(IConfigurationManager configurationManager, GallerieService gallerieService)
        {
            _configuration = configurationManager;
            _gallerieService = gallerieService;

            ImagePaths = new ObservableCollection<string>();

            var user = _configuration.LoadSettings();
            var nom = user.Username;
            Title = "Gallerie de " + nom;

            _configuration.ConfigurationChanged += ConfigurationChanged;
        }

        private void ConfigurationChanged(object sender, EventArgs e)
        {
            var user = _configuration.LoadSettings();
            var nom = user.Username;
            Title = "Gallerie de " + nom;
        }
        [RelayCommand]
        private async Task LoadImages()
        {
            try
            {
                //await Shell.Current.DisplayAlert("","Loading images...","OK");
                await AfficherImages();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading images: {ex.Message}");
            }
        }

        private async Task AfficherImages()
        {
            var bucketName = _configuration.LoadSettings().Username;
            var appFolderPath = Path.Combine(FileSystem.AppDataDirectory, "images", bucketName);
            var files = Directory.GetFiles(appFolderPath);

            ImagePaths.Clear();

            if (files.Length == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Gallerie", "Aucune image à afficher", "OK");
                return;
            }

            foreach (var file in files)
            {
                ImagePaths.Add(file);
            }
        }
    }
}