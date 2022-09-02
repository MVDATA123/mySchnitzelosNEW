using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using GCloud.Shared.Dto.Domain;
using mvdata.foodjet.Adapter.ViewPager;
using mvdata.foodjet.Caching;
using Fragment = Android.Support.V4.App.Fragment;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace mvdata.foodjet
{
    [Activity(Label = "ManagerCouponEditActivity", ParentActivity = typeof(ManagerCouponsList))]
    public class ManagerCouponEditActivity : AppCompatActivity
    {
        private Toolbar _toolbar;
        private TabLayout _tabLayout;
        private ViewPager _viewPager;
        private MyFragmentPagerAdapter _adapter;
        private Page1Fragment _page1Fragment;
        private Page2Fragment _page2Fragment;
        private Page3Fragment _page3Fragment;

        private CouponDto _coupon;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_manager_coupon_edit);

            var guid = Intent.GetStringExtra("couponGuid");

            _toolbar = FindViewById<Toolbar>(Resource.Id.app_bar);
            _tabLayout = FindViewById<TabLayout>(Resource.Id.tabLayoutManagerCouponsEdit);
            _viewPager = FindViewById<ViewPager>(Resource.Id.viewPagerManagerCouponsEdit);

            _tabLayout.SetupWithViewPager(_viewPager);

            if (Guid.TryParse(guid, out var couponGuid))
            {
                CachingHolder.Instance.GetCouponByGuid(couponGuid).Match(c => _coupon = c, () => Finish());
                SetupViewPager(_viewPager);
            }
            else
            {
                Finish();
            }
        }

        private void SetupViewPager(ViewPager viewPager)
        {
            _page1Fragment = new Page1Fragment();
            _page2Fragment = new Page2Fragment();
            _page3Fragment = new Page3Fragment();
            _adapter = new MyFragmentPagerAdapter(SupportFragmentManager);
            _adapter.AddFragment(_page1Fragment, "Details");
            _adapter.AddFragment(_page2Fragment, "Gültigkeiten");
            _adapter.AddFragment(_page3Fragment, "Filialen");
            viewPager.Adapter = _adapter;
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetSupportActionBar(_toolbar);
            SupportActionBar.SetTitle(Resource.String.managerCouponEditListActivityHeader);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
        }
    }

    public class Page1Fragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_manager_edit_coupon_details, container, false);
        }
    }

    public class Page2Fragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_manager_edit_coupon_details, container, false);
        }
    }

    public class Page3Fragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_manager_edit_coupon_details, container, false);
        }
    }
}