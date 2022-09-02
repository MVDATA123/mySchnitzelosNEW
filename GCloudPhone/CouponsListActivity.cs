using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V4.Content.Res;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Util;
using Firebase.Messaging;
using GCloud.Shared.Dto.Domain;
using mvdata.foodjet.Adapter;
using GCloudShared.Domain;
using GCloudShared.Extensions;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Java.Lang;
using mvdata.foodjet.Caching;
using mvdata.foodjet.RecycleView;
using mvdata.foodjet.RecycleView.CouponsList;
using Optional;
using Optional.Collections;
using Refit;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Exception = Java.Lang.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using SearchView = Android.Support.V7.Widget.SearchView;
using Thread = System.Threading.Thread;

namespace mvdata.foodjet
{
    [Activity(Label = "Gutscheine", LaunchMode = LaunchMode.SingleTop, ScreenOrientation = ScreenOrientation.Portrait)]
    [MetaData("android.app.searchable", Resource = "@xml/coupon_searchable")]
    [IntentFilter(new[] {Intent.ActionSearch})]
    public class CouponsListActivity : BaseActivity, SearchView.IOnQueryTextListener,
        AppBarLayout.IOnOffsetChangedListener
    {
        private ListView _listViewCouponsList;
        private RecyclerView _recyclerView;
        private Toolbar _toolbar;
        private CoordinatorLayout _mainRoot;
        private SwipeRefreshLayout _swipeRefreshLayout;
        private ImageView _storeBanner, _imageViewIconSmall;
        private Button _btnCouponsListFollowStore;
        private Button _btnCouponsListEnableNotifications;
        private AppBarLayout _appBarLayout;
        private CollapsingToolbarLayout _collapsingToolbarLayout;
        private TextView _txtCouponsListHeader;
        private CouponsAdapter _couponsAdapter;
        private RecyclerView.LayoutManager _layoutManager;
        private FloatingActionButton _fab;

        private IUserCouponService _userCouponService;
        private IUserStoreService _userStoreService;
        private IStoreService _storeService;
        private SubscriptionRepository _subscriptionRepository;

        private List<CouponDto> _coupons = new List<CouponDto>();
        private string _storeGuidFilter;
        private string _storeName;
        private bool _storeCashbackEnabled;
        private bool _isUserAlreadyFollowing;
        private bool _toolbarIsShow = true;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.CouponsList);

            _mainRoot = FindViewById<CoordinatorLayout>(Resource.Id.mainRootCouponsList);
            _toolbar = FindViewById<Toolbar>(Resource.Id.app_bar_couponsList);

            _storeGuidFilter = Intent.GetStringExtra("storeGuid");
            _storeName = Intent.GetStringExtra("storeName");
            _storeCashbackEnabled = Intent.GetBooleanExtra("storeCashbackEnabled", false);
            _isUserAlreadyFollowing = Intent.GetBooleanExtra("isUserAlreadyFollowing", false);

            _appBarLayout = FindViewById<AppBarLayout>(Resource.Id.appBarLayoutCouponsList);
            _collapsingToolbarLayout = FindViewById<CollapsingToolbarLayout>(Resource.Id.toolbar_collapse);
            _appBarLayout.AddOnOffsetChangedListener(this);

            _swipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeLayoutCouponsList);
            _storeBanner = FindViewById<ImageView>(Resource.Id.storeBanner);
            _btnCouponsListFollowStore = FindViewById<Button>(Resource.Id.btnCouponsListFollowStore);
            _btnCouponsListEnableNotifications = FindViewById<Button>(Resource.Id.btnCouponsListEnableNotifications);
            _txtCouponsListHeader = FindViewById<TextView>(Resource.Id.txtCouponsListHeader);

            _userCouponService = RestService.For<IUserCouponService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
            _storeService = RestService.For<IStoreService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
            _userStoreService = RestService.For<IUserStoreService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
            _subscriptionRepository = new SubscriptionRepository(DbBootstraper.Connection);
            _swipeRefreshLayout.Refresh += RefreshCoupons;

            _imageViewIconSmall = FindViewById<ImageView>(Resource.Id.imageViewIconSmall);
            _fab = FindViewById<FloatingActionButton>(Resource.Id.floating_action_button);

            _recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerViewCouponsList);
            _layoutManager = new LinearLayoutManager(this);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.AddItemDecoration(new DividerItemDecoration(_recyclerView.Context, (int)Orientation.Vertical));
            var sizeProvider = new FixedPreloadSizeProvider(48, 48);
            if (Guid.TryParse(_storeGuidFilter, out var storeGuid))
            {
                //add the cached Coupons
                _coupons.AddRange(CachingHolder.Instance.GetCouponsByStore(storeGuid).Values());

                //Set the bannerimage
                CachingHolder.Instance.GetStoreBannerAsBase64(storeGuid).Match(imageBase64 =>
                {
                    var bytes = Convert.FromBase64String(imageBase64);
                    Glide.With(this).AsBitmap().Load(bytes).Into(_storeBanner);
                }, () =>
                {
                    Glide.With(this).AsDrawable().Load(Resource.Drawable.nav_header_background).Into(_storeBanner);
                });
                CachingHolder.Instance.GetCompanyLogoAsBase64(storeGuid).Match(imageBase64 =>
                {
                    var bytes = Convert.FromBase64String(imageBase64);
                    Glide.With(this).AsBitmap().Load(bytes).Into(_imageViewIconSmall);
                    Glide.With(this).AsBitmap().Load(bytes).Into(_fab);
                }, () =>
                {
                    _fab.Visibility = ViewStates.Gone;
                });
            }

            LoadItemsInAdapter(_coupons);

            InitializeButtons();
        }

        private async void RefreshCoupons(object sender, EventArgs e)
        {
            await LoadCoupons();
            if (Guid.TryParse(_storeGuidFilter, out var storeGuid))
            {
                CachingHolder.Instance.SetCouponsOfStore(storeGuid, _coupons);
            }
        }


        public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset)
        {
            int scrollRange = -1;
            if (scrollRange == -1)
            {
                scrollRange = appBarLayout.TotalScrollRange;
            }

            if (scrollRange + verticalOffset == 0)
            {
                _collapsingToolbarLayout.Title = $"{_storeName}";
                if (_fab.Visibility != ViewStates.Gone)
                {
                    _imageViewIconSmall.Visibility = ViewStates.Visible;
                }
                _toolbarIsShow = true;
                _txtCouponsListHeader.Visibility = ViewStates.Invisible;
            }
            else if (_toolbarIsShow)
            {
                _collapsingToolbarLayout.Title =
                    " "; //carefull there should a space between double quote otherwise it wont work 
                _toolbarIsShow = false;
                _txtCouponsListHeader.Visibility = ViewStates.Visible;
                if (_fab.Visibility != ViewStates.Gone)
                {
                    _imageViewIconSmall.Visibility = ViewStates.Invisible;
                }
            }
        }

        private void CheckButtonStates()
        {
            if (_storeGuidFilter != null)
            {
                if (_isUserAlreadyFollowing)
                {
                    _btnCouponsListFollowStore.SetBackgroundResource(Resource.Drawable.follow_button_style_filled);
                    _btnCouponsListFollowStore.SetTextColor(Color.White);

                    if (_subscriptionRepository.HasSubscription(_storeGuidFilter))
                    {
                        _btnCouponsListEnableNotifications.SetBackgroundResource(Resource.Drawable.icons8_bell_filled);
                    }
                    else
                    {
                        _btnCouponsListEnableNotifications.SetBackgroundResource(Resource.Drawable.icons8_bell);
                    }
                }
                else
                {
                    _btnCouponsListFollowStore.SetBackgroundResource(Resource.Drawable.follow_button_style);
                    _btnCouponsListFollowStore.SetTextColor(
                        new Color(ResourcesCompat.GetColor(Resources, Resource.Color.primaryColor, null)));
                    _btnCouponsListEnableNotifications.SetBackgroundResource(Resource.Drawable.icons8_bell);
                }
            }
        }

        private void InitializeButtons()
        {
            CheckButtonStates();
            _btnCouponsListFollowStore.Click += (sender0, args0) =>
            {
                if (!_isUserAlreadyFollowing)
                {
                    var builder = new AlertDialog.Builder(this, Resource.Style.DesignThemeAlertDialog);
                    builder.SetMessage(GetString(Resource.String.storeFollow))
                        .SetPositiveButton(GetString(Resource.String.storeFollowConfirm), async (sender, args) =>
                        {
                            try
                            {
                                try
                                {
                                    if (Guid.TryParse(_storeGuidFilter, out var storeGuid))
                                    {
                                        _userStoreService.AddToWatchList(_storeGuidFilter).GetAwaiter().GetResult();
                                        FirebaseMessaging.Instance.SubscribeToTopic(_storeGuidFilter);
                                        _subscriptionRepository.Insert(new Subscription()
                                        {
                                            StoreId = Guid.Parse(_storeGuidFilter)
                                        });
                                        _isUserAlreadyFollowing = true;
                                        CachingHolder.Instance.SetStoreFollowStatus(storeGuid, true);
                                    }
                                    InvalidateOptionsMenu();
                                    CheckButtonStates();
                                    Snackbar.Make(_mainRoot, GetString(Resource.String.generalSuccessMessage),
                                        Snackbar.LengthLong).Show();
                                }
                                catch (ApiException ex)
                                {
                                    ex.ShowApiErrorResultSnackbar(_mainRoot);
                                }
                            }
                            catch (ApiException apiException)
                            {
                                apiException.ShowApiErrorResultSnackbar(_mainRoot);
                            }
                        })
                        .SetNegativeButton(GetString(Resource.String.storeFollowDecline), (sender, args) => { }).Show();
                }
                else
                {
                    var builder = new AlertDialog.Builder(this, Resource.Style.DesignThemeAlertDialog);
                    builder.SetMessage(GetString(Resource.String.storeUnfollow))
                        .SetPositiveButton(GetString(Resource.String.storeUnfollowConfirm), async (sender, args) =>
                        {
                            try
                            {
                                if (Guid.TryParse(_storeGuidFilter, out var storeGuid))
                                {
                                    await _userStoreService.DeleteFromWatchlist(_storeGuidFilter);
                                    FirebaseMessaging.Instance.UnsubscribeFromTopic(_storeGuidFilter);
                                    _subscriptionRepository.DeleteById(_storeGuidFilter);
                                    _isUserAlreadyFollowing = false;
                                    CachingHolder.Instance.SetStoreFollowStatus(storeGuid, false);
                                }
                                CheckButtonStates();
                            }
                            catch (ApiException apiException)
                            {
                                apiException.ShowApiErrorResultSnackbar(_mainRoot);
                            }
                        })
                        .SetNegativeButton(GetString(Resource.String.storeUnfollowDecline), (sender, args) => { })
                        .Show();
                }
            };
            _btnCouponsListEnableNotifications.Click += (sender, args) =>
            {
                if (_isUserAlreadyFollowing)
                {
                    if (_subscriptionRepository.HasSubscription(_storeGuidFilter))
                    {
                        FirebaseMessaging.Instance.UnsubscribeFromTopic(_storeGuidFilter);
                        _subscriptionRepository.DeleteById(_storeGuidFilter);
                        Snackbar.Make(_mainRoot, Resource.String.couponsListUnsubscribeSuccessful, Snackbar.LengthLong)
                            .Show();
                    }
                    else
                    {
                        FirebaseMessaging.Instance.SubscribeToTopic(_storeGuidFilter);
                        _subscriptionRepository.Insert(new Subscription() {StoreId = Guid.Parse(_storeGuidFilter)});
                        Snackbar.Make(_mainRoot, Resource.String.couponsListSubscribeSuccessful, Snackbar.LengthLong)
                            .Show();
                    }

                    CheckButtonStates();
                }
                else
                {
                    Snackbar.Make(_mainRoot, GetString(Resource.String.enableNotificationOnlyPossibleWhenFollowing),
                        Snackbar.LengthLong).Show();
                }
            };
        }

        public override void OnShowNoInternetMessageSuccess()
        {
            base.OnShowNoInternetMessageSuccess();
            Finish();
        }

        protected override void OnResume()
        {
            SetSupportActionBar(_toolbar);
            if (_storeName != null)
            {
                _txtCouponsListHeader.Text = $"{_storeName}";
                SupportActionBar.Title = $"{_storeName}";
            }
            else
            {
                SupportActionBar.SetTitle(Resource.String.couponsActivityHeader);
            }

            base.OnResume();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.CouponsListMenu, menu);
            menu.FindItem(Resource.Id.CouponsListMenuShowCashback)
                .SetVisible(_storeCashbackEnabled && _storeGuidFilter != null);

            // Associate searchable configuration with the SearchView
            var searchManager =
                (SearchManager) GetSystemService(Context.SearchService);
            var searchView =
                (SearchView) menu.FindItem(Resource.Id.CouponsListMenuSearch).ActionView;
            searchView.SetSearchableInfo(searchManager.GetSearchableInfo(ComponentName));
            searchView.SetOnQueryTextListener(this);
            menu.FindItem(Resource.Id.CouponsListMenuSearch).SetVisible(_storeGuidFilter == null);


            return true;
        }

        private void ListViewCouponsListOnItemClick(object sender, CouponDto coupon)
        {
            if (!coupon.IsValid)
            {
                Snackbar.Make(_mainRoot, GetString(Resource.String.couponNotValid), Snackbar.LengthLong).Show();
                return;
            }

            var intent = new Intent(this, typeof(CouponDetailsActivity));
            intent.PutExtra("couponGuid", coupon.Id.ToString());
            StartActivity(intent);
        }

        private async Task LoadCoupons()
        {
            await Task.Run(async () =>
            {
                RunOnUiThread(() => _swipeRefreshLayout.Refreshing = true);
                List<CouponDto> newCoupons = new List<CouponDto>();
                try
                {
                    if (string.IsNullOrEmpty(_storeGuidFilter))
                    {
                        newCoupons = await _userCouponService.GetUserCoupons(true);
                    }
                    else
                    {
                        newCoupons = await _userCouponService.GetUserCouponsByStore(_storeGuidFilter, true);
                    }
                }
                catch (ApiException e)
                {
                    if (e.StatusCode == 0)
                    {
                        return;
                    }
                }

                LoadItemsInAdapter(newCoupons);
                RunOnUiThread(() => _swipeRefreshLayout.Refreshing = false);

            });
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.CouponsListMenuShowCashback)
            {
                var intent = new Intent(this, typeof(CashbackActivity));
                intent.PutExtra("storeGuid", _storeGuidFilter);
                StartActivity(intent);
            }

            foreach (var store in CachingHolder.Instance.Stores.Values()
                .Where(s => s.Id == Guid.Parse(_storeGuidFilter)))
            {
                store.IsUserFollowing = false;
            }

            foreach (var store in CachingHolder.Instance.Stores.Values()
                .Where(s => s.Id == Guid.Parse(_storeGuidFilter)))
            {
                store.IsUserFollowing = true;
            }

            return true;
        }

        private void LoadItemsInAdapter(List<CouponDto> coupons)
        {
            var newCoupons = new List<CouponDto>(coupons);
            _coupons.Clear();
            _coupons.AddRange(newCoupons);
            _couponsAdapter = new CouponsAdapter(this, _coupons);
            _couponsAdapter.ItemClick += ListViewCouponsListOnItemClick;
            RunOnUiThread(() =>
            {
                _recyclerView.SetAdapter(_couponsAdapter);
            });
        }

        private void SearchCoupon(string query)
        {
            new Thread(new ThreadStart(delegate
            {
                RunOnUiThread(async () =>
                {
                    if (_coupons == null)
                    {
                        _coupons = await _userCouponService.GetUserCoupons(_isUserAlreadyFollowing);
                    }

                    var searchResult = _coupons.Where(c => c.Name.ToUpper().Contains(query.ToUpper())).ToList();
                    LoadItemsInAdapter(searchResult);
                });
            })).Start();
        }

        public bool OnQueryTextChange(string newText)
        {
            SearchCoupon(newText);
            return true;
        }

        public bool OnQueryTextSubmit(string query)
        {
            SearchCoupon(query);
            return true;
        }
    }
}