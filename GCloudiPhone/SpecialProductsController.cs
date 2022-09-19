using Foundation;
using System;
using UIKit;

namespace GCloudiPhone
{
    public partial class SpecialProductsController : UIViewController
    {
        public SpecialProductsController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //PerformSegue("CouponListViewController", this);

        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {

            var couponDetailViewController = segue.DestinationViewController as CouponListViewController;

            couponDetailViewController.CouponType = "WithSpecialProducts";


            //if (segue.Identifier == "CouponDetailsSegue")
            //{
            //    var couponDetailController = segue.DestinationViewController as CouponDetailController;
            //    //var couponListItem = sender as CouponListItem;
            //    //couponDetailController.Coupon = couponListItem.Coupon;
            //}

            //if (segue.Identifier == "PopUpSegue")
            //{
            //    var storeDetailViewController = segue.DestinationViewController as StoreCouponViewController;
            //    //storeDetailViewController.Store = ((NSObjectWrapper)sender).Context as StoreLocationDto;
            //    //if (backButton == null)
            //    //{
            //    //    backButton = new UIBarButtonItem("Karte", UIBarButtonItemStyle.Plain, null, null);
            //    //}
            //    //NavigationItem.BackBarButtonItem = backButton;
            //}

            base.PrepareForSegue(segue, sender);
        }

       
    }
}