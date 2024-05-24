using TrackSense.Entities;
using TrackSense.Configurations;
using ExifLibrary;
using Minio.DataModel.Args;
using Minio;
using System.Net;
using Minio.Exceptions;
using Minio.DataModel;
using System.Diagnostics;
using Microsoft.Maui.Storage;
using TrackSense.Views;

namespace TrackSense.Services;

public class GallerieService
{

    IConfigurationManager _configuration;
    Settings _userSettings;

    public GallerieService(IConfigurationManager configurationManager)
    {
        _configuration = configurationManager;
        _userSettings = _configuration.LoadSettings();

        _configuration.ConfigurationChanged += (s, e) =>
        {
            _userSettings = _configuration.LoadSettings();
        };
    }

    public async Task PrendrePhoto()
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

            var LocationService = new LocationService();

            var location = await LocationService.GetLocationAsync();

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
        long maxFileSize = 25 * 1024 * 1024; // 25 MB ça devrait être suffisant sauf pour les comiques avec des caméras de 200 megapixels genre S23 Ultra...
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

            var endpoint = _userSettings.Endpoint;
            var accessKey = _userSettings.AccessKey;
            var secretKey = _userSettings.SecretKey;

            try
            {
                using var minio = new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(accessKey, secretKey)
                    .WithSSL()
                    .Build();
#if DEBUG
                minio.SetTraceOn();
#endif
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

    private async Task UploadToMinio(IMinioClient minio, string filePath, string fileName)
    {
        var bucketName = _userSettings.Username; //faudrait mettre le userId ou ketchose...
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

        catch (MinioException minioEx)
        {
#if DEBUG
            await Shell.Current.DisplayAlert("Erreur Minio", minioEx.Message, "OK");
#elif RELEASE
            await Shell.Current.DisplayAlert("Erreur Minio", "Une erreur est survenue lors du téléversement vers Minio", "OK");
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


    public async Task OuvrirGallerie()
    {
        try
        {
            await PrepareGetGalleryFromBucket();
            await Shell.Current.GoToAsync(nameof(GallerieImagesPage));
        }
        catch(Exception e)
        {
            await Shell.Current.DisplayAlert("Erreur OuvrirGallerie", e.Message, "OK");
        }
    }

    private async Task PrepareGetGalleryFromBucket()
    {
        var endpoint = _userSettings.Endpoint;
        var accessKey = _userSettings.AccessKey;
        var secretKey = _userSettings.SecretKey;
#if DEBUG
        Console.WriteLine(_userSettings.Username);
        Console.WriteLine(_userSettings.ApiUrl);
        Console.WriteLine($"Endpoint: {endpoint}");
        Console.WriteLine($"AccessKey: {accessKey}");
        Console.WriteLine($"SecretKey: {secretKey}");
 #endif

        try
        {
            var minio = new MinioClient()
                    .WithEndpoint(endpoint, 443)
                    .WithCredentials(accessKey, secretKey)
                    .WithSSL()
                    .Build();
            await GetGalleryFromMinio(minio).ConfigureAwait(false);
        }
        catch (MinioException minioEx)
        {
            await Shell.Current.DisplayAlert("Erreur Minio", minioEx.Message, "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erreur PrepareGetGalleryFromBucket", ex.Message, "OK");
        }
    }

    private async Task GetGalleryFromMinio(IMinioClient minio)
    {
        var bucketName = _userSettings.Username;
        var appFolderPath = Path.Combine(FileSystem.AppDataDirectory, "images", bucketName);
        if (!Directory.Exists(appFolderPath)) //normalement, le dossier devrait exister...
        {
            Directory.CreateDirectory(appFolderPath);
        }

        try
        {
            var bktExistArgs = new BucketExistsArgs()
                .WithBucket(bucketName);
            var found = await minio.BucketExistsAsync(bktExistArgs).ConfigureAwait(false);

            if (!found)
            {
                var mkBktArgs = new MakeBucketArgs()
                    .WithBucket(bucketName);
                await minio.MakeBucketAsync(mkBktArgs).ConfigureAwait(true);
#if DEBUG
                await Shell.Current.DisplayAlert("Bucket créé: ", bucketName, "OK");
#endif
            }

                ListObjectsArgs args = new ListObjectsArgs()
                .WithBucket(bucketName)
                .WithRecursive(true)
                .WithVersions(true);
            IObservable<Item> observable = minio.ListObjectsAsync(args);

            var downloadTasks = new List<Task>();

            IDisposable subscription = observable.Subscribe(
                    item =>
                    {
                        Debug.WriteLine("OnNext: {0}", item.Key);
                        var downloadPath = Path.Combine(appFolderPath, item.Key);
                        Debug.WriteLine(downloadPath);
                        downloadTasks.Add(DownloadImageFromMinio(minio, bucketName, item.Key, downloadPath));
                    },
                    ex => Debug.WriteLine("OnError: {0}", ex.Message),
                    () => Debug.WriteLine("OnCompleted"));

            await Task.WhenAll(downloadTasks);
        }
        catch (MinioException minioEx)
        {
            await Shell.Current.DisplayAlert("Erreur Minio", minioEx.Message, "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erreur", ex.Message, "OK");
        }
    }

    private async Task DownloadImageFromMinio(IMinioClient minio, string bucketName = "my-bucket-name", string objectName = "my-objectname", string downloadPath = "my-file-name")
    {
        try
        {
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithFile(downloadPath);
            var stat = await minio.GetObjectAsync(getObjectArgs).ConfigureAwait(false);
        }
        catch (MinioException minioEx)
        {
            await Shell.Current.DisplayAlert("Erreur Minio", minioEx.Message, "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erreur", ex.Message, "OK");
        }
        Debug.WriteLine("EndDLIM");
    }

    //La  méthode suivante est utilisée pour tester l'affichage des images téléchargées dans le dossier de l'application
    private async Task AfficherImages()
    {
        var bucketName = _userSettings.Username;
        Console.WriteLine(bucketName);
        var appFolderPath = Path.Combine(FileSystem.AppDataDirectory, "images", bucketName);
        Console.WriteLine(appFolderPath);
        var files = Directory.GetFiles(appFolderPath);

        if (files.Length == 0)
        {
            await Shell.Current.DisplayAlert("Gallerie", "Aucune image à afficher", "OK");
        }

        foreach (var file in files)
        {
            await Shell.Current.DisplayAlert("Image", file, "OK");
        }
    }
}
