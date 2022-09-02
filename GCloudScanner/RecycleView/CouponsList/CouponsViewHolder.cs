using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using GCloudScanner;

namespace GCloudScanner.RecycleView.CouponsList
{
    public class CouponsViewHolder : RecyclerView.ViewHolder
    {
        public ImageView Image { get; set; }
        public TextView Name { get; set; }
        public TextView ValidFrom { get; set; }
        public TextView ValidTo { get; set; }
        public TextView Value { get; set; }
        public TextView RedeemsLeft { get; set; }

        public CouponsViewHolder(View itemView, Action<int> listener) : base(itemView)
        {
            Image = itemView.FindViewById<ImageView>(Resource.Id.imageViewCouponListRecycleViewItem);
            Name = itemView.FindViewById<TextView>(Resource.Id.txtCouponListRecycleViewItem);
            ValidFrom = itemView.FindViewById<TextView>(Resource.Id.txtCouponListRecycleViewItemValidFrom);
            ValidTo = itemView.FindViewById<TextView>(Resource.Id.txtCouponListRecycleViewItemValidTo);
            Value = itemView.FindViewById<TextView>(Resource.Id.txtCouponListRecycleViewItemValue);
            RedeemsLeft = itemView.FindViewById<TextView>(Resource.Id.txtCouponListRecycleViewItemRedeemsLeft);

            itemView.Click += (sender, args) => listener(base.LayoutPosition);
        }
    }
}