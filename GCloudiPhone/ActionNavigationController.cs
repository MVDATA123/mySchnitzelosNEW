using Foundation;
using System;
using UIKit;

namespace GCloudiPhone
{
    public partial class ActionNavigationController : UINavigationController
    {
        public ActionNavigationController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {

            var couponDetailViewController = segue.DestinationViewController as CouponListViewController;

            couponDetailViewController.CouponType = "WithoutSpecialProducts";

            base.PrepareForSegue(segue, sender);
        }

    }
}