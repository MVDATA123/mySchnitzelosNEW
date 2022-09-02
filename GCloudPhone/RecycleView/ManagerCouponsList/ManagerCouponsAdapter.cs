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
using GCloud.Shared.Dto.Domain;
using GCloudShared.Domain;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Refit;

namespace mvdata.foodjet.RecycleView.CouponsList
{
    public class ManagerCouponsAdapter : RecyclerView.Adapter
    {
        private readonly IUserCouponService _userCouponService;
        private readonly List<CouponDto> _coupons;
        private readonly UserLoginMethod _userLoginMethod;

        public event EventHandler<CouponDto> ItemClicked;
        public event EventHandler<CouponDto> EditItemClicked;
        public ManagerCouponsAdapter(Context context, List<CouponDto> coupons)
        {
            _coupons = coupons.Where(x => !x.ValidTo.HasValue || x.ValidTo.Value.Date >= DateTime.Now).OrderByDescending(c => c.IsValid).ToList();
            _userCouponService = RestService.For<IUserCouponService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
            _userLoginMethod = (UserLoginMethod)context.GetSharedPreferences(context.GetString(Resource.String.sharedPreferencesKey), FileCreationMode.Private).GetInt(context.GetString(Resource.String.sharedPreferencesLoginMethod), -1);
        }


        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is ManagerCouponsViewHolder couponsViewHolder)
            {
                var item = _coupons[position];

                couponsViewHolder.Name.Text = item.Name;
                if (!string.IsNullOrWhiteSpace(item.IconBase64))
                {
                    var bytes = Convert.FromBase64String(item.IconBase64);
                    using (var icon = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length))
                    {
                        couponsViewHolder.Image.SetImageBitmap(icon);
                    }
                }
                else
                {
                    couponsViewHolder.Image.SetImageResource(Resource.Drawable.No_image_available);
                }
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemview = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.recycleviewitem_manager_coupons_list, parent, false);

            var vh = new ManagerCouponsViewHolder(itemview, OnItemClicked, OnEditItemClicked);
            return vh;
        }

        public override int ItemCount => _coupons.Count;

        void OnItemClicked(int position)
        {
            ItemClicked?.Invoke(this, _coupons[position]);
        }

        protected virtual void OnEditItemClicked(int position)
        {
            EditItemClicked?.Invoke(this, _coupons[position]);
        }
    }
}