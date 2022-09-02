using Foundation;
using System;
using UIKit;
using SidebarNavigation;
using GCloudiPhone.Shared;
using System.Collections.Generic;
using GCloud.Shared.Dto.Domain;

namespace GCloudiPhone
{
    public partial class FilterSidebarViewController : UIViewController
    {
        public static WeakReference<FilterSidebarViewController> Instance { get; private set; }
        public WeakReference<GCloudiPhone.TabBarController> MainTabBarControllerRef { get; private set; }
        public SidebarController SidebarController { get; private set; }
        public WeakReference<FilterMenuTableViewController> FilterMenuControllerRef { get; set; }

        public FilterSidebarViewController(IntPtr handle) : base(handle)
        {
            Instance = new WeakReference<FilterSidebarViewController>(this);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var storyboard = UIStoryboard.FromName("Main", null);
            MainTabBarControllerRef = new WeakReference<TabBarController>(storyboard.InstantiateViewController("TabBarController") as GCloudiPhone.TabBarController);
            FilterMenuControllerRef = new WeakReference<FilterMenuTableViewController>(storyboard.InstantiateViewController("FilterMenuTableViewController") as FilterMenuTableViewController);

            if (MainTabBarControllerRef.TryGetTarget(out var mainTabBarController) && FilterMenuControllerRef.TryGetTarget(out var filterMenuController))
                SidebarController = new SidebarController(this, mainTabBarController, filterMenuController)
                {
                    Disabled = true
                };
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            if (segue.Identifier == "TagsFilterSegue")
            {
                var tagsController = segue.DestinationViewController as TagSearchTableViewController;
                tagsController.SelectedTags = (sender as NSObjectWrapper).Context as List<TagDto>;
            }

            base.PrepareForSegue(segue, sender);
        }
    }
}