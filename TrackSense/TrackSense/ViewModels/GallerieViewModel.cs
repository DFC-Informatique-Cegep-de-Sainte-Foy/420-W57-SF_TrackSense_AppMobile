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
        UserService _userService;
        IConfigurationManager _configuration;
        Settings _userSettings;
        GallerieService _gallerieService;

        public GallerieViewModel(UserService userService, IConfigurationManager configurationManager, GallerieService gallerieService)
        {
            Title = "Gallerie";
            _userService = userService;
            _configuration = configurationManager;
            _gallerieService = gallerieService;
            _userSettings = _configuration.LoadSettings();
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
