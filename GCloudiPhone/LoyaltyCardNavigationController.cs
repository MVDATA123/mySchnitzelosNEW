using Foundation;
using System;
using UIKit;
using CoreGraphics;
using GCloudiPhone.Shared;
using GCloudiPhone.Sidebar;
using GCloudiPhone.Helpers;

namespace GCloudiPhone
{
    public partial class LoyaltyCardNavigationController : MemoryOptimizedUINavigationController
    {
        public LoyaltyCardNavigationController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (ManagerRootViewController.Instance.TryGetTarget(out var managerRootView))
            {
                if (managerRootView.ManagerMenuControllerRef.TryGetTarget(out var managerMenuController)) {
                    managerMenuController.SidebarNavigationDelegate = new ManagerSidebarNavigationDelegate(this);
                }
            }
        }
    }
}