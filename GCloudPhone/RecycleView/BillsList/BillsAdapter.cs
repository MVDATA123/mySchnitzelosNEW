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

namespace mvdata.foodjet.RecycleView.BillsList
{
    public class BillsAdapter : RecyclerView.Adapter
    {
        public event EventHandler<Bill_Out_Dto> ItemClick;

        private readonly IBillService _billService;
        private readonly List<Bill_Out_Dto> _bills;
        private readonly UserLoginMethod _userLoginMethod;
        private readonly Context _context;

        public BillsAdapter(Context context, List<Bill_Out_Dto> bills)
        {
            _context = context;
            _bills = bills.OrderByDescending(c => c.Invoice.InvoiceDate).ToList();
            _billService = RestService.For<IBillService>(HttpClientContainer.Instance.HttpClient);
            _userLoginMethod = (UserLoginMethod)context.GetSharedPreferences(context.GetString(Resource.String.sharedPreferencesKey), FileCreationMode.Private).GetInt(context.GetString(Resource.String.sharedPreferencesLoginMethod), -1);
        }
        
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (!(holder is BillsViewHolder vh))
                return;

            var item = _bills[position];
            vh.Company.Text = item.Invoice.Biller.ComanyName;
            vh.Amount.Text = item.Invoice.TotalGrossAmount.ToString("0.##");
            vh.InvoiceDate.Text = item.Invoice.InvoiceDate.ToString();
            vh.InvoiceNumber.Text = item.Invoice.InvoiceNumber;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemview = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.BillListRecycleViewItem, parent, false);

            return new BillsViewHolder(itemview, OnClick);
        }

        public override int ItemCount => _bills.Count;

        private void OnClick(int position)
        {
            ItemClick?.Invoke(this, _bills[position]);
        }
    }
}