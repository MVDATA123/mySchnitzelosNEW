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
    [Register ("InvitationCodeViewController")]
    partial class InvitationCodeViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel EmailAddressTextLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel InvitationCodeLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ShareInvitationCodeButton { get; set; }

        [Action ("ShareInvitationCodeButton_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void ShareInvitationCodeButton_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (EmailAddressTextLabel != null) {
                EmailAddressTextLabel.Dispose ();
                EmailAddressTextLabel = null;
            }

            if (InvitationCodeLabel != null) {
                InvitationCodeLabel.Dispose ();
                InvitationCodeLabel = null;
            }

            if (ShareInvitationCodeButton != null) {
                ShareInvitationCodeButton.Dispose ();
                ShareInvitationCodeButton = null;
            }
        }
    }
}