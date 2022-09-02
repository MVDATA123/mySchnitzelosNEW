using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Location;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Firebase.Messaging;
using GCloudShared.Domain;
using GCloudShared.Extensions;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using mvdata.foodjet.Caching;
using mvdata.foodjet.Domain;
using mvdata.foodjet.RecycleView;
using mvdata.foodjet.RecycleView.StoreList;
using Newtonsoft.Json;
using Optional;
using Optional.Collections;
using Refit;
using ZXing.Mobile;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using SearchView = Android.Support.V7.Widget.SearchView;

namespace mvdata.foodjet
{
    [Activity(Label = "Filialen", MainLauncher = false, LaunchMode = LaunchMode.SingleTop, ScreenOrientation = ScreenOrientation.Portrait)]
    [MetaData("android.app.searchable", Resource = "@xml/store_searchable")]
    [IntentFilter(new[] { Intent.ActionSearch })]
    public class StoresListActivity : BaseActivity, SearchView.IOnQueryTextListener, IMenuItemOnActionExpandListener
    {
        private FloatingActionButton _fab;
        Toolbar _toolbar;
        private TextView _txtEmptyList;
        private RecyclerView _recyclerView;
        private RecyclerView.LayoutManager _layoutManager;
        private StoreAdapter _storeAdapter;
        private FusedLocationProviderClient _locationProviderClient;
        private Option<LatLng> _myLocation;
        private RelativeLayout _mainRoot;
        private ProgressBar _progressBar;
        private SwipeRefreshLayout _swipeRefreshLayout;

        private Option<bool> _hasLocationPermissions;
        private Option<bool> _hasCameraPermissions;
        private bool _locationPermissionRequestPending = true;
        private bool _cameraPermissionRequestPending = true;

        private List<StoreLocationDto> _stores;
        private List<StoreLocationDto> _allStores;

        private SubscriptionRepository _subscriptionRepository;
        private IStoreService _storeService;
        private IUserStoreService _userStoreService;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _subscriptionRepository = new SubscriptionRepository(DbBootstraper.Connection);
            _storeService = RestService.For<IStoreService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
            _userStoreService = RestService.For<IUserStoreService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);

            _stores = CachingHolder.Instance.Stores.Values().Where(s => s.IsUserFollowing).ToList();

            SetContentView(Resource.Layout.StoresList);

            _locationProviderClient = LocationServices.GetFusedLocationProviderClient(this);
            
            _txtEmptyList = FindViewById<TextView>(Resource.Id.txtStoresListEmpty);
            _fab = FindViewById<FloatingActionButton>(Resource.Id.floating_action_button);
            _toolbar = FindViewById<Toolbar>(Resource.Id.app_bar);
            _recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerViewStoresList);
            _mainRoot = FindViewById<RelativeLayout>(Resource.Id.mainRootStoresList);
            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBarStoresList);
            _swipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeLayoutStoresList);

            _layoutManager = new LinearLayoutManager(this);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.AddItemDecoration(new DividerItemDecoration(_recyclerView.Context,(int)Orientation.Vertical));
            _progressBar.Visibility = ViewStates.Visible;

            _fab.Click += FabOnClick;
            _swipeRefreshLayout.Refresh += ReloadStores;

            CheckLocationPerissions();

            while (_locationPermissionRequestPending)
            {
                await Task.Delay(20);
            }

            CheckCameraPerissions();

            while (_cameraPermissionRequestPending)
            {
                await Task.Delay(20);
            }

            if (_hasLocationPermissions.HasValue && _hasLocationPermissions.Contains(true))
            {
                var location = (await _locationProviderClient.GetLastLocationAsync()).SomeNotNull();
                _myLocation = location.Map(x => new LatLng(x.Latitude, x.Longitude));
            }

            if (!_hasCameraPermissions.HasValue || _hasCameraPermissions.Contains(false))
            {
                _fab.Visibility = ViewStates.Gone;
            }
            else
            {
                _fab.Visibility = ViewStates.Visible;
            }

            _progressBar.Visibility = ViewStates.Gone;

            _storeAdapter = new StoreAdapter(this, _stores, _myLocation.Map(x => new LatLng(x.Latitude, x.Longitude)), false);
            _storeAdapter.ItemClick += ListViewStoresOnItemClick;
            _storeAdapter.LocationClick += StoreAdapterOnLocationClick;
            _recyclerView.SetAdapter(_storeAdapter);

            if (_stores.Any())
            {
                _txtEmptyList.Visibility = ViewStates.Gone;
                _swipeRefreshLayout.Visibility = ViewStates.Visible;
            }
            _swipeRefreshLayout.Refreshing = false;

            //new Thread(async delegate()
            //{
            //    try
            //    {
            //        var stores = (await _storeService.GetStores()).Select(s => new StoreLocationDto(s).SomeNotNull()).ToList();
            //        CachingHolder.Instance.Stores = stores;
            //        _stores = CachingHolder.Instance.Stores.Values().Where(s => s.IsUserFollowing).ToList();
            //        _storeAdapter = new StoreAdapter(this, _stores, _myLocation, false);
            //        FillStoresAdapter();
            //        RunOnUiThread(() =>
            //        {
            //            if (_stores.Any())
            //            {
            //                _txtEmptyList.Visibility = ViewStates.Gone;
            //                _swipeRefreshLayout.Visibility = ViewStates.Visible;
            //            }
            //            _swipeRefreshLayout.Refreshing = false;
            //        });
            //    }
            //    catch (ApiException e)
            //    {
            //        if (e.StatusCode == 0)
            //        {
            //            _swipeRefreshLayout.Refreshing = false;
            //            return;
            //        }
            //        RunOnUiThread(() => e.ShowApiErrorResultSnackbar(_mainRoot));
            //    }
            //}).Start();
        }

        private void ReloadStores(object sender, EventArgs e)
        {
            LoadStores();
        }

        private async void LoadStores()
        {
            RunOnUiThread(() => _swipeRefreshLayout.Refreshing = true);
            try
            {
                var stores = (await _userStoreService.GetUserStores()).Select(x => new StoreLocationDto(x)).ToList();

                _stores.Clear();

                if (stores.Any())
                {
                    _stores.AddRange(stores);
                    _storeAdapter.NotifyDataSetChanged();
                    _swipeRefreshLayout.Visibility = ViewStates.Visible;
                    _txtEmptyList.Visibility = ViewStates.Gone;
                }
                else
                {
                    _swipeRefreshLayout.Visibility = ViewStates.Gone;
                    _txtEmptyList.Visibility = ViewStates.Visible;
                }
            }
            catch (ApiException apiException)
            {
                if (apiException.StatusCode == 0)
                {
                    return;
                }

                apiException.ShowApiErrorResultSnackbar(_mainRoot);
            }
            finally
            {
                RunOnUiThread(() => _swipeRefreshLayout.Refreshing = false);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.StoresListMenu, menu);

            // Associate searchable configuration with the SearchView
            var searchManager =
                (SearchManager)GetSystemService(Context.SearchService);
            var searchViewMenuItem =
                menu.FindItem(Resource.Id.StoresListMenuSearch);
            var searchView = (SearchView)searchViewMenuItem.ActionView;
            searchView.SetSearchableInfo(searchManager.GetSearchableInfo(ComponentName));
            searchView.SetOnQueryTextListener(this);
            searchViewMenuItem.SetOnActionExpandListener(this);

            return true;
        }

        private void ListViewStoresOnItemClick(object sender, StoreLocationDto store)
        {
            var intent = new Intent(this, typeof(CouponsListActivity));
            intent.PutExtra("storeGuid", store.Id.ToString());
            intent.PutExtra("storeName", store.Name);
            intent.PutExtra("storeCashbackEnabled", store.Company?.IsCashbackEnabled ?? false);
            intent.PutExtra("isUserAlreadyFollowing", store.IsUserFollowing);
            StartActivity(intent);
            Finish();
        }

        protected override void OnResume()
        {
            SetSupportActionBar(_toolbar);
            SupportActionBar.SetTitle(Resource.String.filialeActivityHeader);
            base.OnResume();
        }

        private async void FabOnClick(object sender, EventArgs eventArgs)
        {
            if (_hasCameraPermissions.Contains(false))
            {
                return;
            }

#if __ANDROID__
            MobileBarcodeScanner.Initialize(Application);
#endif
            var scanner = new MobileBarcodeScanner();
            var result = await scanner.Scan();
            if (result != null && Guid.TryParse(result.Text, out var storeGuid))
            {
                try
                {
                    await _userStoreService.AddToWatchList(result.Text);
                    _subscriptionRepository.Insert(new Subscription
                    {
                        StoreId = storeGuid
                    });
                    FirebaseMessaging.Instance.SubscribeToTopic(storeGuid.ToString());
                    LoadStores();
                }
                catch (ApiException apiException)
                {
                    apiException.ShowApiErrorResultSnackbar(_mainRoot);
                }
            }
            else
            {
                Snackbar.Make(_mainRoot, Resource.String.invalidQrCode, Snackbar.LengthLong).Show();
            }
        }

        private void StoreAdapterOnLocationClick(object sender, StoreLocationDto store)
        {
            var intent = new Intent(this, typeof(MapsActivity));
            intent.PutExtra("storeJson", JsonConvert.SerializeObject(store));
            StartActivity(intent);
        }

        private void CheckLocationPerissions()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.AccessFineLocation, Manifest.Permission.AccessCoarseLocation }, 1);
            }
            else
            {
                _hasLocationPermissions = true.Some();
                _locationPermissionRequestPending = false;
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == 1)
            {
                if (grantResults.Any() && grantResults.All(x => x == Android.Content.PM.Permission.Granted))
                {
                    _hasLocationPermissions = true.Some();
                }
                _locationPermissionRequestPending = false;
            }
            if (requestCode == 2)
            {
                if (grantResults.Any() && grantResults.All(x => x == Android.Content.PM.Permission.Granted))
                {
                    _hasCameraPermissions = true.Some();
                }
                _cameraPermissionRequestPending = false;
            }
        }

        public bool OnQueryTextChange(string newText)
        {
            if (string.IsNullOrWhiteSpace(newText)) return false;
            SearchStoresByTag(newText);
            return true;
        }

        public bool OnQueryTextSubmit(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return false;
            SearchStoresByTag(query);
            return true;
        }

        private void SearchStoresByTag(string query)
        {
            new Thread(delegate()
            {
                RunOnUiThread(async () =>
                {
                    if (_allStores == null)
                    {
                        _allStores = (await _storeService.GetStores()).Select(s => new StoreLocationDto(s)).ToList();
                    }

                    var searchResult = _allStores.Where(s => s.HasTag(query)).ToList();

                    var location = (await _locationProviderClient.GetLastLocationAsync()).SomeNotNull();
                    _myLocation = location.Map(x => new LatLng(x.Latitude, x.Longitude));
                    _stores.Clear();
                    _stores.AddRange(searchResult);
                    _storeAdapter.NotifyDataSetChanged();
                });
            }).Start();
        }

        public bool OnMenuItemActionCollapse(IMenuItem item)
        {
            return true;
        }

        public bool OnMenuItemActionExpand(IMenuItem item)
        {
            return true;
        }
        public override void OnShowNoInternetMessageSuccess()
        {
            base.OnShowNoInternetMessageSuccess();
            Finish();
        }

        private void CheckCameraPerissions()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) != Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.Camera}, 2);
            }
            else
            {
                _hasCameraPermissions = true.Some();
                _cameraPermissionRequestPending = false;
            }
        }
    }
}