using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using GCloud.Shared.Dto.Domain;
using mvdata.foodjet.Domain;
using Optional;
using Optional.Unsafe;

namespace mvdata.foodjet.RecycleView.StoreList
{
    public class StoreAdapter : RecyclerView.Adapter
    {

        public SortedList<double, StoreLocationDto> Stores { get; set; }
        public event EventHandler<StoreLocationDto> ItemClick;
        public event EventHandler<StoreLocationDto> LocationClick;
        public override int ItemCount => Stores.Count;

        private Context Context { get; set; }

        public Option<LatLng> UserLocation { get; set; } = Option.None<LatLng>();

        public StoreAdapter(Context context, List<StoreLocationDto> stores)
        {
            Context = context;
            Stores = new SortedList<double, StoreLocationDto>(stores.ToDictionary(x => (double)x.Name.ToCharArray().FirstOrDefault(), x => x, new DuplicateKeyComparer<double>()), new DuplicateKeyComparer<double>());
        }

        public StoreAdapter(Context context, List<StoreLocationDto> stores, Option<LatLng> userLocation, bool limitDistance) : this(context, stores)
        {
            UserLocation = userLocation;
            userLocation
                .Map(x => new Location(LocationManager.GpsProvider) { Latitude = x.Latitude, Longitude = x.Longitude })
                .Match(
                location =>
                {
                    if (limitDistance)
                    {
                        var tmpStores = stores;
                        //.Where(x => new Location(LocationManager.GpsProvider)
                        //{
                        //    Longitude = x.StoreLocation.Longitude,
                        //    Latitude = x.StoreLocation.Latitude
                        //}.DistanceTo(userL.ValueOrDefault()) <= 10000);
                        Stores = new SortedList<double, StoreLocationDto>(tmpStores.ToDictionary(
                            x => (double)new Location(LocationManager.GpsProvider)
                            {
                                Longitude = x.StoreLocation.Longitude,
                                Latitude = x.StoreLocation.Latitude
                            }.DistanceTo(location), x => x));
                    }
                    else
                    {
                        Stores = new SortedList<double, StoreLocationDto>(stores.ToDictionary(x => (double)new Location(LocationManager.GpsProvider) { Longitude = x.StoreLocation.Longitude, Latitude = x.StoreLocation.Latitude }.DistanceTo(location), x => x, new DuplicateKeyComparer<double>()), new DuplicateKeyComparer<double>());
                    }
                },
                () =>
                {
                    Stores = new SortedList<double, StoreLocationDto>(stores.ToDictionary(x => (double)x.Name.ToCharArray().FirstOrDefault(), x => x, new DuplicateKeyComparer<double>()), new DuplicateKeyComparer<double>());
                });
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is StoreViewHolder storeViewHolder)
            {

                storeViewHolder.Company.Text = Stores.Values[position].Company.Name;
                storeViewHolder.Store.Text = Stores.Values[position].Name;
                storeViewHolder.Address.Text = $"{Stores.Values[position].Street} {Stores.Values[position].HouseNr}";
                storeViewHolder.StoreLocation = Stores.Values[position].StoreLocation.Some().NotNull();
                storeViewHolder.UserLocation = UserLocation;
                if (!storeViewHolder.UserLocation.HasValue)
                {
                    storeViewHolder.LocationButton.Visibility = ViewStates.Gone;
                    storeViewHolder.LocationButton.Clickable = false;
                }
                var distance = storeViewHolder.DistanctToStore;
                if (distance.HasValue)
                {
                    if (distance.ValueOr(0f) >= 1000)
                    {
                        storeViewHolder.Distance.Text =
                            $"{Math.Round(distance.ValueOr(0) / 1000, 2).ToString(CultureInfo.InvariantCulture)}km";
                    }
                    else
                    {
                        storeViewHolder.Distance.Text =
                            $"{Math.Round(distance.ValueOr(0)).ToString(CultureInfo.InvariantCulture)}m";
                    }
                }
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.StoreRecycleViewItem, parent, false);

            var vh = new StoreViewHolder(itemView, OnItemClick, OnLocationClick);

            return vh;
        }

        void OnItemClick(int position)
        {
            ItemClick?.Invoke(this, Stores.Values[position]);
        }

        protected virtual void OnLocationClick(int position)
        {
            LocationClick?.Invoke(this, Stores.Values[position]);
        }

        private class DuplicateKeyComparer<TKey>
            :
                IComparer<TKey>, IEqualityComparer<TKey> where TKey : IComparable
        {
            #region IComparer<TKey> Members

            public int Compare(TKey x, TKey y)
            {
                int result = x.CompareTo(y);

                if (result == 0)
                    return 1;   // Handle equality as beeing greater
                else
                    return result;
            }

            #endregion

            public bool Equals(TKey x, TKey y)
            {
                return false;
            }

            public int GetHashCode(TKey obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}