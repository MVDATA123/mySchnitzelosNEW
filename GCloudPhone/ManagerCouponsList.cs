using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Constraints;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using GCloud.Shared.Dto.Domain;
using GCloudShared.Service;
using GCloudShared.Shared;
using Java.Security;
using mvdata.foodjet.RecycleView;
using mvdata.foodjet.RecycleView.CouponsList;
using Refit;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace mvdata.foodjet
{
    [Activity(Label = "ManagerCouponsList", ParentActivity = typeof(Dashboard))]
    public class ManagerCouponsList : AppCompatActivity
    {
        private ConstraintLayout _mainRoot;
        private Toolbar _toolbar;
        private ProgressBar _progressBar;

        private RecyclerView _recyclerView;
        private RecyclerView.LayoutManager _layoutManager;
        private ManagerCouponsAdapter _adapter;

        private List<CouponDto> _coupons;
        private IUserCouponService _userCouponService;


        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_manager_coupons_list);

            _mainRoot = FindViewById<ConstraintLayout>(Resource.Id.mainRootManagerCouponsList);
            _toolbar = FindViewById<Toolbar>(Resource.Id.app_bar);
            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBarManagerCoupons);

            _userCouponService = RestService.For<IUserCouponService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
            _coupons = await _userCouponService.GetManagerCoupons();

            _recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerViewManagerCouponsList);
            _layoutManager = new LinearLayoutManager(this);
            _adapter = new ManagerCouponsAdapter(this, _coupons);
            _recyclerView.SetAdapter(_adapter);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.AddItemDecoration(new DividerItemDecoration(_recyclerView.Context, (int)Orientation.Vertical));

            _adapter.EditItemClicked += (sender, coupon) =>
            {
                var intent = new Intent(this, typeof(ManagerCouponEditActivity));
                intent.PutExtra("couponGuid", coupon.Id.ToString());
                StartActivity(intent);
            };
            _adapter.ItemClicked += (sender, coupon) =>
            {
                var intent = new Intent(this, typeof(CouponDetailsActivity));
                intent.PutExtra("couponGuid", coupon.Id.ToString());
                StartActivity(intent);
            };

            _progressBar.Visibility = ViewStates.Gone;
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetSupportActionBar(_toolbar);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetTitle(Resource.String.managerCouponListActivityHeader);
        }
    }
}