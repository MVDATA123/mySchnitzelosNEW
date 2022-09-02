using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using GCloud.Shared.Dto.Domain;
using GCloudShared.Extensions;
using GCloudShared.Service;
using GCloudShared.Shared;
using mvdata.foodjet.RecycleView.StoreList.Manager;
using Newtonsoft.Json;
using Optional;
using Refit;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace mvdata.foodjet
{
    [Activity(Label = "ManagerCouponsList", ParentActivity = typeof(Dashboard), ScreenOrientation = ScreenOrientation.Portrait)]
    public class ManagerStoresList : BaseActivity
    {
        private RecyclerView.LayoutManager _layoutManager;
        private List<Option<StoreDto>> _stores;
        private RecyclerView _recyclerView;
        private ManagerStoreAdapter _adapter;
        private RelativeLayout _mainRoot;
        private SwipeRefreshLayout _swipeRefreshLayout;
        private ProgressBar _progressBar;
        private FloatingActionButton _fab;
        private Toolbar _toolbar;
        private ImageButton _btnEditStore;

        private IUserStoreService _userStoreService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _userStoreService = RestService.For<IUserStoreService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);

            SetContentView(Resource.Layout.StoresList);
            _toolbar = FindViewById<Toolbar>(Resource.Id.app_bar);
            _mainRoot = FindViewById<RelativeLayout>(Resource.Id.mainRootStoresList);
            _recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerViewStoresList);
            _swipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeLayoutStoresList);
            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBarStoresList);
            _fab = FindViewById<FloatingActionButton>(Resource.Id.floating_action_button);

            _swipeRefreshLayout.Refresh += delegate
            {
                LoadStoresList();
            };

            _layoutManager = new LinearLayoutManager(this);
            _recyclerView.SetLayoutManager(_layoutManager);
            _fab.Visibility = ViewStates.Gone;
            _progressBar.Visibility = ViewStates.Gone;

            LoadStoresList();
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetSupportActionBar(_toolbar);
            SupportActionBar.SetTitle(Resource.String.filialeActivityHeader);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
        }

        private void LoadStoresList()
        {
            RunOnUiThread(() => _swipeRefreshLayout.Refreshing = true);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    _userStoreService.GetManagerStores().ContinueWith(
                        stores =>
                        {
                            _stores = stores.Result.Select(s => s.SomeNotNull()).ToList();
                            _adapter = new ManagerStoreAdapter(_stores);
                            _adapter.ItemClicked += delegate(object sender, StoreDto store)
                            {
                                var intent = new Intent(this, typeof(ManagerEditStoreActivity));
                                intent.PutExtra("store", JsonConvert.SerializeObject(store));
                                StartActivity(intent);
                            };
                            RunOnUiThread(() =>
                            {
                                _recyclerView.SetAdapter(_adapter);
                                _swipeRefreshLayout.Refreshing = false;
                                _progressBar.Visibility = ViewStates.Gone;
                            });
                        }, TaskContinuationOptions.OnlyOnRanToCompletion
                    );
                }
                catch (ApiException apiException)
                {
                    apiException.ShowApiErrorResultSnackbar(_mainRoot);
                }
            });
        }
    }
}