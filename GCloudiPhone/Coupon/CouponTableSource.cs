using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using GCloud.Shared.Dto.Domain;
using UIKit;

namespace GCloudiPhone
{
    public class CouponTableSource : UITableViewSource
    {
        List<CouponDto> TableItems;

        public CouponTableSource(List<CouponDto> items)
        {
            TableItems = items.Where(x => !x.ValidTo.HasValue || x.ValidTo.Value.Date >= DateTime.Now).OrderByDescending(c => c.IsValid).ToList();
            items = null;
        }

        public void UpdateTableSource(WeakReference<UITableView> tableViewRef, List<CouponDto> items)
        {
            TableItems = null;
            TableItems = items.Where(x => !x.ValidTo.HasValue || x.ValidTo.Value.Date >= DateTime.Now).OrderByDescending(c => c.IsValid).ToList();
            items = null;
            if (tableViewRef.TryGetTarget(out var tableView))
            {
                tableView.ReloadData();
            }
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return TableItems.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var item = TableItems[indexPath.Row];
            var cell = (CouponListItem)tableView.DequeueReusableCell("CouponRow", indexPath);

            //---- if there are no cells to reuse, create a new one
            if (cell == null)
            { cell = new CouponListItem(); }

            cell.UpdateCell(item);

            return cell;
        }
    }
}
