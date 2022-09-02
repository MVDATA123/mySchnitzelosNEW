using System;
using UIKit;
using GCloudiPhone.Caching;
using GCloudiPhone.Helpers;

namespace GCloudiPhone
{
    public partial class StartupViewController : UIViewController
    {
        public StartupViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            LoadData();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            //LoadData();
        }

        private async void LoadData() {
            await CachingService.GetAllData(true);

            var board = UIStoryboard.FromName("Main", null);
            var rootView = board.InstantiateViewController("ManagerRootViewController");
            //rootView.ModalPresentationStyle = UIModalPresentationStyle.Custom;
            rootView.ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;

            PresentViewController(rootView, true, null);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            if ((NavigationController == null && IsMovingFromParentViewController) || (ParentViewController != null && ParentViewController.IsBeingDismissed))
            {
                MemoryUtility.ReleaseUIViewWithChildren(this.View);
            }
        }
    }
}