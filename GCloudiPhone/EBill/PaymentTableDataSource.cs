using System;
using Foundation;
using UIKit;
using GCloud.Shared.Dto;
using System.Collections.Generic;

namespace GCloudiPhone
{
    public class PaymentTableDataSource : UITableViewDataSource
    {
        private List<InvoicePaymentMethod> payments;

        public PaymentTableDataSource()
        {
        }

        public void UpdateTable(UITableView tableView, List<InvoicePaymentMethod> payments)
        {
            this.payments = payments;
            tableView.ReloadData();
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (EBillPaymentTableViewCell)tableView.DequeueReusableCell("EBillPaymentCell");
            if(cell == null)
            {
                throw new NotImplementedException();
            }

            cell.UpdateCell(payments[indexPath.Row]);

            return cell;
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return payments.Count;
        }
    }
}
