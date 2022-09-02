using System;
using Foundation;
using UIKit;
using GCloudiPhone.Shared;

namespace GCloudiPhone
{
    public class FilterMenuTableSource : UITableViewSource
    {
        private readonly WeakReference<FilterMenuTableViewController> filterMenuTableViewController;
        private readonly WeakReference<UITableView> tableView;

        public FilterMenuTableSource(FilterMenuTableViewController filterMenuTableViewController, UITableView tableView)
        {
            this.filterMenuTableViewController = new WeakReference<FilterMenuTableViewController>(filterMenuTableViewController);
            this.tableView = new WeakReference<UITableView>(tableView);
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            return this.tableView.TryGetTarget(out var table) ? table.CellAt(indexPath) : null;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return tableview.NumberOfRowsInSection(section);
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);

            if (indexPath.Section == 0 && indexPath.Row == 0)
            {
                if (filterMenuTableViewController.TryGetTarget(out var ctl))
                {
                    if (FilterSidebarViewController.Instance.TryGetTarget(out var filterSidebar))
                    {
                        filterSidebar.PerformSegue("TagsFilterSegue", NSObjectWrapper.Wrap(ctl.Tags));
                    }
                }
            }
        }
    }
}
