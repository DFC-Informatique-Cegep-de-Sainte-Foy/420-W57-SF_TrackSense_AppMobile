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

        private void ShellContent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }

        private void ShellContent_PropertyChanged_1(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }
    }
}