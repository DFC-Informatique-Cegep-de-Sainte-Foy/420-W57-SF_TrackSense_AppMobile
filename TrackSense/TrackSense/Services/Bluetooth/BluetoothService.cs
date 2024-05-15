using Microsoft.Maui.Controls;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using TrackSense.Entities;
using TrackSense.Models;
using TrackSense.Services.Bluetooth.BluetoothDTO;

namespace TrackSense.Services.Bluetooth
{
    public class BluetoothService : IObservable<BluetoothEvent>
    {
        IBluetoothLE bluetoothLE;
        List<IDevice> bluetoothDevices = new();
        private List<IObserver<BluetoothEvent>> observers = new();
        CompletedRideDTO _completedRideDTO;
        bool isBusy = false;
        public bool IsReceiving { get; private set; }
        public int ScreenRotation { get; private set; }
        ICharacteristic _notificationCharacteristic;
        ICharacteristic _dataCharacteristic;

        public BluetoothService(IBluetoothLE bluetoothLE)
        {
            this.bluetoothLE = bluetoothLE;
        }

        public async Task<List<TrackSenseDevice>> GetBluetoothDevices()
        {
            if (bluetoothDevices.Count == 0)
            {
                bluetoothLE.Adapter.DeviceDiscovered += (s, a) =>
                {
                    if (!bluetoothDevices.Contains(a.Device))
                    {
                        bluetoothDevices.Add(a.Device);
                    }
                };
            }

            bluetoothDevices.Clear();

            List<TrackSenseDevice> devicesList = new List<TrackSenseDevice>();

            bluetoothLE.Adapter.ScanTimeout = 2000;
            bluetoothLE.Adapter.ScanMode = ScanMode.Balanced;

            await bluetoothLE.Adapter.StartScanningForDevicesAsync();

            foreach (IDevice device in bluetoothDevices)
            {
                if(!String.IsNullOrWhiteSpace(device.Name) && device.Name.Contains("TrackSense"))
                {
                    TrackSenseDevice bleDevice = new TrackSenseDevice()
                    {
                        Name = device.Name,
                        IsConnected = device.State == DeviceState.Connected,
                        Id = device.Id,
                        Address = device.NativeDevice.ToString()
                    };
                    devicesList.Add(bleDevice);
                }
            }

            return devicesList;
        }

        public async Task ConnectToBluetoothDevice(Guid p_id)
        {
            try
            {
                IDevice device = bluetoothDevices.SingleOrDefault(d => d.Id == p_id);
                if (device is not null)
                {
                    await bluetoothLE.Adapter.ConnectToDeviceAsync(device);

                    SetNotifications();
                    await GetSettingsFromDevice();
                    BluetoothEvent BTEventConnection = new BluetoothEvent(BluetoothEventType.CONNECTION, true);
                    observers.ForEach(o => o.OnNext(BTEventConnection));
                }
            }
            catch (DeviceConnectionException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private async Task GetSettingsFromDevice() // not functionnal yet
        {
                IDevice connectedDevice = bluetoothLE.Adapter.ConnectedDevices.SingleOrDefault();

            if (connectedDevice is not null)
            {
                Guid screenServiceUUID = new Guid("68c50cff-e5ad-4cb8-9541-997d42925f27");
                Guid screenRotationCharacUUID = new Guid("65000b05-c1a9-4dfb-a173-bdaa4a029bf6");
                IService screenRotationService = await connectedDevice.GetServiceAsync(screenServiceUUID);
                ICharacteristic screenRotationCharac = await screenRotationService.GetCharacteristicAsync(screenRotationCharacUUID);

                try
                {
                    byte[] screenRotationBytes = await screenRotationCharac.ReadAsync();
                    string screenRotationString = Encoding.UTF8.GetString(screenRotationBytes);
                    ScreenRotation = int.Parse(screenRotationString);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Une erreur est survenue pendant la réception de la valeur de rotation de l'écran : " + e.Message);
                }
            }
        }

        private async void SetNotifications()
        {

            IDevice connectedDevice = bluetoothLE.Adapter.ConnectedDevices.SingleOrDefault();
            Guid completedRideServiceUID = new Guid("62ffab64-3646-4fb9-88d8-541deb961192");
            Guid rideStatsUID = new Guid("51656aa8-b795-427f-a96c-c4b6c57430dd");
            Guid rideNotificationUID = new Guid("61656aa8-b795-427f-a96c-c4b6c57430dd");

            IService completedRideService = await connectedDevice.GetServiceAsync(completedRideServiceUID);
            _dataCharacteristic = await completedRideService.GetCharacteristicAsync(rideStatsUID);
            _notificationCharacteristic = await completedRideService.GetCharacteristicAsync(rideNotificationUID);

            _notificationCharacteristic.ValueUpdated += async (sender, args) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        // read stats and Points
                        byte[] status = args.Characteristic.Value;
                        string statusString = Encoding.UTF8.GetString(status);
                        if (statusString == "sending" && !isBusy)
                        {
                            isBusy = true;
                            IsReceiving = true;
                            byte[] rideBytes = await _dataCharacteristic.ReadAsync();
                            string rideMessage = Encoding.UTF8.GetString(rideBytes);

                            if (_completedRideDTO is null)
                            {
                                CompletedRideDTO completedRideDTO = new CompletedRideDTO(rideMessage);

                                BluetoothEvent BTEventReceiveStats = new BluetoothEvent(BluetoothEventType.SENDING_RIDE_STATS, completedRideDTO.ToEntity());
                                observers.ForEach(o => o.OnNext(BTEventReceiveStats));
                                this._completedRideDTO = completedRideDTO;
                                Debug.WriteLine("Ride ajouté : " + completedRideDTO.CompletedRideId);

                                while (this._completedRideDTO.CompletedRidePoints.Count != this._completedRideDTO.Statistics.NumberOfPoints)
                                {
                                    try
                                    {
                                        Debug.WriteLine("ReadAsync() un point");
                                        rideBytes = await _dataCharacteristic.ReadAsync();
                                        rideMessage = Encoding.UTF8.GetString(rideBytes);
                                        int rideStep;
                                        bool success = int.TryParse(rideMessage.Split(';')[0], out rideStep);

                                        if (success)
                                        {
                                            CompletedRidePointDTO pointDTO = new CompletedRidePointDTO(rideMessage); // !
                                            if (pointDTO.RideStep == this._completedRideDTO.CompletedRidePoints.Count + 1)
                                            {
                                                Entities.CompletedRidePoint completedRidePoint = pointDTO.ToEntity();

                                                BluetoothEvent BTEventReceivePoint = new BluetoothEvent(BluetoothEventType.SENDING_RIDE_POINT, completedRidePoint);
                                                observers.ForEach(o => o.OnNext(BTEventReceivePoint));

                                                this._completedRideDTO.CompletedRidePoints.Add(pointDTO);
                                                Debug.WriteLine("Point ajouté : " + pointDTO.RideStep);
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.WriteLine("Erreur lecture point : " + e.Message);
                                    }
                                }
                                _completedRideDTO = null;
                                IsReceiving = false;
                                isBusy = false;
                            }
                        }
                    }
                    catch(ArgumentNullException e)
                    {
                        Debug.WriteLine("Le format du trajet reçu n'est pas valide : " + e.Message);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Une erreur est survenue pendant la réception du trajet : " + e.Message);
                    }
                });
            };
            await _notificationCharacteristic.StartUpdatesAsync();
        }

        public IDevice GetConnectedDevice()
        {
            return bluetoothLE.Adapter.ConnectedDevices.SingleOrDefault();
        }

        public bool BluetoothIsOn()
        {
            return bluetoothLE.State == BluetoothState.On;
        }

        public IDisposable Subscribe(IObserver<BluetoothEvent> observer)
        {
            if (observer is null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            observers.Add(observer);
            return new UnsubscriberBluetooth(observers, observer);
        }

        internal async Task<bool> ConfirmRideDataReception(int rideStepNumber)
        {
            byte[] confirmationString = Encoding.UTF8.GetBytes(rideStepNumber.ToString());
            bool result = false;
            Debug.WriteLine("Confirmation réception point #" + rideStepNumber);

            try
            {
                while (!result)
                {
                    result = await _notificationCharacteristic.WriteAsync(confirmationString);
                }
                return result;

            }
            catch (Exception e)
            {
                Debug.WriteLine("Erreur ConfirmRideStatsReception : " + e.Message);
                return result;
                //throw e;
            }
        }

        #region DEBUG

        public async void DisconnectDevice()
        {
            if (this.GetConnectedDevice() is not null)
            {
                await bluetoothLE.Adapter.DisconnectDeviceAsync(bluetoothLE.Adapter.ConnectedDevices.SingleOrDefault());
                BluetoothEvent BTEventConnection = new BluetoothEvent(BluetoothEventType.DECONNECTION, false);
                observers.ForEach(o => o.OnNext(BTEventConnection));
                this._dataCharacteristic = null;
                this._notificationCharacteristic = null;
                this._completedRideDTO = null;
            }
        }

        internal async Task<bool> SetScreenRotation(int screenRotation)
        {
            IDevice connectedDevice = bluetoothLE.Adapter.ConnectedDevices.SingleOrDefault();
            Guid screenServiceUUID = new Guid("68c50cff-e5ad-4cb8-9541-997d42925f27");
            Guid screenRotationCharacUUID = new Guid("65000b05-c1a9-4dfb-a173-bdaa4a029bf6");
            IService screenRotationService = await connectedDevice.GetServiceAsync(screenServiceUUID);
            ICharacteristic screenRotationCharac = await screenRotationService.GetCharacteristicAsync(screenRotationCharacUUID);

            byte[] screenRotationString = Encoding.UTF8.GetBytes(screenRotation.ToString());
            bool result = false;

            try
            {
                while (!result)
                {
                    result = await screenRotationCharac.WriteAsync(screenRotationString);
                }
                return result;

            }
            catch (Exception e)
            {
                Debug.WriteLine("Erreur envoi rotation : " + e.Message);
                return result;
            }
        }

        internal async Task<bool> EnvoyerTrajet(string json)
        {
            //TODO:
            IDevice connectedDevice = bluetoothLE.Adapter.ConnectedDevices.SingleOrDefault();
            Guid trajetServiceUUID = new("68c50cff-e5ad-4cb8-9541-997d42925f17");   //  Il est cohérent avec l'UUID de service enregistré par Bluetooth côté TS.
            Guid trajetJSONCharacUUID = new("65000b05-c1a9-4dfb-a173-bdaa4a029cf6");// Il est cohérent avec l'UUID de service enregistré par Bluetooth côté TS.
            IService trajetService = await connectedDevice.GetServiceAsync(trajetServiceUUID);
            ICharacteristic trajetJSONCharac = await trajetService.GetCharacteristicAsync(trajetJSONCharacUUID);
            
            byte[] trajetJsonString = Encoding.UTF8.GetBytes(json);
            bool result = false;

            try
            {
                while (!result)
                {
                    result = await trajetJSONCharac.WriteAsync(trajetJsonString);
                }
                return result;

            }
            catch (Exception e)
            {
                Debug.WriteLine("Erreur envoi trajet : " + e.Message);
                return result;
            }
        }



        #endregion
    }
}
