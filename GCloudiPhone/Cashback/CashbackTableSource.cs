using System;
using UIKit;
using CoreGraphics;
using Foundation;
using System.Collections.Generic;
using GCloud.Shared.Dto.Domain;

namespace GCloudiPhone
{
    public class CashbackTableSource : UITableViewSource
    {
        List<CashbackDto> TableItems;

        public CashbackTableSource(List<CashbackDto> items)
        {
            TableItems = items;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return TableItems.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var item = TableItems[indexPath.Row];
            var cell = (CashbackTableViewCell)tableView.DequeueReusableCell("CashbackRow");

            //---- if there are no cells to reuse, create a new one
            if (cell == null)
            { cell = new CashbackTableViewCell(item); }

            cell.UpdateCell(item);

            return cell;
        }
    }
}
