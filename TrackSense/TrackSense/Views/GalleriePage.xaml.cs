using System.Net;
using ExifLibrary;
using Minio;
using Minio.DataModel.Args;

namespace TrackSense.Views;

public partial class GalleriePage : ContentPage
{
	public GalleriePage()
	{
		InitializeComponent();
	}

    private async void PrendrePhoto(object sender, EventArgs e)
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

            var location = await GetLocationAsync();

            if (photo != null && location != null)
            {
                string fileName = $"{DateTime.Now:yyyyMMddHHmmss}_lng{location.Longitude}_lat{location.Latitude}_{photo.FileName}";
                string localFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                using Stream sourceStream = await photo.OpenReadAsync();
                using FileStream localFileStream = File.OpenWrite(localFilePath);

                await sourceStream.CopyToAsync(localFileStream);
                localFileStream.Flush();
                localFileStream.Close();

#if DEBUG
                    await Shell.Current.DisplayAlert("Chemin fichier: ", localFilePath, "OK");
#endif
                var file = ImageFile.FromFile(localFilePath);
                var latRef = location.Latitude > 0 ? 'N' : 'S';
                var lngRef = location.Longitude > 0 ? 'E' : 'W';

                double latitude = Math.Abs(location.Latitude);
                double longitude = Math.Abs(location.Longitude);

                //https://github.com/oozcitak/exiflibrary/tree/master/Documentation
                file.Properties.Set(ExifTag.GPSLatitudeRef, latRef.ToString());
                file.Properties.Set(ExifTag.GPSLatitude, latitude);

                file.Properties.Set(ExifTag.GPSLongitudeRef, lngRef.ToString());
                file.Properties.Set(ExifTag.GPSLongitude, longitude);

                file.Save(localFilePath);

                await PrepareUploadToMinioBucket(localFilePath, fileName);
            }
        }
        else
        {
            await Shell.Current.DisplayAlert("Erreur", "La caméra n'est pas supportée et/ou la géolocalisation n'est pas activée!", "OK");
        }
    }

    private async Task<bool> ValiderImage(string filePath)
    {
        bool resultat = false;
        var validImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        if (!validImageExtensions.Contains(Path.GetExtension(filePath)))
        {
            await Shell.Current.DisplayAlert("Téléversement échoué.", "Le fichier devrait être une image", "OK");
            resultat = false;
        }

        FileInfo fileInfo = new FileInfo(filePath);
        long maxFileSize = 25 * 1024 * 1024; // 25 MB ça devrait être suffisant sauf pour les frais chier avec des caméras de 200 megapixels gens S23 Ultra...
        if (fileInfo.Length > maxFileSize)
        {
            await Shell.Current.DisplayAlert("Téléversement échoué", "La taille de l'image ne devrait pas dépasser 20Mo.", "OK");
            resultat = false;
        }

        return resultat;
    }

    private static string GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".gif":
                return "image/gif";
            case ".png":
                return "image/png";
            default:
                return "application/octet-stream";
        }
    }

    private async Task<bool> PrepareUploadToMinioBucket(string filePath, string fileName)
    {
        bool resultat = false;

        if (!await ValiderImage(filePath))
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                                               | SecurityProtocolType.Tls11
                                               | SecurityProtocolType.Tls12;
            var endpoint = "10.201.176.179:9000";
            var accessKey = "n2qsvPKdSi5HPz9kfdRE";
            var secretKey = "kic5lA5pxjqyNvP5Jp4oIWHboYvneuinciZ5Tp90";

            try
            {
                using var minio = new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(accessKey, secretKey)
                    //.WithSSL()
                    .Build();
                await UploadToMinio(minio, filePath, fileName).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
#if DEBUG
                await Shell.Current.DisplayAlert("Erreur", ex.Message, "OK");
#elif RELEASE
                await Shell.Current.DisplayAlert("Erreur", "Une erreur est survenue lors la préparation pour l'envoi de l'image", "OK");
#endif
            }
        }

        return resultat;
    }

    private static async Task UploadToMinio(IMinioClient minio, string filePath, string fileName)
    {
        var bucketName = "test2"; //faudrait mettre le userId ou ketchose...
        var location = "";
        var objectName = fileName;
        var contentType = GetContentType(filePath);

        try
        {
            var bktExistArgs = new BucketExistsArgs()
                .WithBucket(bucketName);
            var found = await minio.BucketExistsAsync(bktExistArgs).ConfigureAwait(true);
            if (!found)
            {
                var mkBktArgs = new MakeBucketArgs()
                    .WithBucket(bucketName)
                    .WithLocation(location);
                await minio.MakeBucketAsync(mkBktArgs).ConfigureAwait(true);
#if DEBUG
                await Shell.Current.DisplayAlert("Bucket créé: ", bucketName, "OK");
#endif
            }

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithFileName(filePath)
                .WithContentType(contentType);
            _ = await minio.PutObjectAsync(putObjectArgs).ConfigureAwait(true);
#if DEBUG
            await Shell.Current.DisplayAlert("Objet créé: ", objectName, "OK");
#elif RELEASE
            await Shell.Current.DisplayAlert("Téléversement réussi", "L'image a été téléversée avec succès", "OK");
#endif
        }
        catch (Exception ex)
        {
#if DEBUG
            Console.WriteLine(ex.Message);
            await Shell.Current.DisplayAlert(fileName, ex.Message, "OK");
#elif RELEASE
            await Shell.Current.DisplayAlert("Erreur", "Une erreur est survenue lors du téléversement de l'image", "OK");
#endif
        }

    }


    private async Task<Location> GetLocationAsync()
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Best);
            var location = await Geolocation.GetLocationAsync(request);

            if (location != null)
            {
                return location;
            }
        }
        catch (FeatureNotSupportedException fnsEx)
        {
#if DEBUG
            await Shell.Current.DisplayAlert("Erreur", fnsEx.Message, "OK");
#elif RELEASE
            await Shell.Current.DisplayAlert("Erreur", "La localisation n'est pas supportée", "OK");
#endif
        }
        catch (FeatureNotEnabledException fneEx)
        {
#if DEBUG
            await Shell.Current.DisplayAlert("Erreur", fneEx.Message, "OK");
#elif RELEASE
            await Shell.Current.DisplayAlert("Erreur", "La localisation n'est pas activée", "OK");
#endif
        }
        catch (PermissionException pEx)
        {
#if DEBUG
            await Shell.Current.DisplayAlert("Erreur", pEx.Message, "OK");
#elif RELEASE
            await Shell.Current.DisplayAlert("Erreur", "La permission pour la localisation n'est pas activée", "OK");
#endif
        }
        catch (Exception ex)
        {
#if DEBUG
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
#elif RELEASE
            await Shell.Current.DisplayAlert("Erreur", "Une erreur est survenue lors de la récupération de la localisation", "OK");
#endif
        }

        return null;
    }

    private async void OuvrirGallerie(object sender, EventArgs e)
    {
        await Shell.Current.DisplayAlert("Ouvrir la gallerie", "Pas encore implémenté", "OK");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
}