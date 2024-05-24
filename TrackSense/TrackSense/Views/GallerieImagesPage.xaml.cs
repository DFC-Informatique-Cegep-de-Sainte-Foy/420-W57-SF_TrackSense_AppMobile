using TrackSense.ViewModels;

namespace TrackSense.Views;

public partial class GallerieImagesPage : ContentPage
{
	public GallerieImagesPage(GallerieImagesViewModel viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is GallerieImagesViewModel viewModel)
        {
            if (viewModel.LoadImagesCommand.CanExecute(null))
            {
                viewModel.LoadImagesCommand.Execute(null);
            }
        }
    }
}