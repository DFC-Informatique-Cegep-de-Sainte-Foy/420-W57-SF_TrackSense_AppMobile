using Microsoft.Extensions.Logging;
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
            builder.Services.AddSingleton<IBluetoothLE>(CrossBluetoothLE.Current);
            builder.Services.AddSingleton<IAdapter>(CrossBluetoothLE.Current.Adapter);
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
            builder.Services.AddSingleton<IConfigurationManager, ConfigurationManager>();

            builder.Services.AddSingleton<ICompletedRideLocalData, RideData>();

            builder.Services.AddSingleton<BluetoothService>();
            builder.Services.AddSingleton<RideService>();
            builder.Services.AddSingleton<UserService>();


/* Unmerged change from project 'TrackSense (net7.0-android)'
Before:
            builder.Services.AddSingleton<TrackSenseDevicesViewModel>();
After:
            builder.Services.AddSingleton<ViewModels.BluetoothWatchDog>();
*/

/* Unmerged change from project 'TrackSense (net7.0-ios)'
Before:
            builder.Services.AddSingleton<TrackSenseDevicesViewModel>();
After:
            builder.Services.AddSingleton<ViewModels.BluetoothWatchDog>();
*/
            builder.Services.AddSingleton<BluetoothWatchDogViewModel>();
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<CompletedRidesViewModel>();
            builder.Services.AddSingleton<CompletedRideStatisticsViewModel>();
            builder.Services.AddSingleton<SettingsViewModel>();

            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<CompletedRidesPage>();
            builder.Services.AddTransient<BluetoothWatchDog>();
            builder.Services.AddTransient<CompletedRideStatisticsPage>();
            builder.Services.AddTransient<SettingsPage>();
            return builder.Build();
        }
    }
}