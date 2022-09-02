// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace GCloudiPhone
{
    [Register ("StoreCouponViewController")]
    partial class StoreCouponViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIBarButtonItem CashbackButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView CouponsTable { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIBarButtonItem NavigateButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView StoreImage { get; set; }

        [Action ("NavigateButton_Activated:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void NavigateButton_Activated (UIKit.UIBarButtonItem sender);

        void ReleaseDesignerOutlets ()
        {
            if (CashbackButton != null) {
                CashbackButton.Dispose ();
                CashbackButton = null;
            }

            if (CouponsTable != null) {
                CouponsTable.Dispose ();
                CouponsTable = null;
            }

            if (NavigateButton != null) {
                NavigateButton.Dispose ();
                NavigateButton = null;
            }

            if (StoreImage != null) {
                StoreImage.Dispose ();
                StoreImage = null;
            }
        }
    }
}