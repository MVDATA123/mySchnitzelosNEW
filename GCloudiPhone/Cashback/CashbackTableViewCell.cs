using System;
using System.Globalization;
using GCloud.Shared.Dto.Domain;
using GCloudShared.Service;
using GCloudShared.Shared;
using Refit;
using UIKit;

namespace GCloudiPhone
{
    public partial class CashbackTableViewCell : UITableViewCell
    {
        public CashbackDto CashbackDto { get; set; }

        readonly ICashbackService cashbackService;
        readonly IStoreService storeService;
        
        public CashbackTableViewCell (IntPtr handle) : base (handle)
        {
            cashbackService = RestService.For<ICashbackService>(HttpClientContainer.Instance.HttpClient);
            storeService = RestService.For<IStoreService>(HttpClientContainer.Instance.HttpClient);
        }

        public CashbackTableViewCell(CashbackDto cashbackDto) {
            cashbackService = RestService.For<ICashbackService>(HttpClientContainer.Instance.HttpClient);
            storeService = RestService.For<IStoreService>(HttpClientContainer.Instance.HttpClient);

            SetCellData(cashbackDto);
        }

        public void UpdateCell(CashbackDto cashbackDto)
        {
            SetCellData(cashbackDto);
        }

        public void SetCellData(CashbackDto cashbackDto)
        {
            var cultureInfo = new CultureInfo("de-AT");
            CashbackCreditNew.Text = String.Format("Neues Guthaben: {0}", cashbackDto.CreditNew.ToString("C", cultureInfo));
            if (cashbackDto.CreditChange >= 0)
            {
                CashbackCreditChange.Text = String.Format("+{0}", cashbackDto.CreditChange.ToString("C", cultureInfo));
                CashbackTransactionImage.Image = UIImage.FromBundle("Cashback-In");
            }
            else {
                CashbackCreditChange.Text = cashbackDto.CreditChange.ToString("C", cultureInfo);
                CashbackCreditChange.Highlighted = true;
                CashbackTransactionImage.Image = UIImage.FromBundle("Cashback-Out");
            }
            CashbackDate.Text = String.Format("{0} um {1} Uhr", cashbackDto.CreditDateTime.ToString("dd. MMMM yyyy"), cashbackDto.CreditDateTime.ToString("HH:mm"));
            LoadStoreInfo(cashbackDto);
        }

        private async void LoadStoreInfo(CashbackDto cashbackDto) {
            var stores = await storeService.GetStores();
            var store = stores.Find(s => s.Id.Equals(cashbackDto.StoreId));
            CashbackStoreName.Text = String.Format("{0} - {1}", store.Name, store.Company.Name);
        }
    }
}