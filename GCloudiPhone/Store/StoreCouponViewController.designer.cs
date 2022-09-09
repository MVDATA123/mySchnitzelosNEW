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
        UIKit.UITableView CouponsTable { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView StoreImage { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (CouponsTable != null) {
                CouponsTable.Dispose ();
                CouponsTable = null;
            }

            if (StoreImage != null) {
                StoreImage.Dispose ();
                StoreImage = null;
            }
        }
    }
}