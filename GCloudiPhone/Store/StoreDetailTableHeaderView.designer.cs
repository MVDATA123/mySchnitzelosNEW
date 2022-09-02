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
    [Register ("StoreDetailTableHeaderView")]
    partial class StoreDetailTableHeaderView
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel CompanyNameLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton DisableNotificationsButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton EnableNotificationsButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton FollowButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel StoreAddressLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel StoreNameLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton UnfollowButton { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (CompanyNameLabel != null) {
                CompanyNameLabel.Dispose ();
                CompanyNameLabel = null;
            }

            if (DisableNotificationsButton != null) {
                DisableNotificationsButton.Dispose ();
                DisableNotificationsButton = null;
            }

            if (EnableNotificationsButton != null) {
                EnableNotificationsButton.Dispose ();
                EnableNotificationsButton = null;
            }

            if (FollowButton != null) {
                FollowButton.Dispose ();
                FollowButton = null;
            }

            if (StoreAddressLabel != null) {
                StoreAddressLabel.Dispose ();
                StoreAddressLabel = null;
            }

            if (StoreNameLabel != null) {
                StoreNameLabel.Dispose ();
                StoreNameLabel = null;
            }

            if (UnfollowButton != null) {
                UnfollowButton.Dispose ();
                UnfollowButton = null;
            }
        }
    }
}