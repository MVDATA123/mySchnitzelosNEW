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
    [Register ("InvoiceTableViewCell")]
    partial class InvoiceTableViewCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel CompanyLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel DateTimeLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel TurnoverLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (CompanyLabel != null) {
                CompanyLabel.Dispose ();
                CompanyLabel = null;
            }

            if (DateTimeLabel != null) {
                DateTimeLabel.Dispose ();
                DateTimeLabel = null;
            }

            if (TurnoverLabel != null) {
                TurnoverLabel.Dispose ();
                TurnoverLabel = null;
            }
        }
    }
}