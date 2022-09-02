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
using mvdata.foodjet.Domain;

namespace mvdata.foodjet.Filter
{
    public abstract class AbstractMapFilter
    {
        public abstract bool IsVisible(Marker marker);

        public abstract bool IsVisible(StoreLocationDto store);
    }
}