using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using GCloud.Shared.Dto.Domain;
using GCloudShared.Domain;
using GCloudShared.Service;
using GCloudShared.Shared;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using GCloud.Shared.Dto;

namespace mvdata.foodjet.RecycleView.BillItems
{
    public class BillItemsAdapter : RecyclerView.Adapter
    {
        private readonly List<InvoiceDetailsItemListListLineItem> _invoiceItems;
        private readonly Context _context;

        public BillItemsAdapter(Context context, List<InvoiceDetailsItemListListLineItem> invoiceItems) {
            _context = context;
            _invoiceItems = invoiceItems;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (!(holder is BillItemsViewHolder vh))
                return;

            var item = _invoiceItems[position];
            vh.ItemDesc.Text = item.Description.Trim();
            vh.TaxRate.Text = item.VATRate.ToString();
            vh.Amount.Text = item.Quantity.Value.ToString();
            vh.SinglePrice.Text = item.UnitPrice.ToString("0.##");
            vh.TotalPrice.Text = item.LineItemAmount.ToString("0.##");
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemview = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.BillDetailsBillItemRow, parent, false);

            return new BillItemsViewHolder(itemview);
        }

        public override int ItemCount => _invoiceItems.Count;
    }
}
