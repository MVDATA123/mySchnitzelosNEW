using System.Linq;
using System.Threading.Tasks;
using CoreLocation;
using GCloudiPhone.Extensions;

namespace GCloudiPhone.Location
{
    public static class LocationHelper
    {
        public static async Task<CLLocationCoordinate2D> GeocodeAddress(StoreLocationDto store)
        {
            var geocoder = new CLGeocoder();
            var locations = await geocoder.GeocodeAddressAsync(store.Address);
            store.Latitude = locations.FirstOrDefault().Location.Coordinate.Latitude;
            store.Longitude = locations.FirstOrDefault().Location.Coordinate.Longitude;
            return new CLLocationCoordinate2D(store.Latitude, store.Longitude);
        }
    }
}
