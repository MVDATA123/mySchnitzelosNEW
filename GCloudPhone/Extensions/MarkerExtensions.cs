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
using mvdata.foodjet.Caching;
using mvdata.foodjet.Domain;
using Optional;

namespace mvdata.foodjet.Extensions
{
    public static class MarkerExtensions
    {
        public static Option<StoreLocationDto> GetStore(this Marker marker)
        {
            if (Guid.TryParse((string) marker.Tag, out var storeGuid))
            {
                return CachingHolder.Instance.GetStoreByGuid(storeGuid);
            }

            return Option.None<StoreLocationDto>();
        }
    }
}