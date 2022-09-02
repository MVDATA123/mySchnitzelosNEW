using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using GCloud.Shared.Dto.Domain;
using mvdata.foodjet.Caching;
using mvdata.foodjet.Domain;
using mvdata.foodjet.Extensions;
using Optional;
using Optional.Unsafe;

namespace mvdata.foodjet.Filter
{
    public class DistanceMapFilter : AbstractMapFilter
    {
        public Option<int> MaxDistanceInMeter { get; set; }
        public Option<LatLng> CurrentLocation { get; set; }

        public DistanceMapFilter(Option<int> maxDistanceInMeter, Option<LatLng> currentLocation)
        {
            MaxDistanceInMeter = maxDistanceInMeter;
            CurrentLocation = currentLocation;
        }

        public override bool IsVisible(Marker marker)
        {
            if(Guid.TryParse((string)marker.Tag, out var guid) && CurrentLocation.HasValue && MaxDistanceInMeter.HasValue)
            {
                var isVisible = CachingHolder.Instance
                    .GetStoreByGuid(guid).Map(s => new LatLng(s.Latitude, s.Longitude).DistanceTo(CurrentLocation.ValueOrDefault()))
                    .Map(d => d < MaxDistanceInMeter.ValueOrDefault()).ValueOr(true);

                return isVisible;
            }

            return true;
        }

        public override bool IsVisible(StoreLocationDto store)
        {
            if (CurrentLocation.HasValue && MaxDistanceInMeter.HasValue)
            {
                return store.SomeNotNull()
                    .Map(s => new LatLng(s.Latitude, s.Longitude).DistanceTo(CurrentLocation.ValueOrDefault()))
                    .Map(d => d < MaxDistanceInMeter.ValueOrDefault()).ValueOr(true);
            }

            return true;
        }
    }
}