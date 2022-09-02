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

namespace mvdata.foodjet.RecycleView.CashbackList
{
    public class CashbackViewHolder : RecyclerView.ViewHolder
    {
        public TextView Change { get; set; }
        public TextView Date { get; set; }
        public TextView Time { get; set; }
        public ImageView CashbackIcon { get; set; }

        public CashbackViewHolder(View itemView, Action<int> listener) : base(itemView)
        {
            Change = itemView.FindViewById<TextView>(Resource.Id.txtCashbackRecycleViewItemChange);
            Date = itemView.FindViewById<TextView>(Resource.Id.txtCashbackRecycleViewItemDate);
            Time = itemView.FindViewById<TextView>(Resource.Id.txtCashbackRecycleViewItemTime);
            CashbackIcon = itemView.FindViewById<ImageView>(Resource.Id.cashbackImage);
            itemView.Click += (sender, args) => listener(LayoutPosition);
        }
    }
}