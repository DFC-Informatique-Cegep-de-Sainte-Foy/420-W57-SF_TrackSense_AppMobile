using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackSense.Configurations;
using TrackSense.Entities;
using TrackSense.Services;

namespace TrackSense.ViewModels
{
    public partial class GallerieViewModel : BaseViewModel
    {
        IConfigurationManager _configuration;
        GallerieService _gallerieService;

        public GallerieViewModel(IConfigurationManager configurationManager, GallerieService gallerieService)
        {
            
            _configuration = configurationManager;
            _gallerieService = gallerieService;
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
        async Task PrendrePhotoAsync()
        {
            await _gallerieService.PrendrePhoto();
        }

        [RelayCommand]
        async Task OuvrirGallerieAsync()
        {
            await _gallerieService.OuvrirGallerie();
        }
    }
}
