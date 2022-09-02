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
    [Register ("CouponListItem")]
    partial class CouponListItem
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView CouponImage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel CouponRedeemsLeft { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel CouponTitle { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel CouponValidUntil { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel CouponValue { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (CouponImage != null) {
                CouponImage.Dispose ();
                CouponImage = null;
            }

            if (CouponRedeemsLeft != null) {
                CouponRedeemsLeft.Dispose ();
                CouponRedeemsLeft = null;
            }

            if (CouponTitle != null) {
                CouponTitle.Dispose ();
                CouponTitle = null;
            }

            if (CouponValidUntil != null) {
                CouponValidUntil.Dispose ();
                CouponValidUntil = null;
            }

            if (CouponValue != null) {
                CouponValue.Dispose ();
                CouponValue = null;
            }
        }
    }
}