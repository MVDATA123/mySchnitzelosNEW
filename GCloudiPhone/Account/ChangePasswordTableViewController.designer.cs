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
    [Register ("ChangePasswordTableViewController")]
    partial class ChangePasswordTableViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIBarButtonItem ChangePwBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView ChangePwTable { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField ConfirmNewPwLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField NewPwLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField OldPwLabel { get; set; }

        [Action ("ChangePwBtn_Activated:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void ChangePwBtn_Activated (UIKit.UIBarButtonItem sender);

        void ReleaseDesignerOutlets ()
        {
            if (ChangePwBtn != null) {
                ChangePwBtn.Dispose ();
                ChangePwBtn = null;
            }

            if (ChangePwTable != null) {
                ChangePwTable.Dispose ();
                ChangePwTable = null;
            }

            if (ConfirmNewPwLabel != null) {
                ConfirmNewPwLabel.Dispose ();
                ConfirmNewPwLabel = null;
            }

            if (NewPwLabel != null) {
                NewPwLabel.Dispose ();
                NewPwLabel = null;
            }

            if (OldPwLabel != null) {
                OldPwLabel.Dispose ();
                OldPwLabel = null;
            }
        }
    }
}