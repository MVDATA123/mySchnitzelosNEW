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
    [Register ("ActionController")]
    partial class ActionController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btn { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btn != null) {
                btn.Dispose ();
                btn = null;
            }
        }
    }
}