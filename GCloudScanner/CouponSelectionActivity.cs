using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using GCloud.Shared.Dto.Domain;
using GCloudScanner.RecycleView.CouponsList;
using GCloudScanner.Service;
using Refit;
using AlertDialog = Android.App.AlertDialog;
using Toolbar = Android.Widget.Toolbar;

namespace GCloudScanner
{
    [Activity(Label = "Gutscheine")]
    public class CouponSelectionActivity : AppCompatActivity
    {
        private ListView _listViewCouponsList;
        private RecyclerView _recyclerView;
        private RelativeLayout _mainRoot;

        private CouponsAdapter _couponsAdapter;
        private RecyclerView.LayoutManager _layoutManager;

        private ICouponService _couponService;

        private List<CouponDto> _coupons;
        private string _userId;
        private string _storeId;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.CouponsList);

            _storeId = Intent.GetStringExtra("storeGuid");
            _userId = Intent.GetStringExtra("userId");

            //_listViewCouponsList = FindViewById<ListView>(Resource.Id.listViewCouponsList);
            _recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerViewCouponsList);
            _layoutManager = new LinearLayoutManager(this);
            _recyclerView.SetLayoutManager(_layoutManager);
            _mainRoot = FindViewById<RelativeLayout>(Resource.Id.mainRootCouponsList);

            _couponService = RestService.For<ICouponService>(SettingsActivity.BaseUrl);

            LoadCoupons();

            //_listViewCouponsList.ItemClick += ListViewCouponsListOnItemClick;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            return true;
        }

        private void ListViewCouponsListOnItemClick(object sender, int position)
        {
            var coupon = _coupons[position];
            var intent = new Intent();
            intent.PutExtra("couponId", coupon.Id.ToString());
            intent.PutExtra("userId", _userId);
            SetResult(Result.Ok, intent);
            Finish();
        }

        private void LoadCoupons()
        {
            var progress = new ProgressDialog(this)
            {
                Indeterminate = true
            };
            progress.SetMessage("Wird geladen...");
            progress.Show();

            var pref = GetSharedPreferences(SettingsActivity.PrefKey, FileCreationMode.Private);
            var apiToken = pref.GetString(SettingsActivity.ApiTokenPrefKey, null);
            var cashRegisterId = Guid.Parse(pref.GetString(SettingsActivity.CashRegisterIdPrefKey, null));

            new Thread(new ThreadStart(delegate
            {
                RunOnUiThread(async () =>
                {
                    _coupons = await _couponService.GetCouponsByUser(apiToken,_userId, cashRegisterId);
                    _couponsAdapter = new CouponsAdapter(_coupons);
                    _couponsAdapter.ItemClick += ListViewCouponsListOnItemClick;
                    _recyclerView.SetAdapter(_couponsAdapter);
                    progress.Dismiss();
                });
            })).Start();
        }
    }
}