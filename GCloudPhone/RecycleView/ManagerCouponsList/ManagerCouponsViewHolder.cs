using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Constraints;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace mvdata.foodjet.RecycleView.CouponsList
{
    public class ManagerCouponsViewHolder : RecyclerView.ViewHolder
    {
        public ImageView Image { get; set; }
        public TextView Name { get; set; }
        public ImageButton EditButton { get; set; }
        public ConstraintLayout MainRoot { get; set; }

        public ManagerCouponsViewHolder(View itemView, Action<int> itemClicked, Action<int> editItemClicked) : base(itemView)
        {
            Image = itemView.FindViewById<ImageView>(Resource.Id.imgManagerCouponsListItemIcon);
            Name = itemView.FindViewById<TextView>(Resource.Id.txtManagerCouponsListItemName);
            MainRoot = itemView.FindViewById<ConstraintLayout>(Resource.Id.mainRootManagerCouponsListItem);
            EditButton = itemView.FindViewById<ImageButton>(Resource.Id.btnManagerCouponsListItemEdit);

            itemView.Click += (sender, args) => itemClicked(LayoutPosition);
            EditButton.Click += (sender, args) => editItemClicked(LayoutPosition);
        }
    }
}