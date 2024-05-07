using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackSense.Services
{
    public class LocationService
    {
        public LocationService()
        {
            ;
        }

        public async Task<Location> GetLocationAsync()
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
    }
}
