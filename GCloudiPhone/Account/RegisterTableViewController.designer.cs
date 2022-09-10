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
    [Register("RegisterTableViewController")]
    partial class RegisterTableViewController
    {
        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIKit.UITextField BirthDateTextField { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIKit.UITableViewCell EmailCell { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIKit.UITextField EmailTextField { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIKit.UIActivityIndicatorView LoadingIndicator { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIKit.UITextField PasswdRepeatTextField { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIKit.UITableViewCell PasswordCell { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIKit.UITableViewCell PasswordRepeatCell { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIKit.UITextField PasswordTextField { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIKit.UIButton RegisterButton { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIKit.UITableView RegisterTable { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIKit.UITextField TxtInvitationCode { get; set; }

        [Action("RegisterButton_TouchUpInside:")]
        [GeneratedCode("iOS Designer", "1.0")]
        partial void RegisterButton_TouchUpInside(UIKit.UIButton sender);

        void ReleaseDesignerOutlets()
        {
            if (BirthDateTextField != null)
            {
                BirthDateTextField.Dispose();
                BirthDateTextField = null;
            }

            if (EmailCell != null)
            {
                EmailCell.Dispose();
                EmailCell = null;
            }

            if (EmailTextField != null)
            {
                EmailTextField.Dispose();
                EmailTextField = null;
            }

            if (LoadingIndicator != null)
            {
                LoadingIndicator.Dispose();
                LoadingIndicator = null;
            }

            if (PasswdRepeatTextField != null)
            {
                PasswdRepeatTextField.Dispose();
                PasswdRepeatTextField = null;
            }

            if (PasswordCell != null)
            {
                PasswordCell.Dispose();
                PasswordCell = null;
            }

            if (PasswordRepeatCell != null)
            {
                PasswordRepeatCell.Dispose();
                PasswordRepeatCell = null;
            }

            if (PasswordTextField != null)
            {
                PasswordTextField.Dispose();
                PasswordTextField = null;
            }

            if (RegisterButton != null)
            {
                RegisterButton.Dispose();
                RegisterButton = null;
            }

            if (RegisterTable != null)
            {
                RegisterTable.Dispose();
                RegisterTable = null;
            }

            if (TxtInvitationCode != null)
            {
                TxtInvitationCode.Dispose();
                TxtInvitationCode = null;
            }
        }
    }
}