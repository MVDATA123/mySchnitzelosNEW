using Foundation;
using System;
using UIKit;
using GCloudiPhone.Sidebar;

namespace GCloudiPhone
{
    public partial class ManagerMenuTableViewController : UITableViewController
    {
        public SidebarNavigationDelegate SidebarNavigationDelegate;

        private UIStoryboard storyboard;

        public ManagerMenuTableViewController(IntPtr handle) : base(handle)
        {
            storyboard = UIStoryboard.FromName("Main", null);
        }

        private void NavigateToViewController(string storyboardId)
        {
            var viewController = storyboard.InstantiateViewController(storyboardId);
            SidebarNavigationDelegate.PerformNavigation(viewController);
        }

        partial void StoresButton_TouchUpInside(UIButton sender)
        {
            NavigateToViewController("ManagerStoreTableViewController");
        }

        partial void CouponsButton_TouchUpInside(UIButton sender)
        {
            NavigateToViewController("ManagerCouponTableViewController");
        }

        partial void EBillButton_TouchUpInside(UIButton sender)
        {
            throw new NotImplementedException();
        }

        public static implicit operator WeakReference<object>(ManagerMenuTableViewController v)
        {
            throw new NotImplementedException();
        }
    }
}