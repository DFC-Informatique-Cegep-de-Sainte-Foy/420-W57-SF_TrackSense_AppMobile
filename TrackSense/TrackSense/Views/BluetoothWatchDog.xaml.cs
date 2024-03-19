using TrackSense.ViewModels;

namespace TrackSense.Views;

public partial class BluetoothWatchDog : ContentPage
{

/* Unmerged change from project 'TrackSense (net7.0-android)'
Before:
	public BluetoothWatchDog(TrackSenseDevicesViewModel viewModel)
After:
	public BluetoothWatchDog(ViewModels.BluetoothWatchDog viewModel)
*/

/* Unmerged change from project 'TrackSense (net7.0-ios)'
Before:
	public BluetoothWatchDog(TrackSenseDevicesViewModel viewModel)
After:
	public BluetoothWatchDog(ViewModels.BluetoothWatchDog viewModel)
*/
	public BluetoothWatchDog(BluetoothWatchDogViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	protected async override void OnAppearing()
	{
        base.OnAppearing();

/* Unmerged change from project 'TrackSense (net7.0-android)'
Before:
		if (BindingContext is TrackSenseDevicesViewModel TSViewModel)
After:
		if (BindingContext is ViewModels.BluetoothWatchDog TSViewModel)
*/

/* Unmerged change from project 'TrackSense (net7.0-ios)'
Before:
		if (BindingContext is TrackSenseDevicesViewModel TSViewModel)
After:
		if (BindingContext is ViewModels.BluetoothWatchDog TSViewModel)
*/
		if (BindingContext is BluetoothWatchDogViewModel TSViewModel)
		{
			  await TSViewModel.ScanForBluetoothDevicesAsync();
		}
    }
}