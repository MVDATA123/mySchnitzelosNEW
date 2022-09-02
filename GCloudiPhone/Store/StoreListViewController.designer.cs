// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace GCloudiPhone
{
    [Register ("StoreListViewController")]
    partial class StoreListViewController
    {
        [Outlet]
        UIKit.UITableView StoreList { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIBarButtonItem AddStoreButton { get; set; }

        [Action ("AddStoreButton_Activated:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void AddStoreButton_Activated (UIKit.UIBarButtonItem sender);

        void ReleaseDesignerOutlets ()
        {
            if (AddStoreButton != null) {
                AddStoreButton.Dispose ();
                AddStoreButton = null;
            }
        }
    }
}