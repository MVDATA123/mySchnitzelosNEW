using Foundation;
using System;
using UIKit;

namespace GCloudiPhone
{
    public partial class OnlineShopController : UIViewController
    {
        public OnlineShopController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

           // PerformSegue("storeListViewController", this);

        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {

            var storeListViewController = segue.DestinationViewController as StoreListViewController;

           // couponDetailViewController.CouponType = "WithoutSpecialProducts";


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