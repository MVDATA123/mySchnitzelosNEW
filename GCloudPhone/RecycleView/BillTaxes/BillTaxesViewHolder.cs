using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace mvdata.foodjet.RecycleView.BillTaxes
{
    public class BillTaxesViewHolder : RecyclerView.ViewHolder
    {
        public TextView TaxDesc { get; set; }
        public TextView TaxNet { get; set; }
        public TextView Tax { get; set; }
        public TextView TaxGross { get; set; }

        public BillTaxesViewHolder(View itemView) : base(itemView)
        {
            TaxDesc = itemView.FindViewById<TextView>(Resource.Id.billTaxItemTaxDesc);
            TaxNet = itemView.FindViewById<TextView>(Resource.Id.billTaxItemNet);
            Tax = itemView.FindViewById<TextView>(Resource.Id.billTaxItemTax);
            TaxGross = itemView.FindViewById<TextView>(Resource.Id.billTaxItemGross);
        }
    }
}
