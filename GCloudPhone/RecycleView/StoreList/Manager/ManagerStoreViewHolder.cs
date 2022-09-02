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
using GCloud.Shared.Dto.Domain;

namespace mvdata.foodjet.RecycleView.StoreList.Manager
{
    public class ManagerStoreViewHolder : RecyclerView.ViewHolder
    {
        public TextView StoreName { get; set; }
        public TextView StoreAddress { get; set; }
        public ImageButton StoreEdit { get; set; }

        public ManagerStoreViewHolder(View itemView, Action<int> btnEditClickedAction) : base(itemView)
        {
            StoreName = itemView.FindViewById<TextView>(Resource.Id.txtStoreNameManagerStoreNameRecyclerViewItem);
            StoreAddress = itemView.FindViewById<TextView>(Resource.Id.txtStoreNameManagerStoreAddressRecyclerViewItem);
            StoreEdit = itemView.FindViewById<ImageButton>(Resource.Id.btnManagerStoreEditRecyclerViewItem);
            StoreEdit.Click += delegate { btnEditClickedAction(LayoutPosition); };
        }
    }
}