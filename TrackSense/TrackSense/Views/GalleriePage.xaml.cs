using System.Net;
using ExifLibrary;
using Minio;
using Minio.DataModel.Args;
using TrackSense.ViewModels;

namespace TrackSense.Views;

public partial class GalleriePage : ContentPage
{
	public GalleriePage(GallerieViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
}