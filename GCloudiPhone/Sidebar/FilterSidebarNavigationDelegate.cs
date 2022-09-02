using System;
using UIKit;

namespace GCloudiPhone.Sidebar
{
    public class FilterSidebarNavigationDelegate : SidebarNavigationDelegate
    {
        private readonly WeakReference<UIViewController> viewController;

        public FilterSidebarNavigationDelegate(UIViewController viewController)
        {
            this.viewController = new WeakReference<UIViewController>(viewController);
        }

        public void PerformNavigation(UIViewController destinationController)
        {
            if (viewController.TryGetTarget(out var ctl))
                ctl.PresentViewController(destinationController, true, null);
        }
    }
}
