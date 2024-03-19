using TrackSense.Views;

namespace TrackSense
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(TrackSenseDevicesPage), typeof(TrackSenseDevicesPage));
            Routing.RegisterRoute(nameof(CompletedRideStatisticsPage), typeof(CompletedRideStatisticsPage));
            Routing.RegisterRoute(nameof(CompletedRidesPage), typeof(CompletedRidesPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        }
    }
}