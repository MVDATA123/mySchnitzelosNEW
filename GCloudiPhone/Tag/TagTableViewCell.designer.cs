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
    [Register ("TagTableViewCell")]
    partial class TagTableViewCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel TagLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (TagLabel != null) {
                TagLabel.Dispose ();
                TagLabel = null;
            }
        }
    }
}