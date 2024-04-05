using CommunityToolkit.Mvvm.Input;
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

    private async void PrendrePhoto()
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

                //await UploadFileToFtp(localFilePath);
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
            var endpoint = "10.10.0.58:9000";
            var accessKey = "n2qsvPKdSi5HPz9kfdRE";
            var secretKey = "kic5lA5pxjqyNvP5Jp4oIWHboYvneuinciZ5Tp90";
            //var secretKey = "asdfasdfasdf";

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
                Console.WriteLine(ex.Message);
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
            var found = await minio.BucketExistsAsync(bktExistArgs).ConfigureAwait(false);
            if (!found)
            {
                var mkBktArgs = new MakeBucketArgs()
                    .WithBucket(bucketName)
                    .WithLocation(location);
                await minio.MakeBucketAsync(mkBktArgs).ConfigureAwait(false);
#if DEBUG
                await Shell.Current.DisplayAlert("Bucket créé: ", bucketName, "OK");
#endif
            }

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithFileName(filePath)
                .WithContentType(contentType);
            _ = await minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
#if DEBUG
            await Shell.Current.DisplayAlert("Objet créé: ", objectName, "OK");
#endif
        }
        catch (Exception e)
        {
            await Shell.Current.DisplayAlert(fileName, e.Message, "OK");
        }

    }

    private async Task<bool> UploadFileToFtp(string filePath)
    {
        bool resultat = false;

        if (!await ValiderImage(filePath))
        {
            string ftpUrl = "ftp://10.201.159.71";
            string ftpUsername = "tracksense";
            string ftpPassword = "tracksense";

            Uri serverUri = new Uri($"{ftpUrl}/{Path.GetFileName(filePath)}");
            //https://learn.microsoft.com/en-us/dotnet/api/system.net.ftpwebrequest?view=net-8.0
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverUri);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

            byte[] fileContents;
            using (FileStream sourceStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fileContents = new byte[sourceStream.Length];
                await sourceStream.ReadAsync(fileContents, 0, fileContents.Length);
            }

            request.ContentLength = fileContents.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                await requestStream.WriteAsync(fileContents, 0, fileContents.Length);
            }

            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
            {
#if DEBUG
                    await Shell.Current.DisplayAlert("Upload Complete", $"Status: {response.StatusDescription}", "OK");
#endif
                resultat = true;
            }
        }
        return resultat;
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
            await Shell.Current.DisplayAlert("Erreur", "La localisation n'est pas supportée", "OK");
        }
        catch (FeatureNotEnabledException fneEx)
        {
            await Shell.Current.DisplayAlert("Error", "La localisation n'est pas activée", "OK");
        }
        catch (PermissionException pEx)
        {
            await Shell.Current.DisplayAlert("Error", "La permission pour la localisation n'est pas activée", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", "Une erreur est survenue", "OK");
        }

        return null;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        PrendrePhoto();
    }
}