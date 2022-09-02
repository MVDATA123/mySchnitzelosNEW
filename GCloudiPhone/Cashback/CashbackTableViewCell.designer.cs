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
    [Register ("CashbackTableViewCell")]
    partial class CashbackTableViewCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel CashbackCreditChange { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel CashbackCreditNew { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel CashbackDate { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel CashbackStoreName { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView CashbackTransactionImage { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (CashbackCreditChange != null) {
                CashbackCreditChange.Dispose ();
                CashbackCreditChange = null;
            }

            if (CashbackCreditNew != null) {
                CashbackCreditNew.Dispose ();
                CashbackCreditNew = null;
            }

            if (CashbackDate != null) {
                CashbackDate.Dispose ();
                CashbackDate = null;
            }

            if (CashbackStoreName != null) {
                CashbackStoreName.Dispose ();
                CashbackStoreName = null;
            }

            if (CashbackTransactionImage != null) {
                CashbackTransactionImage.Dispose ();
                CashbackTransactionImage = null;
            }
        }
    }
}