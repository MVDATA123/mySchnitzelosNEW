using System;
using GCloud.Shared.Dto;
using UIKit;

namespace GCloudiPhone
{
    public partial class EBillTaxesTableViewCell : UITableViewCell
    {
        public EBillTaxesTableViewCell(IntPtr handle) : base(handle)
        {
        }

        public void UpdateCell(InvoiceTaxVATItem taxItem)
        {
            var netAmount = taxItem.VATRate > 0 ? (taxItem.Amount * 10) / (taxItem.VATRate / 10) : 0;
            var grossAmount = netAmount + taxItem.Amount;

            taxRateLabel.Text = taxItem.VATRate.ToString("F0") + "%";
            netAmountLabel.Text = netAmount.ToString("F");
            taxAmountLabel.Text = taxItem.Amount.ToString("F");
            grossAmountLabel.Text = grossAmount.ToString("F");
        }
    }
}