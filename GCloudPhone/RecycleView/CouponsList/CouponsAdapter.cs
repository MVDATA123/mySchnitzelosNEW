using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Icu.Text;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using GCloud.Shared.Dto.Domain;
using GCloudShared.Domain;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Refit;

namespace mvdata.foodjet.RecycleView.CouponsList
{
    public class CouponsAdapter : RecyclerView.Adapter
    {
        private readonly IUserCouponService _userCouponService;
        private readonly List<CouponDto> _coupons;
        private readonly UserLoginMethod _userLoginMethod;
        private readonly Context _context;

        public event EventHandler<CouponDto> ItemClick;
        public CouponsAdapter(Context context, List<CouponDto> coupons)
        {
            _context = context;
            _coupons = coupons.Where(x => !x.ValidTo.HasValue || x.ValidTo.Value.Date >= DateTime.Now).OrderByDescending(c => c.IsValid).ToList();
            _userCouponService = RestService.For<IUserCouponService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
            _userLoginMethod = (UserLoginMethod)context.GetSharedPreferences(context.GetString(Resource.String.sharedPreferencesKey), FileCreationMode.Private).GetInt(context.GetString(Resource.String.sharedPreferencesLoginMethod), -1);
        }


        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as CouponsViewHolder;
            var item = _coupons[position];

            if (vh == null) return;

            var now = DateTime.Now;
            var daydifferenceToFrom = item.ValidFrom.HasValue && item.ValidFrom.Value.Date >= now.Date
                ? (item.ValidFrom.Value.Date - now.Date).Days
                : -1;
            var daydifferenceToTo = item.ValidTo.HasValue && item.ValidTo.Value.Date >= now.Date
                ? (item.ValidTo.Value.Date - now.Date).Days
                : -1;

            vh.Name.Text = item.Name;

            if (daydifferenceToTo > 1 && daydifferenceToFrom <= 0)
            {
                vh.ValidTo.Text = $@"Noch {daydifferenceToTo} Tage gültig!";
                vh.ValidTo.Visibility = ViewStates.Visible;
            }
            else if (daydifferenceToTo == 1)
            {
                vh.ValidTo.Text = $@"Noch {daydifferenceToTo} Tag gültig!";
                vh.ValidTo.Visibility = ViewStates.Visible;
            }
            else if (daydifferenceToTo == 0)
            {
                vh.ValidTo.Text = $@"Nur noch heute Gültig!";
                vh.ValidTo.Visibility = ViewStates.Visible;
            }
            else
            {
                vh.ValidTo.Text = @"Bis auf Widerruf.";
            }

            string sign = "";
            var value = 0m;
            switch (item.CouponType)
            {
                case CouponTypeDto.Value:
                    sign = "C";
                    value = item.Value;
                    break;
                case CouponTypeDto.Percent:
                    sign = "P";
                    value = item.Value / 100;
                    break;
            }

            vh.RedeemsLeft.Text = item.RedeemsLeft?.ToString() ?? vh.RedeemsLeft.Context.GetString(Resource.String.infinity);

            vh.Value.Text = $"{value.ToString(sign, CultureInfo.CurrentCulture)}";

            if (!string.IsNullOrWhiteSpace(item.IconBase64))
            {
                var bytes = Convert.FromBase64String(item.IconBase64);
                Glide.With(_context).AsBitmap().Load(bytes).Into(vh.Image);
            }
            else
            {
                Glide.With(_context).AsDrawable().Load(Resource.Drawable.No_image_available);
            }

            if (!item.IsValid && _userLoginMethod == UserLoginMethod.Normal)
            {
                vh.MainRoot.SetBackgroundColor(Color.LightSeaGreen);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemview = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.CouponListRecycleViewItem, parent, false);

            var vh = new CouponsViewHolder(itemview, OnClick);
            return vh;
        }

        public override int ItemCount => _coupons.Count;

        void OnClick(int position)
        {
            ItemClick?.Invoke(this, _coupons[position]);
        }
    }
}