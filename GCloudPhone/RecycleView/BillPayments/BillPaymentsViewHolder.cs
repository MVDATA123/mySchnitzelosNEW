using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace mvdata.foodjet.RecycleView.BillPayments
{
    public class BillPaymentsViewHolder : RecyclerView.ViewHolder
    {
        public TextView PaymentDesc;
        public TextView PaymentAmount;

        public BillPaymentsViewHolder(View itemView) : base(itemView)
        {
            PaymentDesc = itemView.FindViewById<TextView>(Resource.Id.billPaymentDesc);
            PaymentAmount = itemView.FindViewById<TextView>(Resource.Id.billPaymentAmount);
        }
    }
}
