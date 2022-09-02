using Foundation;
using GCloudiPhone.Helpers;
using System;
using UIKit;

namespace GCloudiPhone
{
    public partial class AboutUsNavigationController : UINavigationController
    {
        public AboutUsNavigationController (IntPtr handle) : base (handle)
        {
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