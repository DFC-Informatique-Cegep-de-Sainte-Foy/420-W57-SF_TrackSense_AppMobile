using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using TrackSense.Entities;
using TrackSense.Services.Bluetooth;
using TrackSense.Services.API.APIDTO;
using TrackSense.Configurations;

namespace TrackSense.Services;

public class RideService
{
    private ICompletedRideLocalData _rideData;
    private IPlannedRideLocalData _plannedRideData;
    BluetoothService _bluetoothService;
    CompletedRide _currentRide;
    List<CompletedRideSummary> _completedRides = new();
    List<PlannedRideSummary> _plannedRides = new();
    HttpClient httpClient;
    IConfigurationManager _config;

    public RideService(ICompletedRideLocalData rideData, IPlannedRideLocalData plannedRideData, BluetoothService bluetoothService, IConfigurationManager config)
    {
        _plannedRideData = plannedRideData;
        _rideData = rideData;
        _bluetoothService = bluetoothService;
        httpClient = new HttpClient();
        _config = config;
    }

    internal async Task ReceiveRideDataFromDevice(CompletedRide rideData)
    {
        if (rideData is null)
        {
            throw new ArgumentNullException(nameof(rideData));
        }

        this._currentRide = rideData;
        Settings userSettings = _config.LoadSettings();
        this._currentRide.UserLogin = userSettings.Username;

        bool isConfirmed = false;
        while (!isConfirmed) // politique de réessai
        {
            isConfirmed = await this._bluetoothService.ConfirmRideDataReception(0);
        }
    }

    internal async Task ReceivePointDataFromDevice(CompletedRidePoint ridePoint)
    {
        if (this._currentRide is null)
        {
            throw new InvalidOperationException();
        }
        
        int numberOfPointsReceived = this._currentRide.CompletedRidePoints.Count;
        int totalNumberOfPoints = this._currentRide.Statistics.NumberOfPoints;

        if (ridePoint.RideStep == numberOfPointsReceived + 1)
        {
            try
            {
                bool isConfirmed = false;
                while (!isConfirmed) // politique de réessai à ajouter
                {
                    isConfirmed = await this._bluetoothService.ConfirmRideDataReception(ridePoint.RideStep);
                }
                Debug.Write(ridePoint.RideStep);
                if (isConfirmed)
                {
                    Debug.Write("Confirmation : point #" + ridePoint.RideStep);
                    this._currentRide.CompletedRidePoints.Add(ridePoint);
                    Debug.Write("Ajout point #" + ridePoint.RideStep);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Erreur confirmation : " + e.Message);
            }
        }

        if (ridePoint.RideStep == totalNumberOfPoints)
        {
            this._rideData.AddCompletedRide(this._currentRide);
            HttpResponseMessage result = await this.PostCompletedRideAsync(this._currentRide);
            if (result.IsSuccessStatusCode)
            {
                this._rideData.DeleteCompletedRideById(this._currentRide.CompletedRideId);
            }
            this._currentRide = null;
        }
    }

    public async Task<List<CompletedRideSummary>> GetUserCompletedRides()
    {
        if (_completedRides.Count != 0)
        {
            _completedRides.Clear();
        }
        Settings userSettings = _config.LoadSettings();
        string url = $"{userSettings.ApiUrl}/users/{userSettings.Username}/completedRides";

        var response = await httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            List<API.APIDTO.CompletedRideSummaryDTO> completedRides = await response.Content.ReadFromJsonAsync<List<API.APIDTO.CompletedRideSummaryDTO>>();
            _completedRides.AddRange(completedRides.Select(ride => ride.ToEntity()));
        }
       
        return _completedRides;
    }

    public async Task<List<PlannedRideSummary>> GetUserPlannedRides()
    {
        if (_plannedRides.Count != 0)
        {
            _plannedRides.Clear();
        }
        Settings userSettings = _config.LoadSettings();
        string url = $"{userSettings.ApiUrl}/users/{userSettings.Username}/plannedRides";

        var response = await httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            List<PlannedRideSummaryDTO> plannedRides = await response.Content.ReadFromJsonAsync<List<PlannedRideSummaryDTO>>();
            _plannedRides.AddRange(plannedRides.Select(ride => ride.ToEntity()));
        }

        return _plannedRides;
    }

    public List<CompletedRideSummary> GetCompletedRideSummariesFromLocalStorage()
    {
        List<CompletedRide> rides = this._rideData.ListCompletedRides();
        List<CompletedRideSummary> summaries = new();

        foreach (CompletedRide ride in rides)
        {
            summaries.Add(new CompletedRideSummary()
            {
                CompletedRideId = ride.CompletedRideId,
                PlannedRideName = "None",
                StartedAt = ride.CompletedRidePoints.First().Location.Timestamp.DateTime,
                Duration = ride.Statistics.Duration,
                Distance = ride.Statistics.Distance
            });
        }

        return summaries;
    }

    internal async Task<HttpResponseMessage> PostCompletedRideAsync(CompletedRide p_completedRide)
    {
        try
        {
            if (p_completedRide == null)
            {
                throw new ArgumentNullException(nameof(p_completedRide));
            }

            CompletedRideDTO completedRideDTO = new CompletedRideDTO(p_completedRide);

            Settings userSettings = _config.LoadSettings();
            string url = $"{userSettings.ApiUrl}/CompletedRides";

            var content = new StringContent(JsonConvert.SerializeObject(completedRideDTO), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return response;
            }
            else
            {
                Console.WriteLine($"HTTP Error: {response.StatusCode}");
                throw new HttpRequestException($"HTTP Error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            throw;
        }
    }


    internal async Task<Entities.CompletedRide> GetCompletedRide(Guid completedRideId)
    {
        if (completedRideId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(completedRideId));
        }
        Settings userSettings = _config.LoadSettings();
        string url = $"{userSettings.ApiUrl}/CompletedRides/{completedRideId}";

        var response = await httpClient.GetAsync(url);

        CompletedRide completedRide = null;

        if (response.IsSuccessStatusCode)
        {
            API.APIDTO.CompletedRideDTO completedRideDTO = await response.Content.ReadFromJsonAsync<API.APIDTO.CompletedRideDTO>();
            completedRide =  completedRideDTO.ToEntity();
        }

        return completedRide;
    }

    internal async Task<PlannedRide> GetPlannedRide(Guid plannedRideId)
    {
        if (plannedRideId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(plannedRideId));
        }
        Settings userSettings = _config.LoadSettings();
        string url = $"{userSettings.ApiUrl}/PlannedRides/{plannedRideId}";

        var response = await httpClient.GetAsync(url);

        PlannedRide plannedRide = null;

        if (response.IsSuccessStatusCode)
        {
            PlannedRideDTO plannedRideDTO = await response.Content.ReadFromJsonAsync<PlannedRideDTO>();
            plannedRide = plannedRideDTO.ToEntity();
        }

        return plannedRide;
    }

    internal void DeleteRidesFromLocalStorage()
    {
        this._rideData.DeleteAllCompletedRides();
    }

    public void InterruptReception()
    {
        this._currentRide = null;
    }

    internal CompletedRide GetCompletedRideFromLocalStorage(Guid completedRideId)
    {
        return this._rideData.GetCompletedRideById(completedRideId);
    }

    internal List<CompletedRide> GetAllCompletedRides()
    {
        return this._rideData.ListCompletedRides();
    }
}
