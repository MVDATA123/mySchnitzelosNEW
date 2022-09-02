using System;
using System.Collections.Generic;
using Foundation;
using GCloudiPhone.Extensions;
using GCloudiPhone.Shared;
using GCloudShared.Extensions;
using GCloudShared.Service;
using GCloudShared.Shared;
using Refit;
using UIKit;

namespace GCloudiPhone
{
    public class StoreTableSource : UITableViewSource
    {
        List<StoreLocationDto> TableItems;
        bool isEditable;
        bool showDistance;
        IUserStoreService storeService;

        public StoreTableSource(List<StoreLocationDto> stores, bool isEditable = false, bool showDistance = false)
        {
            TableItems = stores;
            this.isEditable = isEditable;
            this.showDistance = showDistance;
            storeService = RestService.For<IUserStoreService>(HttpClientContainer.Instance.HttpClient);
        }

        public void UpdateTableSource(WeakReference<UITableView> tableViewRef, List<StoreLocationDto> stores, bool isEditable = false)
        {
            TableItems = null;
            this.isEditable = isEditable;
            TableItems = stores;
            // tableView is null!!??
            if (tableViewRef.TryGetTarget(out var tableView))
            {
                tableView.ReloadData();
            }
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var item = TableItems[indexPath.Row];
            var cell = (StoreListItem)tableView.DequeueReusableCell("StoreRow");

            //---- if there are no cells to reuse, create a new one
            if (cell == null)
            { cell = new StoreListItem(); }

            cell.UpdateCell(item, showDistance);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return TableItems.Count;
        }

        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            switch (editingStyle)
            {
                case UITableViewCellEditingStyle.Delete:
                    var store = TableItems[indexPath.Row];
                    TableItems.RemoveAt(indexPath.Row);
                    tableView.DeleteRows(new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);
                    // UnSub Notifications
                    DeleteFromWatchlist(store.Id);
                    break;
                case UITableViewCellEditingStyle.None:
                    break;
            }
        }

        public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
        {
            return "Entfernen";
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return isEditable;
        }

        private async void DeleteFromWatchlist(Guid storeId)
        {
            await Caching.CachingService.UnfollowStore(storeId.ToString());
            NotificationsHelper.Instance.UnsubscribeStore(storeId);
        }
    }
}
