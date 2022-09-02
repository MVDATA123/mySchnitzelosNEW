using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using GCloud.Shared.Dto.Domain;
using GCloudShared.Service;
using GCloudShared.Shared;
using mvdata.foodjet.Adapter;
using mvdata.foodjet.RecycleView;
using Refit;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace mvdata.foodjet
{
    [Activity(Label = "CashbackActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class CashbackActivity : BaseActivity
    {
        private Toolbar _toolbar;
        private ProgressBar _progressBar;
        private RecyclerView _recyclerView;
        private List<CashbackDto> _cashbacks;
        private ICashbackService _cashbackService;
        private string _selectedStoreGuid;
        private SwipeRefreshLayout _swipeRefreshLayout;

        private CashbackAdapter _cashbackAdapter;
        private RecyclerView.LayoutManager _layoutManager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.CashbackList);

            _selectedStoreGuid = Intent.GetStringExtra("storeGuid");

            _toolbar = FindViewById<Toolbar>(Resource.Id.app_bar_cashback);

            _recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerViewCashbackList);
            _layoutManager = new LinearLayoutManager(this);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.AddItemDecoration(new DividerItemDecoration(_recyclerView.Context, (int)Orientation.Vertical));
            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBarCashbackList);
            _swipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeLayoutCashbackList);

            _swipeRefreshLayout.Refresh += RefreshCashbacks;
            _cashbackService = RestService.For<ICashbackService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);

            LoadCashbacks();
            // Create your application here
        }

        private void RefreshCashbacks(object sender, EventArgs e)
        {
            LoadCashbacks();
            _swipeRefreshLayout.Refreshing = false;
        }

        private void LoadCashbacks()
        {
            _progressBar.Visibility = ViewStates.Visible;

            new Thread(new ThreadStart(async delegate
            {
                if (!string.IsNullOrWhiteSpace(_selectedStoreGuid))
                {
                    _cashbacks = (await _cashbackService.GetCashbacksForStore(_selectedStoreGuid)).OrderByDescending(x => x.CreditDateTime).ToList();
                    RunOnUiThread(() =>
                    {
                        _cashbackAdapter = new CashbackAdapter(_cashbacks);
                        _recyclerView.SetAdapter(_cashbackAdapter);
                        var textViewAppbarBalance = FindViewById<TextView>(Resource.Id.txtAppBarCashbackBalance);
                        textViewAppbarBalance.Text = (_cashbacks.FirstOrDefault()?.CreditNew ?? 0).ToString("C", CultureInfo.CreateSpecificCulture("de-DE"));
                        _progressBar.Visibility = ViewStates.Gone;
                    });
                }
            })).Start();
        }

        protected override void OnResume()
        {
            SetSupportActionBar(_toolbar);
            var textViewAppbarTitle = FindViewById<TextView>(Resource.Id.txtAppBarCashbackTitle);
            textViewAppbarTitle.Text = GetString(Resource.String.CashbackListHeader);
            SupportActionBar.SetDisplayUseLogoEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            base.OnResume();
        }
    }
}