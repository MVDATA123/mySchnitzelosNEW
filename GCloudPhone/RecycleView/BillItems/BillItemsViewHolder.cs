using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using System;

namespace mvdata.foodjet.RecycleView.BillItems
{
    public class BillItemsViewHolder : RecyclerView.ViewHolder
    {
        public TextView ItemDesc { get; set; }
        public TextView TaxRate { get; set; }
        public TextView Amount { get; set; }
        public TextView SinglePrice { get; set; }
        public TextView TotalPrice { get; set; }

        public BillItemsViewHolder(View itemView) : base(itemView)
        {
            ItemDesc = itemView.FindViewById<TextView>(Resource.Id.billItemRowDesc);
            TaxRate = itemView.FindViewById<TextView>(Resource.Id.billItemRowTax);
            Amount = itemView.FindViewById<TextView>(Resource.Id.billItemRowAmount);
            SinglePrice = itemView.FindViewById<TextView>(Resource.Id.billItemRowSinglePrice);
            TotalPrice = itemView.FindViewById<TextView>(Resource.Id.billItemRowTotalPrice);
        }
    }
}
