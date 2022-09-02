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
    [Register ("StartupViewController")]
    partial class StartupViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView AppLogo { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (AppLogo != null) {
                AppLogo.Dispose ();
                AppLogo = null;
            }
        }
    }
}