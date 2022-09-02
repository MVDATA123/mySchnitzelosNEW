// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace GCloudiPhone
{
    [Register ("CouponDetailController")]
    partial class CouponDetailController
    {
        [Outlet]
        UIKit.UIImageView CouponQrCode { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel CouponDescriptionTextField { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView CouponImage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel CouponValueLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel RedeemsLeftTextField { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView ShadowView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ValiditySpanTextField { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (CouponDescriptionTextField != null) {
                CouponDescriptionTextField.Dispose ();
                CouponDescriptionTextField = null;
            }

            if (CouponImage != null) {
                CouponImage.Dispose ();
                CouponImage = null;
            }

            if (CouponValueLabel != null) {
                CouponValueLabel.Dispose ();
                CouponValueLabel = null;
            }

            if (RedeemsLeftTextField != null) {
                RedeemsLeftTextField.Dispose ();
                RedeemsLeftTextField = null;
            }

            if (ShadowView != null) {
                ShadowView.Dispose ();
                ShadowView = null;
            }

            if (ValiditySpanTextField != null) {
                ValiditySpanTextField.Dispose ();
                ValiditySpanTextField = null;
            }
        }
    }
}