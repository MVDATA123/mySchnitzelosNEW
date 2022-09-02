using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using GCloud.Shared.Dto.Domain;
using GCloudScanner;
using GCloudScanner.Service;
using Refit;

namespace GCloudScanner.RecycleView.CouponsList
{
    public class CouponsAdapter : RecyclerView.Adapter
    {
        private readonly ICouponService _couponService;
        private readonly List<CouponDto> _coupons;

        public event EventHandler<int> ItemClick; 
        public CouponsAdapter(List<CouponDto> coupons)
        {
            _coupons = coupons;
            _couponService = RestService.For<ICouponService>(SettingsActivity.BaseUrl);
        }


        public override async void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as CouponsViewHolder;
            var item = _coupons[position];

            if(vh == null) return;

            var now = DateTime.Now;
            var daydifferenceToFrom = item.ValidFrom.HasValue && item.ValidFrom.Value.Date >= now.Date
                ? (item.ValidFrom.Value.Date - now.Date).Days
                : -1;
            var daydifferenceToTo = item.ValidTo.HasValue && item.ValidTo.Value.Date >= now.Date
                ? (item.ValidTo.Value.Date - now.Date).Days
                : -1;

            vh.Name.Text = item.Name;
            if (daydifferenceToFrom > 1)
            {
                vh.ValidFrom.Text = $@"Gültig in {daydifferenceToFrom} Tagen";
                vh.ValidFrom.Visibility = ViewStates.Visible;
            }
            else if (daydifferenceToFrom == 1)
            {
                vh.ValidFrom.Text = $@"Gültig in {daydifferenceToFrom} Tag";
                vh.ValidFrom.Visibility = ViewStates.Visible;
            }
            else if (daydifferenceToFrom == 0)
            {
                vh.ValidFrom.Text = $@"Ab heute Gültig!";
                vh.ValidFrom.Visibility = ViewStates.Visible;
            }
            else
            {
                vh.ValidFrom.Visibility = ViewStates.Invisible;
            }

            if(daydifferenceToTo > 1 && daydifferenceToFrom <= 0)
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
                vh.ValidTo.Visibility = ViewStates.Invisible;
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

            vh.RedeemsLeft.Text = item.RedeemsLeft?.ToString() ?? "Unbegrenzt";

            vh.Value.Text = $"{value.ToString(sign, CultureInfo.CurrentCulture)}";

            using (var stream = await _couponService.GetCouponImage(item.Id))
            {
                var image = BitmapFactory.DecodeStream(stream);
                if (image != null)
                {
                    vh.Image.SetImageBitmap(image);
                }
                else
                {
                    vh.Image.SetImageResource(Resource.Drawable.No_image_available);
                }
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
            ItemClick?.Invoke(this, position);
        }
    }
}