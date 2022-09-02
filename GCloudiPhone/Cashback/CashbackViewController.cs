using System;
using System.Collections.Generic;
using System.Globalization;
using CoreGraphics;
using GCloud.Shared.Dto.Domain;
using GCloudiPhone.Shared;
using GCloudShared.Service;
using GCloudShared.Shared;
using Refit;
using UIKit;

namespace GCloudiPhone
{
    public partial class CashbackViewController : UIViewController
    {
        readonly ICashbackService cashbackService;
        private List<CashbackDto> cashbackDtos;
        private UITableViewController cashbackTableController;

        public String StoreGuid { get; set; }

        public CashbackViewController(IntPtr handle) : base(handle)
        {
            cashbackService = RestService.For<ICashbackService>(HttpClientContainer.Instance.HttpClient);
            cashbackDtos = new List<CashbackDto>();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            cashbackTableController = new UITableViewController { TableView = CashbackTable };

            CashbackTable.RowHeight = 90;
            CashbackTable.Source = new CashbackTableSource(cashbackDtos);
            LoadCashback();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            NavigationItem.BackBarButtonItem = new UIBarButtonItem("Zurück", UIBarButtonItemStyle.Plain, null);
        }

        private async void LoadCashback()
        {
            try
            {
                cashbackDtos = await cashbackService.GetCashbacksForStore(StoreGuid);
            }
            catch (ApiException)
            {
                cashbackDtos = new List<CashbackDto>();
            }
            if (cashbackDtos.Count <= 0)
            {
                // show user that there's no data available
                TableViewHelper.EmptyMessage("Keine Daten vorhanden", new WeakReference<UITableViewController>(cashbackTableController));
                CashbackTable.TableFooterView = new UIView(new CGRect(0, 0, 0, 0));
                return;
            }
            cashbackDtos.Sort((c1, c2) => c2.CreditDateTime.CompareTo(c1.CreditDateTime));
            CashbackCredit.Text = String.Format("Guthaben: {0}", cashbackDtos[0].CreditNew.ToString("C", CultureInfo.GetCultureInfo("de-AT")));
            CashbackTable.Source = new CashbackTableSource(cashbackDtos);
            CashbackTable.ReloadData();
            CashbackTable.TableFooterView = new UIView(new CGRect(0, 0, 0, 0));
        }
    }
}