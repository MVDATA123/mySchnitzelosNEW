using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Locations;
using Android.Media;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Optional;
using Optional.Unsafe;

namespace mvdata.foodjet.RecycleView.StoreList
{
    public class StoreViewHolder : RecyclerView.ViewHolder { 
        public TextView Company { get; set; }
        public TextView Store { get; set; }
        public TextView Address { get; set; }
        public RelativeLayout LocationButton { get; set; }
        public TextView Distance { get; set; }
        public Option<LatLng> StoreLocation { get; set; }
        public Option<LatLng> UserLocation { get; set; }

        public Option<float> DistanctToStore
        {
            get
            {
                if (UserLocation.HasValue && StoreLocation.HasValue)
                {
                    return new Location(LocationManager.GpsProvider)
                    {
                        Latitude = StoreLocation.ValueOrDefault().Latitude,
                        Longitude = StoreLocation.ValueOrDefault().Longitude
                    }.DistanceTo(new Location(LocationManager.GpsProvider)
                    {
                        Latitude = UserLocation.ValueOrDefault().Latitude,
                        Longitude = UserLocation.ValueOrDefault().Longitude
                    }).SomeNotNull();
                }

                return 10f.None();
            }
        }

        

        public StoreViewHolder(View itemView, Action<int> itemClickedlistener, Action<int> locationClickListener) : base(itemView)
        {
            Company = itemView.FindViewById<TextView>(Resource.Id.txtStoreRecycleViewItemCompany);
            Store = itemView.FindViewById<TextView>(Resource.Id.txtStoreRecycleViewItemStore);
            Address = itemView.FindViewById<TextView>(Resource.Id.txtStoreRecycleViewItemAddress);
            LocationButton = itemView.FindViewById<RelativeLayout>(Resource.Id.layoutStoreRecycleViewItemDistance);
            LocationButton.Click += (sender, args) => locationClickListener(LayoutPosition);
            itemView.Click += (sender, args) => itemClickedlistener(LayoutPosition);
            Distance = itemView.FindViewById<TextView>(Resource.Id.txtStoreRecycleViewItemDistance);
        }
    }
}