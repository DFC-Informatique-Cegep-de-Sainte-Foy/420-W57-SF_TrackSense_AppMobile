﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrackSense.Entities.Exceptions;
using TrackSense.Models;
using TrackSense.Services;
using TrackSense.Services.Bluetooth;
using TrackSense.Views;

namespace TrackSense.ViewModels;

public partial class MainPageViewModel : BaseViewModel
{
    BluetoothService _bluetoothService;
    RideService _rideService;

    [ObservableProperty]
    bool isConnected;

    [ObservableProperty]
    bool isReceivingData;

    public MainPageViewModel(BluetoothService btService, RideService rideService)
    {
        Title = "Accueil";
        _bluetoothService = btService;
        _rideService = rideService;

        BluetoothObserver bluetoothObserver = new BluetoothObserver(this._bluetoothService,
            async (value) =>
            {
                switch (value.Type)
                {
                    case BluetoothEventType.CONNECTION:
                        isConnected = true;
                        break;
                    case BluetoothEventType.DECONNECTION:
                        isConnected = false;
                        break;
                    case BluetoothEventType.SENDING_RIDE_STATS:
                        isReceivingData = true;
                        this._rideService.ReceiveRideData(value.RideData);
                        if (value.RideData is Entities.CompletedRide ride)
                        {
                            await Shell.Current.DisplayPromptAsync("Ajout", $"Le trajet {ride.CompletedRideId} est reçu!");
                        }
                        break;
                    case BluetoothEventType.SENDING_RIDE_POINT:
                        isReceivingData = true;
                        this._rideService.ReceivePoints(value.RideData);
                        isReceivingData = false;
                        break;
                    default:
                        break;
                }
            });
    }

    [RelayCommand]
    async Task GoToTrackSenseDevices()
    {
        if (!IsConnected)
        {
            await Shell.Current.GoToAsync(nameof(TrackSenseDevicesPage));
        }
    }

    [RelayCommand]
    async Task SimulateRideReceptionAsync()
    {
        this._bluetoothService.SimulateRideReception();
    }

    [RelayCommand]
    async Task SimulatePointsReceptionAsync()
    {
        this._bluetoothService.SimulatePointsReception();
    }
}
