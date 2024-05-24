using Microsoft.Maui.Media;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE;
using TrackSense.ViewModels;
using TrackSense.Views;
using TrackSense.Services;
using TrackSense.Data;
using TrackSense.Entities;
using TrackSense.Services.Bluetooth;
using SkiaSharp.Views.Maui.Controls.Hosting;
using System.Reflection;
using TrackSense.Configurations;

namespace TrackSense
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp(true)
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
		builder.Logging.AddDebug();
#endif
            var configurationManager = InitializeConfigurationAsync().Result;            

            builder.Services.AddSingleton<IBluetoothLE>(CrossBluetoothLE.Current);
            builder.Services.AddSingleton<IAdapter>(CrossBluetoothLE.Current.Adapter);
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
            builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
            builder.Services.AddSingleton<IConfigurationManager>(configurationManager);

            builder.Services.AddSingleton<ICompletedRideLocalData, RideData>();

            builder.Services.AddSingleton<BluetoothService>();
            builder.Services.AddSingleton<RideService>();
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<LocationService>();
            builder.Services.AddSingleton<GallerieService>();

            builder.Services.AddSingleton<TrackSenseDevicesViewModel>();
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<CompletedRidesViewModel>();
            builder.Services.AddSingleton<CompletedRideStatisticsViewModel>();
            builder.Services.AddSingleton<PlannedRidesViewModel>();
            builder.Services.AddSingleton<PlannedRideStatisticsViewModel>();
            builder.Services.AddSingleton<CreateNewPlannedRideViewModel>();
            builder.Services.AddSingleton<SettingsViewModel>();
            builder.Services.AddSingleton<GallerieViewModel>();
            builder.Services.AddSingleton<GallerieImagesViewModel>();

            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<CompletedRidesPage>();
            builder.Services.AddTransient<PlannedRidesPage>();
            builder.Services.AddTransient<CreateNewPlannedRidePage>();
            builder.Services.AddTransient<TrackSenseDevicesPage>();
            builder.Services.AddTransient<CompletedRideStatisticsPage>();
            builder.Services.AddTransient<PlannedRideStatisticsPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<GalleriePage>();
            builder.Services.AddTransient<GallerieImagesPage>();

            return builder.Build();
        }

        private static async Task<ConfigurationManager> InitializeConfigurationAsync()
        {
            var configurationManager = new ConfigurationManager();
            var configurationFilePath = Path.Combine(FileSystem.AppDataDirectory, "user-settings.json");

            if (!File.Exists(configurationFilePath))
            {
                var defaultSettings = new Settings
                {
                    ApiUrl = "https://tracksense-api.rapidotron.com/api",
                    Username = "admin",
                    Endpoint = "minio.rapidotron.com",
                    AccessKey = "ZUzuRtiSnBktqzWNtSCw",
                    SecretKey = "CD6BbgnuqPPXhXZQdYbh1X3NCxRdtyuOa0aUSPRL"
                };
                configurationManager.SaveSettings(defaultSettings);
            }

            return configurationManager;
        }
    }
}