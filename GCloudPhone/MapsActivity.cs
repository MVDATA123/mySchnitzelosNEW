using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Support.Constraints;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using GCloud.Shared.Dto.Domain;
using GCloudShared.Domain;
using mvdata.foodjet.Extensions;
using GCloudShared.Service;
using GCloudShared.Shared;
using mvdata.foodjet.Caching;
using mvdata.foodjet.Domain;
using mvdata.foodjet.Filter;
using mvdata.foodjet.RecycleView;
using mvdata.foodjet.RecycleView.CouponsList;
using Newtonsoft.Json;
using Optional;
using Optional.Collections;
using Optional.Unsafe;
using Refit;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V4.View;
using Android.Text;
using Android.Util;
using Java.Lang;
using Java.Net;
using mvdata.foodjet.Adapter.ViewPager;
using mvdata.foodjet.Fragments;
using Exception = System.Exception;
using IComparable = System.IComparable;
using Math = Java.Lang.Math;
using String = System.String;
using Thread = System.Threading.Thread;
using Uri = Android.Net.Uri;

namespace mvdata.foodjet
{
    [Activity(Label = "Karte", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MapsActivity : BaseActivity, ITextWatcher, IStoreClickedListener
    {
        //Map Fragment
        private GoogleMap _map;
        private DrawerLayout _drawerLayout;
        private Toolbar _toolbar;
        private ViewPager _viewPager;
        private TabLayout _tabLayout;
        private TextView _kitchenLayoutSelected;
        private LinearLayout _kitchenLayout;

        //Bottom sheet
        private SeekBar _distanceSeekbar;
        private Button _filterResetButton;
        private EditText _tagsSearchEditText;
        private List<TagDto> _allTags;
        private TagAdapter _tagAdapter;

        //Fragments
        private MyFragmentPagerAdapter _pageAdapter;
        private MyMapFragment _mapFragment;
        private MapStoreListFragment _mapStoreListFragment;

        //Logical Components
        private FusedLocationProviderClient _locationProvider;
        private Option<bool> _hasLocationPermissions = Option.None<bool>();
        private bool _permissionRequestPending = true;
        private bool _locationRequestPending = true;
        private readonly MapLocationCallback _locationCallback = new MapLocationCallback();
        private Option<LatLng> _myLocation;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var locationTask = InitLocation();

            CheckLocationPermissions();

            _allTags = CachingHolder.Instance.Tags;
            _tagAdapter = new TagAdapter(_allTags);
            _locationProvider = LocationServices.GetFusedLocationProviderClient(this);

            SetContentView(Resource.Layout.Maps);

            _distanceSeekbar = FindViewById<SeekBar>(Resource.Id.distanceSeekbar);
            _drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            _toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            _tabLayout = FindViewById<TabLayout>(Resource.Id.tabLayout);
            _viewPager = FindViewById<ViewPager>(Resource.Id.viewPager);
            _tabLayout.SetupWithViewPager(_viewPager);
            _pageAdapter = new MyFragmentPagerAdapter(SupportFragmentManager);
            _kitchenLayoutSelected = FindViewById<TextView>(Resource.Id.kitchenLayoutSelected);
            _filterResetButton = FindViewById<Button>(Resource.Id.MapsFilterReset);
            _kitchenLayout = FindViewById<LinearLayout>(Resource.Id.kitchenLayout);
            _kitchenLayout.Click += KitchenLayoutOnClick;
            var mDrawerToggle = new Android.Support.V7.App.ActionBarDrawerToggle(this, _drawerLayout, Resource.String.CashbackListHeader, Resource.String.CashbackListHeader);
            _drawerLayout.AddDrawerListener(mDrawerToggle);

            while (_permissionRequestPending)
            {
                await Task.Delay(20);
            }

            await locationTask;

            _mapFragment = new MyMapFragment(_myLocation);
            _mapStoreListFragment = new MapStoreListFragment(_myLocation);

            _locationCallback.LocationUpdated += LocationChanged;

            _pageAdapter.AddFragment(_mapFragment, "Karte");
            _pageAdapter.AddFragment(_mapStoreListFragment, "Liste");
            _viewPager.Adapter = _pageAdapter;

            SetFilterInitialData();

            try
            {
                if (_hasLocationPermissions.ValueOr(false))
                {
                    await _locationProvider.RequestLocationUpdatesAsync(LocationRequest.Create()
                        .SetInterval(TimeSpan.FromSeconds(30).Milliseconds)
                        .SetFastestInterval(TimeSpan.FromSeconds(20).Milliseconds)
                        .SetPriority(LocationRequest.PriorityHighAccuracy), _locationCallback);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void CheckLocationPermissions()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.AccessFineLocation, Manifest.Permission.AccessCoarseLocation }, 1);
            }
            else
            {
                _hasLocationPermissions = true.Some();
                _permissionRequestPending = false;
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
                else
                {
                    _hasLocationPermissions = false.Some();
                }
            }
            _permissionRequestPending = false;
        }

        private async Task<Option<LatLng>> InitLocation()
        {
            while (!_hasLocationPermissions.HasValue)
            {
                await Task.Delay(20);
            }
            if (_hasLocationPermissions.ValueOr(false))
            {
                var location = (await _locationProvider.GetLastLocationAsync()).SomeNotNull();
                _myLocation = location.Map(x => new LatLng(x.Latitude, x.Longitude));
                _locationRequestPending = false;
            }
            else
            {
                _myLocation = Option.None<LatLng>();
            }

            return _myLocation;
        }

        private void LocationChanged(object sender, Location e)
        {
            _myLocation = e.SomeNotNull().Map(l => new LatLng(l.Latitude, l.Longitude));
            //UpdatePointer();
        }

        /// <summary>
        /// Trigger a navigation to the specified location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KitchenLayoutOnClick(object sender, EventArgs args)
        {
            var dialogBuilder = new AlertDialog.Builder(this);
            var inflater = LayoutInflater;
            var dialogView = inflater.Inflate(Resource.Layout.MapsDialogTag, null);
            dialogBuilder.SetView(dialogView);
            var alertDialog = dialogBuilder.Create();
            alertDialog.Show();

            var kitchenFilterConfirmButton = alertDialog.FindViewById<Button>(Resource.Id.MapsDialogTagSave);
            kitchenFilterConfirmButton.Click += delegate (object o, EventArgs eventArgs)
            {
                var selectedTags = _tagAdapter.Selected.Select(t => t.Name).ToList();
                _pageAdapter.AddFilter(new TagMapFilter(selectedTags));

                if (_tagAdapter.AnySelected && !_tagAdapter.AllSelected)
                {
                    _kitchenLayoutSelected.Text = string.Join(",", selectedTags);
                }
                else if(_tagAdapter.AllSelected)
                {
                    _kitchenLayoutSelected.Text = GetText(Resource.String.MapsFilterAllOptions);
                }
                else
                {
                    _kitchenLayoutSelected.Text = string.Empty;
                }

                alertDialog.Dismiss();
                var settings = GetSharedPreferences(GetString(Resource.String.sharedPreferencesKey), FileCreationMode.Private).Edit();
                settings.PutStringSet(GetString(Resource.String.sharedPreferencesFilterTags), _tagAdapter.Tags.Where(x => x.IsChecked).Select(x => x.Id.ToString()).ToList());
                settings.Apply();
            };

            _tagsSearchEditText = alertDialog.FindViewById<EditText>(Resource.Id.MapsDialogTagSearchBarEditText);
            _tagsSearchEditText.AddTextChangedListener(this);

            var kitchenLayoutRecycleView = alertDialog.FindViewById<RecyclerView>(Resource.Id.MapsDialogTagRecyclerView);

            kitchenLayoutRecycleView.SetAdapter(_tagAdapter);
            kitchenLayoutRecycleView.SetLayoutManager(new LinearLayoutManager(this));
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetSupportActionBar(_toolbar);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.MapsMenu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.MapsMenuFilter:
                    _drawerLayout.OpenDrawer(GravityCompat.End);
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override void OnShowNoInternetMessageSuccess()
        {
            base.OnShowNoInternetMessageSuccess();
            Finish();
        }

        public override void OnBackPressed()
        {
            if (_drawerLayout.IsDrawerOpen(GravityCompat.End))
            {
                _drawerLayout.CloseDrawer(GravityCompat.End);
            }
            else
            {
                base.OnBackPressed();
            }
        }
        public void SetFilterInitialData()
        {
            var preferences = GetSharedPreferences(GetString(Resource.String.sharedPreferencesKey), FileCreationMode.Private);
            var preferencesDistance = preferences.GetInt(GetString(Resource.String.sharedPreferencesFilterDistance), -1).NoneWhen(x => x == -1);
            var preferencesTags = preferences.GetStringSet(GetString(Resource.String.sharedPreferencesFilterTags), new List<string>()).Select(t => new Guid(t)).ToList();

            _pageAdapter.AddFilter(new DistanceMapFilter(preferencesDistance, _myLocation));
            _pageAdapter.AddFilter(new TagMapFilter(_tagAdapter.Tags.Where(t => preferencesTags.Contains(t.Id)).Select(t => t.Name)));
            _tagAdapter.CheckTags(preferencesTags);

            if (_tagAdapter.AnySelected && !_tagAdapter.AllSelected)
            {
                _kitchenLayoutSelected.Text = string.Join(",", _tagAdapter.Selected.Select(t => t.Name));
            }
            else if (_tagAdapter.AllSelected)
            {
                _kitchenLayoutSelected.Text = GetText(Resource.String.MapsFilterAllOptions);
            }
            else
            {
                _kitchenLayoutSelected.Text = "-";
            }

            _distanceSeekbar.Touch += delegate (object sender, View.TouchEventArgs args)
            {
                if (sender is View v)
                {
                    switch (args.Event.Action)
                    {
                        case MotionEventActions.Down:
                            v.Parent.RequestDisallowInterceptTouchEvent(true);
                            break;
                        case MotionEventActions.Up:
                            v.Parent.RequestDisallowInterceptTouchEvent(false);
                            break;
                    }
                    v.OnTouchEvent(args.Event);
                }
            };
            _distanceSeekbar.StopTrackingTouch += delegate (object sender, SeekBar.StopTrackingTouchEventArgs args)
            {
                if (args.SeekBar is SeekBar seekBar)
                {
                    var settings = GetSharedPreferences(GetString(Resource.String.sharedPreferencesKey), FileCreationMode.Private).Edit();
                    settings.PutInt(GetString(Resource.String.sharedPreferencesFilterDistance), seekBar.Progress*100);
                    settings.Apply();

                    var distance = (seekBar.Progress * 100).SomeNotNull();
                    var displayText = distance.ValueOr(0) < 1000 ? $"{distance.ValueOr(0)} m" : $"{System.Math.Round(distance.ValueOr(0) / 1000d, 1)} km";
                    if (seekBar.Progress >= _distanceSeekbar.Max - 1)
                    {
                        displayText = GetText(Resource.String.MapsFilterDistanceAll);
                        distance = 0.None();
                    }
                    FindViewById<TextView>(Resource.Id.MapsFilterCurrentDistance).Text = displayText;
                    _pageAdapter.AddFilter(new DistanceMapFilter(distance, _myLocation));
                }
            };
            _distanceSeekbar.ProgressChanged += delegate (object sender, SeekBar.ProgressChangedEventArgs args)
            {
                var distance = args.Progress * 100;
                var displayText = distance < 1000 ? $"{distance} m" : $"{System.Math.Round(distance / 1000d, 1)} km";
                if (args.Progress >= _distanceSeekbar.Max - 1)
                {
                    displayText = GetText(Resource.String.MapsFilterDistanceAll);
                }
                FindViewById<TextView>(Resource.Id.MapsFilterCurrentDistance).Text = displayText;
            };
            _filterResetButton.Click += delegate (object sender, EventArgs args)
            {
                _distanceSeekbar.Progress = 50;
                _tagAdapter.ClearChecks();
                var settings = GetSharedPreferences(GetString(Resource.String.sharedPreferencesKey), FileCreationMode.Private).Edit();
                settings.Remove(GetString(Resource.String.sharedPreferencesFilterDistance));
                settings.Remove(GetString(Resource.String.sharedPreferencesFilterTags));
                settings.Apply();
                SetDefaultFilter();
            };

            //Here 10000 meters, because 10000 is the current maximum of the slider
            _distanceSeekbar.Progress = preferencesDistance.ValueOr(10000)/100;
        }

        public void SetDefaultFilter()
        {
            _pageAdapter.ClearFilters();
            _pageAdapter.AddFilter(new DistanceMapFilter(5000.Some(), Option.None<LatLng>()));
            _pageAdapter.AddFilter(new TagMapFilter(Enumerable.Empty<string>()));
        }

        public void AfterTextChanged(IEditable s)
        {
            var searchString = _tagsSearchEditText.Text;
            _tagAdapter.SetData(_tagAdapter.Tags.Where(x => x.Name.IndexOf(searchString, 0, StringComparison.CurrentCultureIgnoreCase) != -1).ToList());
        }

        public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
        }

        public void OnTextChanged(ICharSequence s, int start, int before, int count)
        {
        }

        public void OnListStoreClicked(StoreLocationDto store)
        {
            var tab = _tabLayout.GetTabAt(0);
            tab.Select();
            _mapFragment.NavigateToStore(store);
            _mapFragment.ShowStoreInfosInBottomSheet(store, true);
        }

        private class MapLocationCallback : LocationCallback
        {
            public event EventHandler<Location> LocationUpdated;

            public override void OnLocationResult(LocationResult result)
            {
                base.OnLocationResult(result);
                LocationUpdated?.Invoke(this, result.LastLocation);
            }
        }
    }
}