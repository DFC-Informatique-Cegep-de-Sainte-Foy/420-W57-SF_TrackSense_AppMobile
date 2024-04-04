using CommunityToolkit.Mvvm.Input;

namespace TrackSense.Views;

public partial class GalleriePage : ContentPage
{
	public GalleriePage()
	{
		InitializeComponent();
	}

    private async void PrendrePhoto()
    {
        if (MediaPicker.IsCaptureSupported)
        {
            FileResult myPhoto = await MediaPicker.Default.CapturePhotoAsync();

            if (myPhoto is not null)
            {
                string localFilePath = Path.Combine(FileSystem.CacheDirectory, myPhoto.FileName);
                using Stream sourceStream = await myPhoto.OpenReadAsync();
                using FileStream localFileStream = File.OpenWrite(localFilePath);
                await sourceStream.CopyToAsync(localFileStream);
                await Shell.Current.DisplayAlert("Photo", $"Photo saved to {localFileStream.Name}", "OK");
            }
        }
        else
        {
            await Shell.Current.DisplayAlert("Camera", "Camera is not supported", "OK");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        PrendrePhoto();
    }
}