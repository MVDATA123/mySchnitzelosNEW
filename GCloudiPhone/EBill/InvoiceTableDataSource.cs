using System;
using System.Collections.Generic;
using Foundation;
using GCloud.Shared.Dto;
using GCloud.Shared.Dto.Domain;
using UIKit;

namespace GCloudiPhone
{
    public class InvoiceTableDataSource : UITableViewSource
    {
        private List<List<Bill_Out_Dto>> groupedInvoices;

        public void UpdateTable(UITableView tableView, List<List<Bill_Out_Dto>> groupedInvoices)
        {
            this.groupedInvoices = groupedInvoices;
            tableView.ReloadData();
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (InvoiceTableViewCell)tableView.DequeueReusableCell("InvoiceCell");
            if(cell == null) { cell = new InvoiceTableViewCell(); }

            cell.UpdateCell(groupedInvoices[(int)indexPath.Section][(int)indexPath.Row].Invoice);

            return cell;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return groupedInvoices.Count;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return groupedInvoices[(int)section].Count;
        }

        public override string TitleForHeader(UITableView tableView, nint section)
        {
            var invoiceDate = groupedInvoices[(int)section][0].Invoice.InvoiceDate;
            return invoiceDate.ToString("dd. MMMM yyyy");
        }

        public Invoice GetSelectedInvoice(NSIndexPath indexPath)
        {
            return groupedInvoices[indexPath.Section][indexPath.Row].Invoice;
        }
    }
}
