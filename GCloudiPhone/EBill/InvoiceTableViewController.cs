using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreGraphics;
using Foundation;
using GCloud.Shared.Dto.Domain;
using GCloudiPhone.Shared;
using GCloudShared.Domain;
using GCloudShared.Service;
using GCloudShared.Shared;
using Refit;
using UIKit;

namespace GCloudiPhone
{
    public partial class InvoiceTableViewController : UITableViewController, IUISearchResultsUpdating
    {
        private readonly IBillService billService;
        private InvoiceTableDataSource tableDataSource;
        private List<Bill_Out_Dto> invoices;

        //private UISearchController searchController;


        public InvoiceTableViewController(IntPtr handle) : base(handle)
        {
            billService = RestService.For<IBillService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.TableHeaderView = new UIView(new CGRect(0, 0, 0, 0));
            TableView.TableFooterView = new UIView(new CGRect(0, 0, 0, 0));

            //searchController = new ColoredUISearchController(searchResultsController: null)
            //{
            //  DimsBackgroundDuringPresentation = false,
            //  HidesNavigationBarDuringPresentation = false
            //};

            //searchController.SearchResultsUpdater = this;
            //searchController.SearchBar.Placeholder = "Rechnungen durchsuchen";

           // if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
           // {
           //    NavigationItem.SearchController = searchController;
           //     NavigationItem.HidesSearchBarWhenScrolling = true;
           // }
           // else
           // {
              //searchController.SearchBar.SearchBarStyle = UISearchBarStyle.Default;
              //searchController.SearchBar.SizeToFit();
              //TableView.TableHeaderView = searchController.SearchBar;
              //searchController.SearchBar.ShowsCancelButton = true;
              //searchController.SearchBar.TintColor = UIColor.Gray;
           // }

            RefreshControl = new UIRefreshControl();
            TableView.AddSubview(RefreshControl);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            RefreshControl.ValueChanged += RefreshControl_ValueChanged;

            if (((AppDelegate)UIApplication.SharedApplication.Delegate).AuthState == AuthState.Authorized)
            {
                LoadInvoices();

                //searchController.SearchBar.Hidden = false;
            }
            else
            {
                TableViewHelper.EmptyMessage("Wenn du dich anmeldest werden hier deine Rechnungen angezeigt.", new WeakReference<UITableViewController>(this));

               // searchController.SearchBar.Hidden = true;
            }

            exportInvoices.Enabled = false; // Dodato jer izbacuje iz aplikacije (za potencijalnu buducu nadogradnju)
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;
        }

        void RefreshControl_ValueChanged(object sender, EventArgs e)
        {
            RefreshControl.BeginRefreshing();
            LoadInvoices();
        }

        private async void LoadInvoices()
        {
            try
            {
                invoices = await billService.Get(null);
                var x = invoices;
                    
            }
            catch (Exception e)
            {
                ((AppDelegate)UIApplication.SharedApplication.Delegate)._logRepository.Insert(new LogMessage {
                    Level = LogLevel.ERROR,
                    Message = e.Message,
                    StackTrace = e.StackTrace,
                    TimeStamp = DateTime.Now
                });

                ((AppDelegate)UIApplication.SharedApplication.Delegate).ShowNoInternetMessage();
            }
            //if (searchController != null && searchController.Active)
            //{
            //    UpdateSearchResultsForSearchController(searchController);
            //}
            //else
            //{
                var groupedInvoices = invoices.OrderByDescending(i => i.Invoice.InvoiceDate).GroupBy(i => i.Invoice.InvoiceDate.ToShortDateString()).Select(i => i.ToList()).ToList();

                if (groupedInvoices.Count == 0)
                {
                    TableView.BackgroundView = null;
                    TableViewHelper.EmptyMessage("Es sind noch keine Rechnungen verfügbar.", new WeakReference<UITableViewController>(this));
                    RefreshControl.EndRefreshing();


                }

                TableView.BackgroundView = null;

                if (tableDataSource == null)
                {

                    tableDataSource = new InvoiceTableDataSource();
                    TableView.WeakDataSource = tableDataSource;
                }
                tableDataSource.UpdateTable(TableView, groupedInvoices);
            //}
            RefreshControl.EndRefreshing();
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            if (segue.Identifier == "InvoiceDetailsSegue")
            {
                var invoiceDetailsVC = (InvoiceDetailsViewController)segue.DestinationViewController;
                invoiceDetailsVC.Invoice = tableDataSource.GetSelectedInvoice(TableView.IndexPathForSelectedRow);
            }

            //if (searchController != null && searchController.Active)
            //{
            //    searchController.Active = false;
            //}
        }

        partial void ExportInvoices_Activated(UIBarButtonItem sender)
        {
            var activitySpinner = new UIActivityIndicatorView();
            activitySpinner.StartAnimating();
            var rightButton = NavigationItem.RightBarButtonItem;
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(activitySpinner);
            DownloadCsv(rightButton);
        }

        private async void DownloadCsv(UIBarButtonItem rightButton)
        {
            var csvFile = await billService.Csv(Guid.Empty);
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filename = Path.Combine(documents, "Rechnungen.csv");

            File.WriteAllBytes(filename, await csvFile.ReadAsByteArrayAsync());

            NavigationItem.RightBarButtonItem = rightButton;

            var objectsToShare = new[] { NSUrl.FromFilename(filename) };
            var shareVC = new UIActivityViewController(objectsToShare, null);
            using (var popoverController = shareVC.PopoverPresentationController)
            {
                if (popoverController != null)
                {
                    popoverController.SourceView = this.View;
                }
            }

            this.PresentViewController(shareVC, true, null);
        }

        public void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            var query = searchController.SearchBar.Text;
            List<Bill_Out_Dto> resultList;

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();
                resultList = invoices.FindAll(invoiceDto => invoiceDto.Invoice.InvoiceNumber.Contains(query) || invoiceDto.Invoice.Biller.ComanyName.ToLower().Contains(query) || invoiceDto.Invoice.Biller.Address.Name.ToLower().Contains(query));
            }
            else
            {
                resultList = invoices;
            }

            if (tableDataSource == null)
            {
                tableDataSource = new InvoiceTableDataSource();
                TableView.WeakDataSource = tableDataSource;
            }

            if (resultList != null)
            {
                if (resultList.Count == 0)
                {
                    TableViewHelper.EmptyMessage("Keine Rechnungen gefunden.", new WeakReference<UITableViewController>(this));
                }
                else
                {
                    TableView.BackgroundView = null;
                }
                tableDataSource.UpdateTable(TableView, resultList.OrderByDescending(i => i.Invoice.InvoiceDate).GroupBy(i => i.Invoice.InvoiceDate.ToShortDateString()).Select(i => i.ToList()).ToList());
            }
        }

        //partial void InvoiceSearchBarButton_Activated(UIBarButtonItem sender)
       // {
           // if (searchController != null)
           // {
            //    searchController.Active = true;
          //  }
       // }
    }
}