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
    [Register ("EBillTaxesTableViewCell")]
    partial class EBillTaxesTableViewCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel grossAmountLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel netAmountLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel taxAmountLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel taxRateLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (grossAmountLabel != null) {
                grossAmountLabel.Dispose ();
                grossAmountLabel = null;
            }

            if (netAmountLabel != null) {
                netAmountLabel.Dispose ();
                netAmountLabel = null;
            }

            if (taxAmountLabel != null) {
                taxAmountLabel.Dispose ();
                taxAmountLabel = null;
            }

            if (taxRateLabel != null) {
                taxRateLabel.Dispose ();
                taxRateLabel = null;
            }
        }
    }
}