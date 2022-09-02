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
    [Register ("EBillProductTableViewCell")]
    partial class EBillProductTableViewCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel amountLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lineTotalLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel pricePerUnitLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel productNameLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel taxRateLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (amountLabel != null) {
                amountLabel.Dispose ();
                amountLabel = null;
            }

            if (lineTotalLabel != null) {
                lineTotalLabel.Dispose ();
                lineTotalLabel = null;
            }

            if (pricePerUnitLabel != null) {
                pricePerUnitLabel.Dispose ();
                pricePerUnitLabel = null;
            }

            if (productNameLabel != null) {
                productNameLabel.Dispose ();
                productNameLabel = null;
            }

            if (taxRateLabel != null) {
                taxRateLabel.Dispose ();
                taxRateLabel = null;
            }
        }
    }
}