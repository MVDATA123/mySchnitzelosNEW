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

namespace mvdata.foodjet.RecycleView.BillTaxes
{
    public class BillTaxesAdapter : RecyclerView.Adapter
    {
        private readonly List<InvoiceTaxVATItem> _taxes;
        private readonly Context _context;

        public BillTaxesAdapter(Context context, List<InvoiceTaxVATItem> taxes)
        {
            _context = context;
            _taxes = taxes;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (!(holder is BillTaxesViewHolder vh))
                return;

            var item = _taxes[position];
            vh.TaxDesc.Text = item.VATRate.ToString();
            vh.TaxNet.Text = item.VATRate.ToString();
            vh.Tax.Text = item.Amount.ToString("0.##");
            vh.TaxGross.Text = item.VATRate.ToString("0.##");
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemview = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.BillTaxRow, parent, false);

            return new BillTaxesViewHolder(itemview);
        }

        public override int ItemCount => _taxes.Count;
    }
}
