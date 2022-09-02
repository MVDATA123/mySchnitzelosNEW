using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using System;

namespace mvdata.foodjet.RecycleView.BillsList
{
    /*
      public string InvoiceNumber { get; set; }
        public string Company { get; set; }
        public decimal Amount { get; set; }
        public DateTime InvoiceDate { get; set; }
         */

    public class BillsViewHolder : RecyclerView.ViewHolder
    {
        public TextView Company { get; set; }
        public TextView Amount { get; set; }
        public TextView InvoiceDate { get; set; }
        public TextView InvoiceNumber { get; set; }
        public RelativeLayout MainRoot { get; set; }

        public BillsViewHolder(View itemView, Action<int> itemClicked) : base(itemView)
        {
            Company = itemView.FindViewById<TextView>(Resource.Id.txtBillCompany);
            Amount = itemView.FindViewById<TextView>(Resource.Id.txtBillAmount);
            InvoiceDate = itemView.FindViewById<TextView>(Resource.Id.txtBillInvoiceDate);
            InvoiceNumber = itemView.FindViewById<TextView>(Resource.Id.txtInvoiceNumber);
            MainRoot = itemView.FindViewById<RelativeLayout>(Resource.Id.bills_list_main_root);

            itemView.Click += (sender, args) => itemClicked(LayoutPosition);
        }
    }
}