using Foundation;
using System;
using GCloudiPhone.Extensions;
using UIKit;
using Refit;
using GCloudShared.Service;
using GCloudShared.Shared;

namespace GCloudiPhone
{
    public partial class StoreListItem : UITableViewCell
    {
        public StoreLocationDto Store { get; set; }

        public StoreListItem(IntPtr handle) : base(handle)
        {
        }

        public StoreListItem()
        {
        }

        public void UpdateCell(StoreLocationDto store, bool showDistance)
        {
            SetCellData(store, showDistance);
        }

        public void SetCellData(StoreLocationDto store, bool showDistance = true)
        {
            Store = store;
            StoreName.Text = store.Name;
            //CompanyName.Text = store.Company.Name;
            //CompanyName.Hidden = true;
            AddressLabel.Text = store.Address;
            if (showDistance)
            {
                DistanceLabel.Hidden = false;
                DistanceLabel.Text = FormatDistanceToUser();
            }
            else
            {
                DistanceLabel.Hidden = true;
            }
        }

        private string FormatDistanceToUser()
        {
            if (!Store.DistanceToUser.HasValue)
            {
                return "";
            }

            if ((Store.DistanceToUser.Value / 1000) >= 1)
            {
                return decimal.Round((decimal)(Store.DistanceToUser.Value / 1000), 2, MidpointRounding.AwayFromZero) + "km";
            }
            else
            {
                return decimal.Round((decimal)Store.DistanceToUser.Value, MidpointRounding.AwayFromZero) + "m";
            }
        }
    }
}