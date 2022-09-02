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
    [Register ("StoreMapViewController")]
    partial class StoreMapViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISegmentedControl FindSegmentControl { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton FocusOnUserBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton FollowButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton NextStoreButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton PrevStoreButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel StoreInfoCompanyName { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView StoreInfoCouponsTable { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel StoreInfoStoreName { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView StoreInfoView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        MapKit.MKMapView StoreMapView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView StoreTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton UnFollowButton { get; set; }

        [Action ("FocusOnUserBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void FocusOnUserBtn_TouchUpInside (UIKit.UIButton sender);

        [Action ("NextStoreButton_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void NextStoreButton_TouchUpInside (UIKit.UIButton sender);

        [Action ("PrevStoreButton_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void PrevStoreButton_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (FindSegmentControl != null) {
                FindSegmentControl.Dispose ();
                FindSegmentControl = null;
            }

            if (FocusOnUserBtn != null) {
                FocusOnUserBtn.Dispose ();
                FocusOnUserBtn = null;
            }

            if (FollowButton != null) {
                FollowButton.Dispose ();
                FollowButton = null;
            }

            if (NextStoreButton != null) {
                NextStoreButton.Dispose ();
                NextStoreButton = null;
            }

            if (PrevStoreButton != null) {
                PrevStoreButton.Dispose ();
                PrevStoreButton = null;
            }

            if (StoreInfoCompanyName != null) {
                StoreInfoCompanyName.Dispose ();
                StoreInfoCompanyName = null;
            }

            if (StoreInfoCouponsTable != null) {
                StoreInfoCouponsTable.Dispose ();
                StoreInfoCouponsTable = null;
            }

            if (StoreInfoStoreName != null) {
                StoreInfoStoreName.Dispose ();
                StoreInfoStoreName = null;
            }

            if (StoreInfoView != null) {
                StoreInfoView.Dispose ();
                StoreInfoView = null;
            }

            if (StoreMapView != null) {
                StoreMapView.Dispose ();
                StoreMapView = null;
            }

            if (StoreTableView != null) {
                StoreTableView.Dispose ();
                StoreTableView = null;
            }

            if (UnFollowButton != null) {
                UnFollowButton.Dispose ();
                UnFollowButton = null;
            }
        }
    }
}