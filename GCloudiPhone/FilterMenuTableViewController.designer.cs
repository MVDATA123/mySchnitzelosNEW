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
    [Register ("FilterMenuTableViewController")]
    partial class FilterMenuTableViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel DistanceLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISlider DistanceSlider { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView FilterMenuTable { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton SelectTagsButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel TagsLabel { get; set; }

        [Action ("SelectTagsButton_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void SelectTagsButton_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (DistanceLabel != null) {
                DistanceLabel.Dispose ();
                DistanceLabel = null;
            }

            if (DistanceSlider != null) {
                DistanceSlider.Dispose ();
                DistanceSlider = null;
            }

            if (FilterMenuTable != null) {
                FilterMenuTable.Dispose ();
                FilterMenuTable = null;
            }

            if (SelectTagsButton != null) {
                SelectTagsButton.Dispose ();
                SelectTagsButton = null;
            }

            if (TagsLabel != null) {
                TagsLabel.Dispose ();
                TagsLabel = null;
            }
        }
    }
}