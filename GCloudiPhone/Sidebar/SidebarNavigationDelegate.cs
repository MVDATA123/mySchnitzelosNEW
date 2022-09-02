using System;
using UIKit;

namespace GCloudiPhone.Sidebar
{
    public interface SidebarNavigationDelegate
    {
        void PerformNavigation(UIViewController destinationController);
    }
}
