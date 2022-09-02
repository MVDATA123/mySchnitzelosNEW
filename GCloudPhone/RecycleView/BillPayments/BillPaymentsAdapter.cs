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

namespace mvdata.foodjet.RecycleView.BillPayments
{
    public class BillPaymentsAdapter : RecyclerView.Adapter
    {
        private readonly List<InvoicePaymentMethod> _payments;
        private readonly Context _context;

        public BillPaymentsAdapter(Context context, List<InvoicePaymentMethod> payments)
        {
            _context = context;
            _payments = payments;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (!(holder is BillPaymentsViewHolder vh))
                return;

            var item = _payments[position];
            vh.PaymentDesc.Text = item.Comment.Trim();
            vh.PaymentAmount.Text = item.Amount.ToString("0.##");
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemview = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.BillPaymentRow, parent, false);

            return new BillPaymentsViewHolder(itemview);
        }

        public override int ItemCount => _payments.Count;
    }
}
