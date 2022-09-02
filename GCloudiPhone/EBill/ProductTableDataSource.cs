using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using GCloud.Shared.Dto;

namespace GCloudiPhone
{
    public class ProductTableDataSource : UITableViewSource
    {
        private InvoiceDetailsItemListListLineItem[] products;

        public void UpdateTable(UITableView tableView, InvoiceDetailsItemListListLineItem[] products)
        {
            this.products = products;
            tableView.ReloadData();
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (EBillProductTableViewCell)tableView.DequeueReusableCell("eBillProductCell");
            if(cell == null) { throw new NotImplementedException(); }

            cell.UpdateCell(products[indexPath.Row]);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return products.Length;
        }
    }
}
