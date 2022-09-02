using Foundation;
using GCloud.Shared.Dto;
using System;
using UIKit;

namespace GCloudiPhone
{
    public partial class EBillProductTableViewCell : UITableViewCell
    {
        public EBillProductTableViewCell (IntPtr handle) : base (handle)
        {
        }

        public void UpdateCell(InvoiceDetailsItemListListLineItem invoiceLine)
        {
            productNameLabel.Text = invoiceLine.Description;
            taxRateLabel.Text = invoiceLine.VATRate.ToString();
            amountLabel.Text = invoiceLine.Quantity.Value.ToString();
            pricePerUnitLabel.Text = invoiceLine.UnitPrice.ToString("F");
            lineTotalLabel.Text = invoiceLine.LineItemAmount.ToString("F");
        }
    }
}