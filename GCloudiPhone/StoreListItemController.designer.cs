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
    [Register ("StoreListItemController")]
    partial class StoreListItemController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView CompanyLogo { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel CompanyName { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel StoreName { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (CompanyLogo != null) {
                CompanyLogo.Dispose ();
                CompanyLogo = null;
            }

            if (CompanyName != null) {
                CompanyName.Dispose ();
                CompanyName = null;
            }

            if (StoreName != null) {
                StoreName.Dispose ();
                StoreName = null;
            }
        }
    }
}