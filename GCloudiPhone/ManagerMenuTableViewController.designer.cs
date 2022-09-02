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
    [Register ("ManagerMenuTableViewController")]
    partial class ManagerMenuTableViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton CouponsButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton eBillButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton LogoutButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ReportsButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton StoresButton { get; set; }

        [Action ("CouponsButton_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void CouponsButton_TouchUpInside (UIKit.UIButton sender);

        [Action ("EBillButton_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void EBillButton_TouchUpInside (UIKit.UIButton sender);

        [Action ("StoresButton_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void StoresButton_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (CouponsButton != null) {
                CouponsButton.Dispose ();
                CouponsButton = null;
            }

            if (eBillButton != null) {
                eBillButton.Dispose ();
                eBillButton = null;
            }

            if (LogoutButton != null) {
                LogoutButton.Dispose ();
                LogoutButton = null;
            }

            if (ReportsButton != null) {
                ReportsButton.Dispose ();
                ReportsButton = null;
            }

            if (StoresButton != null) {
                StoresButton.Dispose ();
                StoresButton = null;
            }
        }
    }
}