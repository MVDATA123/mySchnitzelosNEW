using Foundation;
using System;
using UIKit;
using GCloud.Shared.Dto;

namespace GCloudiPhone
{
    public partial class EBillPaymentTableViewCell : UITableViewCell
    {
        public EBillPaymentTableViewCell (IntPtr handle) : base (handle)
        {
        }

        public void UpdateCell(InvoicePaymentMethod payment)
        {
            descriptionLabel.Text = payment.Comment;
            amountLabel.Text = payment.Amount.ToString("F");
        }
    }
}