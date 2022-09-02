using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using GCloud.Shared.Dto.Domain;
using mvdata.foodjet.Caching;
using mvdata.foodjet.Domain;
using mvdata.foodjet.Extensions;
using mvdata.foodjet.Filter;
using Optional;

namespace mvdata.foodjet.RecycleView.MapList
{
    public class MapListAdapter : RecyclerView.Adapter
    {
        public event EventHandler<StoreLocationDto> Clicked;

        private readonly List<VisibleStoreLocationDto> _stores;
        private List<StoreLocationDto> VisibleStoreLocationDtos => _stores.Where(s => s.IsVisible).Select(s => s.Store).ToList();

        private readonly Dictionary<Type, AbstractMapFilter> _filter = new Dictionary<Type, AbstractMapFilter>();

        private readonly Random _random = new Random();
        private Option<LatLng> _userLocation;

        public MapListAdapter(IEnumerable<StoreLocationDto> stores, Option<LatLng> userLocation)
        {
            _userLocation = userLocation;
            _stores = stores
                .Select(s => new VisibleStoreLocationDto(s, userLocation.Map(location => location.DistanceTo(s.StoreLocation))))
                .OrderBy(s => s.Distance.ValueOr(0))
                .ToList();
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is MapListViewHolder mapListViewHolder)
            {
                var store = VisibleStoreLocationDtos[position];
                mapListViewHolder.StoreAddress.Text = store.Address;
                mapListViewHolder.StoreName.Text = $"{store.Company.Name} - {store.Name}";
                _userLocation.Match(location =>
                    {
                        mapListViewHolder.StoreDistance.Visibility = ViewStates.Visible;
                        mapListViewHolder.StoreDistance.Text = (location.DistanceTo(store.StoreLocation) / 1000).ToString("0.00 km");
                    },
                    () => { mapListViewHolder.StoreDistance.Visibility = ViewStates.Gone; });
                CachingHolder.Instance.GetCompanyLogoAsBase64(store.Id).Match(base64 =>
                    {
                        mapListViewHolder.StoreLogo.Visibility = ViewStates.Visible;
                        Glide.With(mapListViewHolder.MainRoot).AsBitmap().Load(Convert.FromBase64String(base64)).Into(mapListViewHolder.StoreLogo);
                    },
                    () => { mapListViewHolder.StoreLogo.Visibility = ViewStates.Gone; });
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.fragment_map_store_list_item, parent, false);

            var vh = new MapListViewHolder(itemView, OnClicked);

            return vh;
        }

        public override int ItemCount => _stores.Count(x => x.IsVisible);

        protected virtual void OnClicked(int position)
        {
            Clicked?.Invoke(this, VisibleStoreLocationDtos[position]);
        }

        public void AddFilter(AbstractMapFilter filter, bool autoApply = true)
        {
            RemoveFilter(filter.GetType());
            _filter.Add(filter.GetType(), filter);

            if (autoApply)
            {
                ApplyFilters();
            }
            NotifyDataSetChanged();
        }

        public void ApplyFilters()
        {
            foreach (var store in _stores)
            {
                store.IsVisible = _filter.Values.Aggregate(true, (x, y) => x && y.IsVisible(store.Store));
            }
            NotifyDataSetChanged();
        }

        public void ClearFilters()
        {
            _filter.Clear();
        }

        public void RemoveFilter(Type type)
        {
            if (_filter.ContainsKey(type))
            {
                _filter.Remove(type);
            }
        }

        private class VisibleStoreLocationDto
        {
            public readonly StoreLocationDto Store;

            public VisibleStoreLocationDto(StoreLocationDto store, Option<double> distance)
            {
                Store = store;
                Distance = distance;
            }

            public string Address => Store.Address;

            public LatLng StoreLocation
            {
                get => Store.StoreLocation;
                set => Store.StoreLocation = value;
            }

            public double Longitude
            {
                get => Store.Longitude;
                set => Store.Longitude = value;
            }

            public double Latitude
            {
                get => Store.Latitude;
                set => Store.Latitude = value;
            }

            public Guid Id
            {
                get => Store.Id;
                set => Store.Id = value;
            }

            public string Name
            {
                get => Store.Name;
                set => Store.Name = value;
            }

            public string City
            {
                get => Store.City;
                set => Store.City = value;
            }

            public string Street
            {
                get => Store.Street;
                set => Store.Street = value;
            }

            public string HouseNr
            {
                get => Store.HouseNr;
                set => Store.HouseNr = value;
            }

            public string Plz
            {
                get => Store.Plz;
                set => Store.Plz = value;
            }

            public DateTime CreationDateTime
            {
                get => Store.CreationDateTime;
                set => Store.CreationDateTime = value;
            }

            public CompanyDto Company
            {
                get => Store.Company;
                set => Store.Company = value;
            }

            public CountryDto Country
            {
                get => Store.Country;
                set => Store.Country = value;
            }

            public ICollection<TagDto> Tags
            {
                get => Store.Tags;
                set => Store.Tags = value;
            }

            public bool IsUserFollowing
            {
                get => Store.IsUserFollowing;
                set => Store.IsUserFollowing = value;
            }

            public string BannerImage
            {
                get => Store.BannerImage;
                set => Store.BannerImage = value;
            }

            public bool HasTag(string tag)
            {
                return Store.HasTag(tag);
            }

            public bool IsVisible { get; set; } = true;
            public Option<double> Distance { get; set; } = Option.None<double>();
        }
    }
}