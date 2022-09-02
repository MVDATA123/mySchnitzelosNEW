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
    [Register ("TotalPointsViewController")]
    partial class TotalPointsViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton freundeWerbenButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel InvitationCodeLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel TotalPointsLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (freundeWerbenButton != null) {
                freundeWerbenButton.Dispose ();
                freundeWerbenButton = null;
            }

            if (InvitationCodeLabel != null) {
                InvitationCodeLabel.Dispose ();
                InvitationCodeLabel = null;
            }

            if (TotalPointsLabel != null) {
                TotalPointsLabel.Dispose ();
                TotalPointsLabel = null;
            }
        }
    }
}