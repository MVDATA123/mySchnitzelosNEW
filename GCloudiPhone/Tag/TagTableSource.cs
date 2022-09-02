using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using GCloud.Shared.Dto.Domain;
using UIKit;

namespace GCloudiPhone
{
    public class TagTableSource : UITableViewSource
    {
        List<TagDto> TableItems;
        private TagSearchTableViewController TableViewController { get; set; }

        public TagTableSource(List<TagDto> items, TagSearchTableViewController tableViewController)
        {
            TableItems = items;
            TableViewController = tableViewController;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return TableItems.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var item = TableItems[indexPath.Row];
            var cell = (TagTableViewCell)tableView.DequeueReusableCell("TagRow");

            //---- if there are no cells to reuse, create a new one
            if (cell == null)
            { cell = new TagTableViewCell(item); }

            cell.UpdateCell(item);
            //if (TableViewController.SelectedTags.FirstOrDefault(t => t.Id.Equals(item.Id)) != null)
            //{
            //    cell.Accessory = UITableViewCellAccessory.Checkmark;
            //    cell.Selected = true;
            //}
            //else
            //{
            //    cell.Accessory = UITableViewCellAccessory.None;
            //    cell.Selected = false;
            //}

            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.CellAt(indexPath);
            cell.Accessory = UITableViewCellAccessory.Checkmark;
            var tag = TableItems.ElementAt(indexPath.Row);
            if (!TableViewController.SelectedTags.Any(t => t.Id.Equals(tag.Id)))
            {
                TableViewController.SelectedTags.Add(tag);
            }
        }

        public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.CellAt(indexPath);
            cell.Accessory = UITableViewCellAccessory.None;
            var tag = TableItems.ElementAt(indexPath.Row);
            TableViewController.SelectedTags.RemoveAt(TableViewController.SelectedTags.FindIndex(t => t.Id.Equals(tag.Id)));
        }
    }
}
