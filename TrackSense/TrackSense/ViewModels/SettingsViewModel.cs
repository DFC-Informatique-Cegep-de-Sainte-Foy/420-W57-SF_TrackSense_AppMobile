using CommunityToolkit.Maui.Core.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TrackSense.Configurations;
using TrackSense.Entities;
using TrackSense.Services;
using TrackSense.Services.Bluetooth;

namespace TrackSense.ViewModels
{
    public partial class SettingsViewModel : BaseViewModel
    {
        RideService _rideService;
        UserService _userService;
        BluetoothService _bluetoothService;
        IConfigurationManager _configuration;
        Settings _userSettings;

        [ObservableProperty]
        int screenRotation;

        public SettingsViewModel(RideService rideService, BluetoothService bluetoothService, UserService userService, IConfigurationManager configurationManager)
        {
            Title = "Paramètres";
            _rideService = rideService;
            _bluetoothService = bluetoothService;
            _userService = userService;
            _configuration = configurationManager;
            _userSettings = _configuration.LoadSettings();
            bool isDeviceConnected = this.IsDeviceConnected();
            ScreenRotation = _userSettings.ScreenRotation;

            if (isDeviceConnected)
            {
                ScreenRotation = _bluetoothService.ScreenRotation;
                if (_userSettings.ScreenRotation != ScreenRotation)
                {
                    _userSettings.ScreenRotation = ScreenRotation;
                    _configuration.SaveSettings(_userSettings);
                }
            }
        }

        internal bool IsDeviceConnected()
        {
            return _bluetoothService.GetConnectedDevice() != null;
        }

        [RelayCommand]
        async Task DeleteRidesFromStorageAsync()
        {
            List<CompletedRide> allRides = _rideService.GetAllCompletedRides();
            int totalNumberOfRides = allRides.Count;

            bool choice = await Shell.Current.DisplayAlert("Supprimer les trajets", $"Cette action supprimera {totalNumberOfRides} trajet(s) enregistré(s) dans la mémoire du téléphone. Êtes-vous sûr ?", "Oui", "Non");
            if (choice)
            {
                _rideService.DeleteRidesFromLocalStorage();
            }
        }

        [RelayCommand]
        async Task DisconnectFromDeviceAsync()
        {
            this._bluetoothService.DisconnectDevice();
        }

        [RelayCommand]
        async Task DisplayApiUrlOptionsAsync()
        {
            Grid apiUrlOptions = Shell.Current.CurrentPage.FindByName<Grid>("apiUrlOptions");
            Shell.Current.CurrentPage.FindByName<Entry>("apiUrlEntry").Text = _userSettings.ApiUrl;

            apiUrlOptions.IsVisible = !apiUrlOptions.IsVisible;
        }

        [RelayCommand]
        async Task ChangeApiUrlAsync()
        {
            string modifiedApiUrl = Shell.Current.CurrentPage.FindByName<Entry>("apiUrlEntry").Text;

            Uri uriResult;
            bool uriIsValid = Uri.TryCreate(modifiedApiUrl, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (!uriIsValid)
            {
                await Shell.Current.DisplayAlert("Oups", "Le format de votre URL n'est pas valide", "Ok");
                return;
            }

            if (modifiedApiUrl != _userSettings.ApiUrl)
            {
                _userSettings.ApiUrl = modifiedApiUrl;

                _configuration.SaveSettings(_userSettings);

                await Shell.Current.DisplayAlert("Modification URL", $"Modification de l'URL de l'API pour {modifiedApiUrl}", "Ok");
            }

            await Shell.Current.CurrentPage.FindByName<Entry>("apiUrlEntry").HideKeyboardAsync(CancellationToken.None);

            Shell.Current.CurrentPage.FindByName<Grid>("apiUrlOptions").IsVisible = false;

        }

        [RelayCommand]
        async Task DisplayUsernameOptionsAsync()
        {
            Shell.Current.CurrentPage.FindByName<Entry>("usernameEntry").Text = _userSettings.Username;
            Grid usernameOptions = Shell.Current.CurrentPage.FindByName<Grid>("usernameOptions");
            usernameOptions.IsVisible = !usernameOptions.IsVisible;
        }

        [RelayCommand]
        async Task ChangeUsernameAsync()
        {
            string modifiedUsername = Shell.Current.CurrentPage.FindByName<Entry>("usernameEntry").Text;

            if (String.IsNullOrWhiteSpace(modifiedUsername))
            {
                return;
            }

            string pattern = @"[^a-zA-Z0-9à-ÿ_\-]"; // Allow letters, digits, underscores, hyphens, and accented characters

            string sanitizedUsername = Regex.Replace(modifiedUsername, pattern, "");

            if (String.IsNullOrWhiteSpace(sanitizedUsername))
            {
                await Shell.Current.DisplayAlert("Oups", "Le format de votre nom utilisateur n'est pas valide", "Ok");
                return;
            }

            if (sanitizedUsername != _userSettings.Username)
            {
                _userSettings.Username = sanitizedUsername;

                _configuration.SaveSettings(_userSettings);

                await Shell.Current.DisplayAlert("Changement de compte", $"Vous êtes maintenant connecté en tant que {sanitizedUsername}", "Ok");
            }

            await Shell.Current.CurrentPage.FindByName<Entry>("usernameEntry").HideKeyboardAsync(CancellationToken.None);

            Shell.Current.CurrentPage.FindByName<Grid>("usernameOptions").IsVisible = false;
        }

        [RelayCommand]
        async Task DisplayAccountOptionsAsync()
        {
            Grid accountOptions = Shell.Current.CurrentPage.FindByName<Grid>("accountOptions");
            accountOptions.IsVisible = !accountOptions.IsVisible;
        }

        [RelayCommand]
        async Task CreateAccountAsync()
        {
            string accountUsername = Shell.Current.CurrentPage.FindByName<Entry>("accountEntry").Text;

            if (String.IsNullOrWhiteSpace(accountUsername))
            {
                return;
            }

            string pattern = @"[^a-zA-Z0-9à-ÿ_\-]"; // Allow letters, digits, underscores, hyphens, and accented characters

            string sanitizedUsername = Regex.Replace(accountUsername, pattern, "");

            if (String.IsNullOrWhiteSpace(sanitizedUsername))
            {
                await Shell.Current.DisplayAlert("Oups", "Le format de votre nom utilisateur n'est pas valide", "Ok");
                return;
            }

            if (!await _userService.IsUserLoginAvailable(sanitizedUsername))
            {
                await Shell.Current.DisplayAlert("Oups", "Ce nom utilisateur existe déjà", "Ok");
                return;
            }

            User user = new User()
            {
                UserLogin = sanitizedUsername,
                Password = sanitizedUsername,
                PasswordConfirmed = sanitizedUsername,
                Email = $"{sanitizedUsername}@email.com",
                FirstName = sanitizedUsername,
                LastName = sanitizedUsername
            };

            HttpResponseMessage response = await _userService.CreateUser(user);

            if (!response.IsSuccessStatusCode)
            {
                await Shell.Current.DisplayAlert("Création de compte", $"Une erreur est survenue lors de la création de votre compte. Le nom d'utilisateur existe déjà ou le format n'est pas valide", "Ok");
                return;
            }

            _userSettings.Username = sanitizedUsername;
            _configuration.SaveSettings(_userSettings);

            await Shell.Current.DisplayAlert("Création de compte", $"Votre compte a été créé avec succès. Vous êtes connecté en tant que {sanitizedUsername}", "Ok");
            await Shell.Current.CurrentPage.FindByName<Entry>("accountEntry").HideKeyboardAsync(CancellationToken.None);

            Shell.Current.CurrentPage.FindByName<Grid>("accountOptions").IsVisible = false;
        }

        [RelayCommand]
        async Task ChangeScreenRotationAsync(string rotationId)
        {
            if (!_bluetoothService.BluetoothIsOn())
            {
                return;
            }

            if (_bluetoothService.GetConnectedDevice() is null)
            {
                return;
            }

            bool isConfirmed = false;   

            while (!isConfirmed) // politique de réessai à ajouter
            {
                isConfirmed = await _bluetoothService.SetScreenRotation(int.Parse(rotationId));
            }

            if (isConfirmed)
            {
                ScreenRotation = int.Parse(rotationId);
                _userSettings.ScreenRotation = ScreenRotation;
                _configuration.SaveSettings(_userSettings);
            }
        }
        [RelayCommand]
        async Task DisplayEndpointAsync()
        {
            Shell.Current.CurrentPage.FindByName<Entry>("endpointEntry").Text = _userSettings.Endpoint;
            Grid endpointOptions = Shell.Current.CurrentPage.FindByName<Grid>("endpointOptions");
            endpointOptions.IsVisible = !endpointOptions.IsVisible;
        }

        [RelayCommand]
        async Task ChangeEndpointAsync()
        {
            string modifiedEndpoint = Shell.Current.CurrentPage.FindByName<Entry>("endpointEntry").Text;

            if (String.IsNullOrWhiteSpace(modifiedEndpoint))
            {
                return;
            }

            if (modifiedEndpoint != _userSettings.Endpoint)
            {
                _userSettings.Endpoint = modifiedEndpoint;

                _configuration.SaveSettings(_userSettings);

                await Shell.Current.DisplayAlert("Modification de l'endpoint", $"Modification de l'endpoint pour {modifiedEndpoint}", "Ok");
            }

            await Shell.Current.CurrentPage.FindByName<Entry>("endpointEntry").HideKeyboardAsync(CancellationToken.None);

            Shell.Current.CurrentPage.FindByName<Grid>("endpointOptions").IsVisible = false;
        }
        [RelayCommand]
        async Task DisplayAccessKeyAsync()
        { 
            Shell.Current.CurrentPage.FindByName<Entry>("accessKeyEntry").Text = _userSettings.AccessKey;
            Grid accessKeyOptions = Shell.Current.CurrentPage.FindByName<Grid>("accessKeyOptions");
            accessKeyOptions.IsVisible = !accessKeyOptions.IsVisible;
        }

        [RelayCommand]
        async Task ChangeAccessKeyAsync()
        {
            string modifiedAccessKey = Shell.Current.CurrentPage.FindByName<Entry>("accessKeyEntry").Text;

            if (String.IsNullOrWhiteSpace(modifiedAccessKey))
            {
                return;
            }

            if (modifiedAccessKey != _userSettings.AccessKey)
            {
                _userSettings.AccessKey = modifiedAccessKey;

                _configuration.SaveSettings(_userSettings);

                await Shell.Current.DisplayAlert("Modification de la clé d'accès", $"Modification de la clé d'accès pour {modifiedAccessKey}", "Ok");
            }

            await Shell.Current.CurrentPage.FindByName<Entry>("accessKeyEntry").HideKeyboardAsync(CancellationToken.None);

            Shell.Current.CurrentPage.FindByName<Grid>("accessKeyOptions").IsVisible = false;
        }

        [RelayCommand]
        async Task DisplaySecretKeyAsync()
        {
            Shell.Current.CurrentPage.FindByName<Entry>("secretKeyEntry").Text = _userSettings.SecretKey;
            Grid secretKeyOptions = Shell.Current.CurrentPage.FindByName<Grid>("secretKeyOptions");
            secretKeyOptions.IsVisible = !secretKeyOptions.IsVisible;
        }

        [RelayCommand]
        async Task ChangeSecretKeyAsync()
        {
            string modifiedSecretKey = Shell.Current.CurrentPage.FindByName<Entry>("secretKeyEntry").Text;

            if (String.IsNullOrWhiteSpace(modifiedSecretKey))
            {
                return;
            }

            if (modifiedSecretKey != _userSettings.SecretKey)
            {
                _userSettings.SecretKey = modifiedSecretKey;

                _configuration.SaveSettings(_userSettings);

                await Shell.Current.DisplayAlert("Modification de la clé secrète", $"Modification de la clé secrète pour {modifiedSecretKey}", "Ok");
            }

            await Shell.Current.CurrentPage.FindByName<Entry>("secretKeyEntry").HideKeyboardAsync(CancellationToken.None);

            Shell.Current.CurrentPage.FindByName<Grid>("secretKeyOptions").IsVisible = false;
        }
    }
}
