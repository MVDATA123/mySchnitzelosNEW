using Foundation;
using System;
using UIKit;

namespace GCloudiPhone
{
    public partial class StoreDetailTableHeaderView : UIView
    {
        public string StoreName
        {
            set { StoreNameLabel.Text = value; }
        }

        public string CompanyName
        {
            set { CompanyNameLabel.Text = value; }
        }

        public string StoreAddress
        {
            set { StoreAddressLabel.Text = value; }
        }

        public UIButton Follow
        {
            get { return FollowButton; }
        }

        public UIButton Unfollow
        {
            get { return UnfollowButton; }
        }

        public UIButton EnableNotifications
        {
            get { return EnableNotificationsButton; }
        }

        public UIButton DisableNotifications
        {
            get { return DisableNotificationsButton; }
        }

        public StoreDetailTableHeaderView(IntPtr handle) : base(handle)
        {
        }
    }
}