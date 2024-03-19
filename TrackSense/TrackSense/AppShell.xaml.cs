using TrackSense.Views;

namespace TrackSense
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(BluetoothWatchDog), typeof(BluetoothWatchDog));
            Routing.RegisterRoute(nameof(CompletedRideStatisticsPage), typeof(CompletedRideStatisticsPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        }
    }
}