using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace mvdata.foodjet.Extensions
{
    public static class LocationExtensions
    {
        /// <summary>
        /// Returns the approximate Distance between the two locations in meters
        /// </summary>
        /// <param name="from">is the Startpoint</param>
        /// <param name="to">is the Targetpoint</param>
        /// <returns></returns>
        public static double DistanceTo(this LatLng from, LatLng to)
        {
            return new Location(LocationManager.GpsProvider)
            {
                Longitude = from.Longitude,
                Latitude = from.Latitude
            }.DistanceTo(new Location(LocationManager.GpsProvider)
            {
                Longitude = to.Longitude,
                Latitude = to.Latitude
            });
        }
    }
}