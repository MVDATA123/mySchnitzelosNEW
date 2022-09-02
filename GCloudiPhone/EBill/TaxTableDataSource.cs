using System;
using Foundation;
using GCloud.Shared.Dto;
using UIKit;

namespace GCloudiPhone
{
    public class TaxTableDataSource : UITableViewDataSource
    {
        private InvoiceTaxVATItem[] taxes;

        public void UpdateTable(UITableView tableView, InvoiceTaxVATItem[] taxes)
        {
            this.taxes = taxes;
            tableView.ReloadData();
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (EBillTaxesTableViewCell)tableView.DequeueReusableCell("EBillTaxesCell");
            if(cell == null) { throw new NotImplementedException(); }

            cell.UpdateCell(taxes[indexPath.Row]);

            return cell;
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return taxes.Length;
        }
    }
}
