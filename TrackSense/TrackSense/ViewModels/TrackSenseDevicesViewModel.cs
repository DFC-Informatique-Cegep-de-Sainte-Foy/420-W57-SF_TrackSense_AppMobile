using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using TrackSense.Models;
using TrackSense.Services.Bluetooth;
using TrackSense.Views;

namespace TrackSense.ViewModels
{
    public partial class TrackSenseDevicesViewModel : BaseViewModel
    {
        BluetoothService _bluetoothService;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsConnected))]
        TrackSenseDevice _connectedDevice;

        [ObservableProperty]
        bool isRefreshing;


        public bool IsConnected => this.ConnectedDevice is not null;

        public ObservableCollection<TrackSenseDevice> NearbyDevices { get; } = new();

        public TrackSenseDevicesViewModel(BluetoothService p_bluetoothService)
        {
            this.Title = "Ajout TrackSense";
            this._bluetoothService = p_bluetoothService;
        }

        async Task RequestBluetooth()
        {
            var status = PermissionStatus.Unknown;

            status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
            {
                return;
            }

            if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
            {
                await Shell.Current.DisplayAlert("Besoin d'autorisation", "Autoriser le scan.", "Ok");
            }

            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        [RelayCommand]
        public async Task ScanForBluetoothDevicesAsync()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                IsRefreshing = true;

                await RequestBluetooth();

                if (!this._bluetoothService.BluetoothIsOn())
                {
                    await Shell.Current.DisplayAlert("Erreur connexion bluetooth", "Veuillez vérifier votre connexion bluetooth", "Ok");
                    return;
                }

                if (this.NearbyDevices.Count > 0)
                {
                    this.NearbyDevices.Clear();
                }

                List<TrackSenseDevice> deviceList = await this._bluetoothService.GetBluetoothDevices();

                foreach (TrackSenseDevice device in deviceList)
                {
                    this.NearbyDevices.Add(device);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Erreur!", $"Message: {ex.Message}", "Ok");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        async Task ConnectToDeviceAsync(Guid p_id)
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                await _bluetoothService.ConnectToBluetoothDevice(p_id);
                IDevice device = _bluetoothService.GetConnectedDevice();

                if (device is not null)
                {
                    this.ConnectedDevice = this.NearbyDevices.SingleOrDefault(d => d.Id == device.Id);
                    if (this.ConnectedDevice is not null)
                    {
                        this.ConnectedDevice.IsConnected = true;
                    }
                }
                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Oups!",
                    $"Un problème est survenu pendant la connexion. Veuilez vérifier votre appareil.",
                    "Ok");
                Console.WriteLine("Erreur : " + ex.Message);
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        async Task DisplayDeviceMessageAsync()
        {
            IDevice connectedDevice = _bluetoothService.GetConnectedDevice();
            Guid serviceTestUID = new Guid("4fafc201-1fb5-459e-8fcc-c5c9c331914b");
            Guid characteristicTestUID = new Guid("beb5483e-36e1-4688-b7f5-ea07361b26a8");

            IService serviceTest = await connectedDevice.GetServiceAsync(serviceTestUID);
            ICharacteristic characteristicTest = await serviceTest.GetCharacteristicAsync(characteristicTestUID);

            byte[] messageByte = await characteristicTest.ReadAsync();

            string message = Encoding.UTF8.GetString(messageByte);

            await Shell.Current.DisplayAlert("Message", $"Affichage du message app embarquee : {message}", "Ok");
        }

        [RelayCommand]
        async Task SendMessageToDeviceAsync()
        {
            IDevice connectedDevice = _bluetoothService.GetConnectedDevice();
            Guid serviceTestUID = new Guid("4fafc201-1fb5-459e-8fcc-c5c9c331914b");
            Guid characteristicTestUID = new Guid("beb5483e-36e1-4688-b7f5-ea07361b26a8");

            IService serviceTest = await connectedDevice.GetServiceAsync(serviceTestUID);
            ICharacteristic characteristicTest = await serviceTest.GetCharacteristicAsync(characteristicTestUID);

            string messageToSend = "TrackSense mobile app !";
            byte[] messageToByte = Encoding.UTF8.GetBytes(messageToSend);

            var result = await characteristicTest.WriteAsync(messageToByte);

            string resultMessage = result ? "Envoi réussi" : "Problème avec l'envoi";

            await Shell.Current.DisplayAlert("Résultat", resultMessage, "OK");
        }
    }
}
