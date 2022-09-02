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
using Android.Util;
using Android.Views;
using Android.Widget;
using mvdata.foodjet.Caching;
using mvdata.foodjet.Domain;
using mvdata.foodjet.Filter;
using mvdata.foodjet.RecycleView.MapList;
using Optional;
using Optional.Collections;
using Fragment = Android.Support.V4.App.Fragment;
using IFilterable = mvdata.foodjet.Filter.IFilterable;

namespace mvdata.foodjet.Fragments
{
    public class MapStoreListFragment : Fragment, IFilterable
    {
        private readonly List<StoreLocationDto> _stores;

        private RecyclerView _recyclerView;
        private RecyclerView.LayoutManager _layoutManager;
        private readonly MapListAdapter _adapter;
        private readonly Option<LatLng> _myLocation;

        private IStoreClickedListener _storeClickedListener;

        public MapStoreListFragment(Option<LatLng> myLocation)
        {
            _myLocation = myLocation;
            _stores = CachingHolder.Instance.Stores.Values().ToList();
            _adapter = new MapListAdapter(_stores, _myLocation);
        }

        public override void OnAttach(Context context)
        {
            base.OnAttach(context);
            if (Activity is IStoreClickedListener storeClickedListener)
            {
                _storeClickedListener = storeClickedListener;
            }
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            _layoutManager = new LinearLayoutManager(Activity);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.SetAdapter(_adapter);
            _adapter.Clicked += delegate(object sender, StoreLocationDto store)
            {
                _storeClickedListener.OnListStoreClicked(store);
            };

            var separator = new DividerItemDecoration(_recyclerView.Context, (int)Orientation.Vertical);
            _recyclerView.AddItemDecoration(separator);
            
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_map_store_list, container, false);

            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerView);

            return view;
        }

        public void AddFilter(AbstractMapFilter filter, bool autoApply = true)
        {
            _adapter.AddFilter(filter, autoApply);
        }

        public void ApplyFilters()
        {
            _adapter.ApplyFilters();
        }

        public void ClearFilter()
        {
            _adapter.ClearFilters();
        }

        public void RemoveFilter(Type filterType)
        {
            _adapter.RemoveFilter(filterType);
        }
    }

    public interface IStoreClickedListener
    {
        void OnListStoreClicked(StoreLocationDto store);
    }
}