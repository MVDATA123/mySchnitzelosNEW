using Foundation;
using GCloud.Shared.Dto;
using System;
using UIKit;

namespace GCloudiPhone
{
    public partial class InvoiceTableViewCell : UITableViewCell
    {
        public InvoiceTableViewCell (IntPtr handle) : base (handle)
        {
        }

        public InvoiceTableViewCell() : base()
        {
        }

        public void UpdateCell(Invoice invoice)
        {
            CompanyLabel.Text = invoice.Biller.ComanyName;
            DateTimeLabel.Text = invoice.InvoiceDate.ToString("dd.MM.yyyy 'um' HH:mm 'Uhr'");
            TurnoverLabel.Text = invoice.InvoiceCurrency + " " + invoice.TotalGrossAmount.ToString("F");
        }
    }
}