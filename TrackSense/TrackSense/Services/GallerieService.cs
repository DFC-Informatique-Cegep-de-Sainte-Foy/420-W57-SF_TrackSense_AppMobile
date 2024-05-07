using TrackSense.Entities;
using TrackSense.Configurations;
using ExifLibrary;
using Minio.DataModel.Args;
using Minio;
using System.Net;
using Minio.Exceptions;
using Minio.DataModel;

namespace TrackSense.Services;

public class GallerieService
{

    UserService _userService;
    IConfigurationManager _configuration;
    Settings _userSettings;

    public GallerieService(UserService userService, IConfigurationManager configurationManager)
    {
        _configuration = configurationManager;
        //_userService = userService;
        _userSettings = _configuration.LoadSettings();
    }

    public async Task PrendrePhoto()
    {
        _userSettings = _configuration.LoadSettings();
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
 
        try
        {
            var bktExistArgs = new BucketExistsArgs()
                .WithBucket(bucketName);
            var found = await minio.BucketExistsAsync(bktExistArgs).ConfigureAwait(false);

            if (!found)
            {
                await Shell.Current.DisplayAlert("Erreur", "Le bucket n'existe pas", "OK");
                return;
            }          

            ListObjectsArgs args = new ListObjectsArgs()
                .WithBucket(bucketName)
                //.WithPrefix("prefix")
                .WithRecursive(true)
                .WithVersions(true);
            IObservable<Item> observable = minio.ListObjectsAsync(args);
            IDisposable subscription = observable.Subscribe(
                    item => Console.WriteLine("OnNext: {0}", item.Key),
                    ex => Console.WriteLine("OnError: {0}", ex.Message),
                    () => Console.WriteLine("OnComplete: {0}"));
#if DEBUG
            var list = await minio.ListBucketsAsync().ConfigureAwait(false);
            foreach (var bucket in list.Buckets)
            {
                Console.WriteLine(bucket.Name + " " + bucket.CreationDateDateTime);
            }
#endif
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
}
