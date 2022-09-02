using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Support.Constraints;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using GCloud.Shared.Dto.Domain;
using GCloudShared.Domain;
using Java.Net;
using mvdata.foodjet.Caching;
using mvdata.foodjet.Domain;
using mvdata.foodjet.Extensions;
using mvdata.foodjet.Filter;
using mvdata.foodjet.RecycleView;
using mvdata.foodjet.RecycleView.CouponsList;
using Newtonsoft.Json;
using Optional;
using Optional.Collections;
using Optional.Unsafe;
using Refit;
using IFilterable = mvdata.foodjet.Filter.IFilterable;
using Uri = Android.Net.Uri;

namespace mvdata.foodjet.Fragments
{
    public class MyMapFragment : Fragment, IOnMapReadyCallback, IFilterable
    {
        private const string _tag = "MapsFiltering";

        //Components
        private CoordinatorLayout _mainRoot;
        private FloatingActionButton _fabMapNavigate;
        private MapView _mapView;
        private GoogleMap _map;

        //BottomSheet Components
        private TextView _txtBottomSheetHeaderText, _txtBottomSheetSubheaderText;
        private BottomSheetBehavior _bottomSheetBehavior;
        private ConstraintLayout _bottomSheetLayout;
        private ConstraintLayout _bottomSheetHeader;

        //Bottom sheet recyclerView
        private RecyclerView _recyclerView;
        private RecyclerView.LayoutManager _layoutManager;
        private CouponsAdapter _couponAdapter;
        private ImageView _btnBottomSheetLeft, _btnBottomSheetRight;

        //Logical structure
        private readonly SortedList<double, Marker> _markers = new SortedList<double, Marker>(new DuplicateKeyComparer<double>());

        private Option<int> _markerIndex = 0.None();
        private Option<StoreLocationDto> _initialStore = Option.None<StoreLocationDto>();
        private Option<LatLng> _myLocation = Option.None<LatLng>();
        private List<Option<CouponDto>> _coupons = new List<Option<CouponDto>>();
        private Option<Marker> _selectedMarker = Option.None<Marker>();
        private Dictionary<Type, AbstractMapFilter> _mapFilters = new Dictionary<Type, AbstractMapFilter>();

        public MyMapFragment(Option<LatLng> myLocation, string initialStoreId = null)
        {
            _myLocation = myLocation;
            if (initialStoreId != null && Guid.TryParse(initialStoreId, out var guid))
            {
                _initialStore = CachingHolder.Instance.GetStoreByGuid(guid);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_map_map, container, false);

            _mainRoot = view.FindViewById<CoordinatorLayout>(Resource.Id.mainRootMaps);
            _fabMapNavigate = view.FindViewById<FloatingActionButton>(Resource.Id.fabMapNavigate);
            _bottomSheetLayout = view.FindViewById<ConstraintLayout>(Resource.Id.bottomSheetBehavior);
            _bottomSheetHeader = view.FindViewById<ConstraintLayout>(Resource.Id.btnBottomSheetMapsTopBar);
            _bottomSheetHeader.Click += ToogleBottomSheet;
            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerViewBottomSheetMaps);
            _txtBottomSheetHeaderText = view.FindViewById<TextView>(Resource.Id.txtFragmentBottomLayoutHeader);
            _txtBottomSheetSubheaderText = view.FindViewById<TextView>(Resource.Id.txtFragmentBottomLayoutSubHeader);
            _btnBottomSheetLeft = view.FindViewById<ImageView>(Resource.Id.btnBottomSheetLeft);
            _btnBottomSheetRight = view.FindViewById<ImageView>(Resource.Id.btnBottomSheetRight);
            _mapView = view.FindViewById<MapView>(Resource.Id.mapView);

            _fabMapNavigate.Hide();
            _fabMapNavigate.Click += FabMapNavigateOnClick;
            _mapView.OnCreate(savedInstanceState);
            _mapView.OnResume();

            return view;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            MapsInitializer.Initialize(Activity.ApplicationContext);
            _mapView.GetMapAsync(this);

            _bottomSheetBehavior = BottomSheetBehavior.From(_bottomSheetLayout);

            _bottomSheetBehavior.SetBottomSheetCallback(new MyBottomSheetCallback(this));
            _layoutManager = new LinearLayoutManager(Activity);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.AddItemDecoration(new DividerItemDecoration(_recyclerView.Context, (int)Orientation.Vertical));
            _btnBottomSheetRight.Click += LoadNextStore;
            _btnBottomSheetLeft.Click += LoadLastStore;
            _couponAdapter = new CouponsAdapter(Activity, _coupons.Values().ToList());
            _couponAdapter.ItemClick += CouponAdapterOnItemClick;
            _recyclerView.SetAdapter(_couponAdapter);
        }

        private void FabMapNavigateOnClick(object sender, EventArgs e)
        {
            _selectedMarker.MatchSome(marker =>
            {
                if (Guid.TryParse((string)marker.Tag, out var guid))
                {
                    var store = CachingHolder.Instance.GetStoreByGuid(guid);
                    store.MatchSome(s =>
                    {
                        var destinationUrl = $"{s.Address}";
                        var uri = $"https://www.google.com/maps/dir/?api=1&destination={URLEncoder.Encode(destinationUrl, "UTF-8")}&travelmode=walking";
                        var intent = new Intent(Intent.ActionView, Uri.Parse(uri));
                        intent.SetPackage("com.google.android.apps.maps");
                        StartActivity(intent);
                    });
                }
            });

        }

        private void LoadNextStore(object sender, EventArgs e)
        {
            if (_markers.Count == 0)
            {
                _markerIndex = 0.None();
                return;
            }

            var firstIndex = _markers.IndexOfValue(_markers.Values.FirstOrDefault(m => m.Visible));

            if (firstIndex == -1)
            {
                //Es sind keine markierungen sichtbar, daher nichts machen
                return;
            }

            _markerIndex = _markers.IndexOfValue(_markers.Values.Skip(_markerIndex.ValueOr(-1) + 1).SkipWhile(m => !m.Visible).FirstOrDefault()).SomeNotNull();

            _markerIndex = _markerIndex.Map(index => index < 0 ? firstIndex : index);

            _markerIndex.MatchSome(markerIndex =>
            {
                var marker = _markers.Values[markerIndex];
                if (marker != null && Guid.TryParse((string)marker.Tag, out var storeId))
                {
                    CachingHolder.Instance.Stores.Values().FirstOrNone(s => s.Id == storeId).MatchSome(s =>
                    {
                        _map.AnimateCamera(CameraUpdateFactory.NewLatLng(new LatLng(s.Latitude, s.Longitude)));
                        _fabMapNavigate.Show();
                    });
                }

                ShowStoreInfosInBottomSheet(marker, false);
            }
            );
        }

        private void LoadLastStore(object sender, EventArgs e)
        {
            if (_markers.Count == 0)
            {
                _markerIndex = 0.None();
                return;
            }

            var lastIndex = _markers.Values.IndexOf(_markers.Values.Reverse().SkipWhile(m => !m.Visible).FirstOrDefault());
            if (lastIndex == -1)
            {
                //Es sind keine markierungen sichtbar, daher nichts machen
                return;
            }

            _markerIndex = _markers.IndexOfValue(_markers.Values.Reverse().Skip(_markers.Count - _markerIndex.ValueOr(_markers.Count)).SkipWhile(m => !m.Visible).FirstOrDefault()).SomeNotNull();

            _markerIndex = _markerIndex.Map(index => index < 0 ? lastIndex : index);

            _markerIndex.MatchSome(markerIndex =>
                {
                    var marker = _markers.Values[markerIndex];
                    if (marker != null && Guid.TryParse((string)marker.Tag, out var storeId))
                    {
                        CachingHolder.Instance.Stores.Values().FirstOrNone(s => s.Id == storeId).MatchSome(s =>
                        {
                            _map.AnimateCamera(CameraUpdateFactory.NewLatLng(new LatLng(s.Latitude, s.Longitude)));
                            _fabMapNavigate.Show();
                        });
                    }

                    ShowStoreInfosInBottomSheet(marker, false);
                }
            );
        }

        private void ToogleBottomSheet(object sender, EventArgs args)
        {
            if (_bottomSheetBehavior.State == BottomSheetBehavior.StateExpanded)
            {
                _bottomSheetBehavior.State = BottomSheetBehavior.StateCollapsed;
            }

            if (_bottomSheetBehavior.State == BottomSheetBehavior.StateCollapsed && _coupons.Count > 0)
            {
                _bottomSheetBehavior.State = BottomSheetBehavior.StateExpanded;
            }
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            if (googleMap != null && Activity is BaseActivity baseActivity)
            {
                _map = googleMap;
                _map.InfoWindowClick += MapOnInfoWindowClick;
                _map.MarkerClick += MapOnMarkerClick;
                _map.MapClick += OnMapClick;
                _map.UiSettings.MapToolbarEnabled = true;
                _map.SetPadding(0, baseActivity.SupportActionBar.Height, 0, _bottomSheetHeader.LayoutParameters.Height);

                DrawMarkers();

                _myLocation.MatchSome(myLocation =>
                {
                    Activity.RunOnUiThread(() =>
                    {
                        _map.MoveCamera(CameraUpdateFactory.NewLatLngZoom(myLocation, 15));
                        _mapFilters.Values.OfType<DistanceMapFilter>().FirstOrNone().Map(f => f.CurrentLocation = _myLocation);
                        ApplyFilters();
                        //Activity.RunOnUiThread(UpdatePointer);
                    });
                });
            }
        }

        private void UpdateCurrentLocationView()
        {
            if (_map != null && _myLocation.HasValue)
            {
                var cameraUpdate = CameraUpdateFactory.NewLatLngZoom(_myLocation.Map(x => new LatLng(x.Latitude, x.Longitude)).ValueOrDefault(), 15);
                Activity.RunOnUiThread(() => _map.MoveCamera(cameraUpdate));
            }
        }

        private void MapOnMarkerClick(object sender, GoogleMap.MarkerClickEventArgs e)
        {
            var marker = e.Marker;
            _fabMapNavigate.Show();
            ShowStoreInfosInBottomSheet(marker, true);
        }

        public void ShowStoreInfosInBottomSheet(Marker marker, bool expandBottomSheet)
        {
            if (Guid.TryParse((string) marker.Tag, out var storeId))
            {
                var store = CachingHolder.Instance.Stores.Values().FirstOrNone(s => s.Id == storeId);
                store.Match(s =>
                {
                    LoadCouponsOfStore(s);
                    _selectedMarker = marker.Some();
                    _txtBottomSheetHeaderText.Text = s.Name;
                    _txtBottomSheetSubheaderText.Text = $"{s.Street} {s.HouseNr}, {s.Plz} {s.City}";
                    if (expandBottomSheet)
                    {
                        _bottomSheetBehavior.State = BottomSheetBehavior.StateExpanded;
                    }

                    marker.ShowInfoWindow();
                    _fabMapNavigate.Show();
                }, OnMapClick);
            }
        }

        public void ShowStoreInfosInBottomSheet(StoreLocationDto store, bool expandBottomSheet)
        {
            var marker = _markers.Values.Where(m => Guid.TryParse((string) m.Tag, out var guid) && store.Id == guid).FirstOrNone();

            marker.Match(m =>
            {
                LoadCouponsOfStore(store);
                _selectedMarker = marker;
                _txtBottomSheetHeaderText.Text = store.Name;
                _txtBottomSheetSubheaderText.Text = $"{store.Street} {store.HouseNr}, {store.Plz} {store.City}";
                if (expandBottomSheet)
                {
                    _bottomSheetBehavior.State = BottomSheetBehavior.StateExpanded;
                }

                m.ShowInfoWindow();
                _fabMapNavigate.Show();
            }, OnMapClick);
        }

        private void LoadCouponsOfStore(StoreLocationDto store)
        {
            if (store != null)
            {
                _coupons = CachingHolder.Instance.GetCouponsByStore(store.Id);
                _couponAdapter = new CouponsAdapter(Activity, _coupons.Values().ToList());
                _couponAdapter.ItemClick += CouponAdapterOnItemClick;
                _recyclerView.SetAdapter(_couponAdapter);
            }
        }

        private void CouponAdapterOnItemClick(object sender, CouponDto coupon)
        {
            if (Activity is BaseActivity baseActivity && baseActivity.UserLoginMethod == UserLoginMethod.Anonymous)
            {
                Snackbar.Make(_mainRoot, GetString(Resource.String.couponNotValidBecauseNoAuth), Snackbar.LengthLong)
                    .Show();
                return;
            }

            if (coupon.IsValid)
            {
                var intent = new Intent(Activity, typeof(CouponDetailsActivity));
                intent.PutExtra("couponGuid", coupon.Id.ToString());
                StartActivity(intent);
            }
            else
            {
                Snackbar.Make(_mainRoot, GetString(Resource.String.couponNotValid), Snackbar.LengthLong).Show();
            }
        }

        private void OnMapClick()
        {
            OnMapClick(null, null);
        }

        private void OnMapClick(object sender, GoogleMap.MapClickEventArgs e)
        {
            _fabMapNavigate.Hide();
            _selectedMarker = Option.None<Marker>();
            _txtBottomSheetHeaderText.Text = GetString(Resource.String.mapsCouponsNearMe);
            _txtBottomSheetSubheaderText.Text = string.Empty;
            _bottomSheetBehavior.State = BottomSheetBehavior.StateCollapsed;
        }

        private void MapOnInfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e)
        {
            if (Activity is BaseActivity baseActivity && baseActivity.UserLoginMethod == UserLoginMethod.Normal)
            {
                var marker = e.Marker;
                if (marker != null && Guid.TryParse((string) marker.Tag, out var storeGuid))
                {
                    var store = CachingHolder.Instance.Stores.Values().FirstOrNone(s => s.Id == storeGuid);
                    store.MatchSome(s =>
                    {
                        var intent = new Intent(baseActivity, typeof(CouponsListActivity));
                        intent.PutExtra("storeGuid", s.Id.ToString());
                        intent.PutExtra("storeName", s.Name);
                        intent.PutExtra("storeCashbackEnabled", s.Company.IsCashbackEnabled);
                        intent.PutExtra("isUserAlreadyFollowing", s.IsUserFollowing);
                        StartActivity(intent);
                    });
                }
            }
            else
            {
                Snackbar.Make(Activity.Window.DecorView.FindViewById(Android.Resource.Id.Content),
                    GetString(Resource.String.functionNotAvailableAsAnonymous), Snackbar.LengthLong).Show();
            }
        }

        private void DrawMarkers()
        {
            var stores = CachingHolder.Instance.Stores.Values();
            _myLocation.MatchSome(myLocation => stores = stores.OrderBy(store => store.StoreLocation.DistanceTo(myLocation)));

            foreach (var store in stores.ToList())
            {
                var storeDistance = 0.0d;
                _myLocation.MatchSome(x => storeDistance = store.StoreLocation.DistanceTo(x));

                if ((store?.StoreLocation?.Latitude ?? 0) == 0 && (store?.StoreLocation?.Longitude ?? 0) == 0)
                {
                    continue;
                }

                var address = store.Address;
                var name = store.Name;

                var distance = 0d;
                _myLocation.MatchSome(myLocation => distance = myLocation.DistanceTo(store.StoreLocation));

                var markerOptions = new MarkerOptions();
                markerOptions.SetTitle(name);
                markerOptions.SetSnippet(address);
                markerOptions.SetPosition(store.StoreLocation);

                //builder.Include(marker.Position);
                Marker marker = null;

                Activity.RunOnUiThread(() =>
                {
                    marker = _map.AddMarker(markerOptions);
                    marker.Tag = store.Id.ToString();
                    marker.Visible = _mapFilters.Values.Aggregate(true, (x, y) => x && y.IsVisible(marker));
                    _markers.Add(distance, marker);
                });
            }
        }

        public void AddFilter(AbstractMapFilter filter, bool applyFilter = true)
        {
            RemoveFilter(filter.GetType());
            _mapFilters.Add(filter.GetType(), filter);

            if (applyFilter)
            {
                ApplyFilters();
            }
        }

        public void ApplyFilters()
        {
            foreach (var marker in _markers.Values)
            {
                Activity.RunOnUiThread(() => marker.Visible = _mapFilters.Values.Aggregate(true, (x, y) => x && y.IsVisible(marker)));
            }
        }

        public void RemoveFilter(Type filterType)
        {
            if (_mapFilters.ContainsKey(filterType))
            {
                _mapFilters.Remove(filterType);
            }
        }

        public void ClearFilter()
        {
            _mapFilters.Clear();
            ApplyFilters();
        }

        public void NavigateToStore(StoreLocationDto store)
        {
            if (store != null)
            {
                NavigateToStore(store.Id);
            }
        }

        public void NavigateToStore(Guid storeGuid)
        {
            var marker = _markers.Values.Where(m => Guid.TryParse((string) m.Tag, out var guid) && guid == storeGuid).FirstOrNone();
            marker.MatchSome(m =>
            {
                var cameraUpdate = CameraUpdateFactory.NewLatLngZoom(m.Position, 15);
                Activity.RunOnUiThread(() =>_map.AnimateCamera(cameraUpdate));
            });
        }

        private class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
        {
            public int Compare(TKey x, TKey y)
            {
                int result = x.CompareTo(y);

                if (result == 0)
                    return 1; // Handle equality as beeing greater
                else
                    return result;
            }
        }

        private class MyBottomSheetCallback : BottomSheetBehavior.BottomSheetCallback
        {
            private readonly MyMapFragment _mapFragment;

            public MyBottomSheetCallback(MyMapFragment mapFragment)
            {
                _mapFragment = mapFragment;
            }

            public override void OnSlide(View bottomSheet, float slideOffset)
            {
            }

            public override void OnStateChanged(View bottomSheet, int newState)
            {
                    _mapFragment._selectedMarker.Match(marker =>
                            _mapFragment._bottomSheetBehavior.State = newState,
                        () =>
                        {
                            _mapFragment._bottomSheetBehavior.State = BottomSheetBehavior.StateCollapsed;
                        });
            }
        }
    }
}