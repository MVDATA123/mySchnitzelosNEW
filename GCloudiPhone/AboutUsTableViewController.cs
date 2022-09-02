using System;
using Foundation;
using GCloudiPhone.Helpers;
using UIKit;

namespace GCloudiPhone
{
    public partial class AboutUsTableViewController : UITableViewController, ICanCleanUpMyself
    {
        public AboutUsTableViewController(IntPtr handle) : base(handle)
        {
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            if (segue.Identifier == "ImprintSegue")
            {
                var webViewController = segue.DestinationViewController as SettingsWebViewController;
                webViewController.Type = "Imprint";
            }
            else if (segue.Identifier == "DataProtectionSegue")
            {
                var webViewController = segue.DestinationViewController as SettingsWebViewController;
                webViewController.Type = "DataProtection";
            }
            else if (segue.Identifier == "TermsAndConditionSegue")
            {
                var webViewController = segue.DestinationViewController as SettingsWebViewController;
                webViewController.Type = "TC";
            }
            base.PrepareForSegue(segue, sender);
        }

        public override void ViewDidDisappear(bool animated)
        {
            if ((NavigationController == null && IsMovingFromParentViewController) || (ParentViewController != null && ParentViewController.IsBeingDismissed))
            {
                MemoryUtility.ReleaseUIViewWithChildren(this.View);
            }

            base.ViewDidDisappear(animated);
        }

        public void CleanUp()
        {
            //ReleaseDesignerOutlets();
        }
    }
}