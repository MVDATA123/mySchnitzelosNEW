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
    [Register ("InvoiceTableViewController")]
    partial class InvoiceTableViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIBarButtonItem exportInvoices { get; set; }

        [Action ("ExportInvoices_Activated:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void ExportInvoices_Activated (UIKit.UIBarButtonItem sender);

        void ReleaseDesignerOutlets ()
        {
            if (exportInvoices != null) {
                exportInvoices.Dispose ();
                exportInvoices = null;
            }
        }
    }
}