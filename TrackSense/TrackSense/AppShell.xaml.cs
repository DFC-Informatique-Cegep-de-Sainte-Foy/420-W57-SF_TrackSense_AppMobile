using TrackSense.Configurations;
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
            Routing.RegisterRoute(nameof(PlannedRideStatisticsPage), typeof(PlannedRideStatisticsPage));
            Routing.RegisterRoute(nameof(GalleriePage), typeof(GalleriePage));
            Routing.RegisterRoute(nameof(GallerieImagesPage), typeof(GallerieImagesPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));

        }

        private void ShellContent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }

        private void ShellContent_PropertyChanged_1(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }
    }
}