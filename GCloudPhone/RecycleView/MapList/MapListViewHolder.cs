using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Support.Constraints;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace mvdata.foodjet.RecycleView.MapList
{
    public class MapListViewHolder : RecyclerView.ViewHolder
    {
        private readonly View _itemView;
        private readonly ConstraintLayout.LayoutParams _parameters;

        public ConstraintLayout MainRoot { get; set; }
        public TextView StoreName { get; set; }
        public TextView StoreAddress { get; set; }
        public TextView StoreDistance { get; set; }
        public ImageView StoreLogo { get; set; }

        public MapListViewHolder(View itemView, Action<int> clickedListener) : base(itemView)
        {
            MainRoot = itemView.FindViewById<ConstraintLayout>(Resource.Id.mainRoot);
            MainRoot.Click += delegate { clickedListener(LayoutPosition); };

            StoreName = itemView.FindViewById<TextView>(Resource.Id.txtStoreName);
            StoreAddress = itemView.FindViewById<TextView>(Resource.Id.txtStoreAddress);
            StoreDistance = itemView.FindViewById<TextView>(Resource.Id.txtStoreDistance);
            StoreLogo = itemView.FindViewById<ImageView>(Resource.Id.imgStoreLogo);

            _itemView = itemView;
        }

        public void Hide()
        {
            _itemView.Visibility = ViewStates.Gone;
            _itemView.LayoutParameters = new ConstraintLayout.LayoutParams(0,0);
        }

        public void Show()
        {
            MainRoot.Visibility = ViewStates.Visible;
            MainRoot.LayoutParameters = new ConstraintLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, (int)TypedValue.ApplyDimension(ComplexUnitType.Dip,100,Application.Context.Resources.DisplayMetrics));
        }
    }
}