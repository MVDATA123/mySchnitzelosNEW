using System;
using System.Collections.Generic;
using System.Linq;
using Android.Support.V7.Widget;
using Android.Views;
using GCloud.Shared.Dto.Domain;
using mvdata.foodjet.Domain;
using Optional;
using Optional.Collections;

namespace mvdata.foodjet.RecycleView.StoreList.Manager
{
    public class ManagerStoreAdapter : RecyclerView.Adapter
    {
        public event EventHandler<StoreDto> ItemClicked;
        private readonly List<StoreDto> _stores;

        public ManagerStoreAdapter(IEnumerable<Option<StoreDto>> stores)
        {
            _stores = stores.Values().ToList();
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is ManagerStoreViewHolder managerStoreViewHolder)
            {
                var store = new StoreLocationDto(_stores[position]);

                managerStoreViewHolder.StoreName.Text = store.Name;
                managerStoreViewHolder.StoreAddress.Text = store.Address;
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ManagerStoreRecycleViewItem, parent, false);

            var vh = new ManagerStoreViewHolder(itemView, OnItemClicked);
            return vh;
        }

        public override int ItemCount => _stores.Count;

        protected virtual void OnItemClicked(int position)
        {
            ItemClicked?.Invoke(this, _stores[position]);
        }
    }
}