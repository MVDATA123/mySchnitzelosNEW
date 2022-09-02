using Foundation;
using System;
using UIKit;
using SidebarNavigation;
using UserNotifications;
using GCloudiPhone.Shared;
using Firebase.CloudMessaging;
using System.Threading.Tasks;
using System.Threading;
using GCloudiPhone.Caching;

namespace GCloudiPhone
{
    public partial class ManagerRootViewController : UIViewController
    {
        public static WeakReference<ManagerRootViewController> Instance { get; private set; }
        public SidebarController SidebarController { get; private set; }
        public WeakReference<FilterSidebarViewController> FilterSidebarControllerRef { get; private set; }
        public WeakReference<ManagerMenuTableViewController> ManagerMenuControllerRef { get; private set; }

        public ManagerRootViewController (IntPtr handle) : base (handle)
        {
            Instance = new WeakReference<ManagerRootViewController>(this);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var storyboard = UIStoryboard.FromName("Main", null);
            FilterSidebarControllerRef = new WeakReference<FilterSidebarViewController>(storyboard.InstantiateViewController("FilterSidebarViewController") as FilterSidebarViewController);
            ManagerMenuControllerRef = new WeakReference<ManagerMenuTableViewController>(storyboard.InstantiateViewController("ManagerNavigationMenu") as ManagerMenuTableViewController);

            if (FilterSidebarControllerRef.TryGetTarget(out var rootViewController) && ManagerMenuControllerRef.TryGetTarget(out var managerMenuController))
            {
                SidebarController = new SidebarController(this, rootViewController, managerMenuController)
                {
                    Disabled = true,
                    MenuLocation = MenuLocations.Left
                };
            }
        }
    }
}