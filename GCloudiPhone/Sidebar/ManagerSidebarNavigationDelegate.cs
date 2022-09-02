using System;
using UIKit;

namespace GCloudiPhone.Sidebar
{
    public class ManagerSidebarNavigationDelegate : SidebarNavigationDelegate
    {
        private readonly WeakReference<UINavigationController> navigationController;

        public ManagerSidebarNavigationDelegate(UINavigationController navigationController)
        {
            this.navigationController = new WeakReference<UINavigationController>(navigationController);
        }

        public void PerformNavigation(UIViewController destinationController)
        {
            if  (ManagerRootViewController.Instance.TryGetTarget(out var managerRoot) && navigationController.TryGetTarget(out var ctl))
            {
                managerRoot.SidebarController.CloseMenu();
                ctl.PushViewController(destinationController, true);
            }
        }
    }
}
