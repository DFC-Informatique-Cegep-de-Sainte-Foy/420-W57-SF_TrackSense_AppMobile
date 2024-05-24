using TrackSense.ViewModels;

namespace TrackSense.Views;

public partial class TrackSenseDevicesPage : ContentPage
{
    public TrackSenseDevicesPage(TrackSenseDevicesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TrackSenseDevicesViewModel TSViewModel)
        {
            await TSViewModel.ScanForBluetoothDevicesAsync();
        }
    }
}