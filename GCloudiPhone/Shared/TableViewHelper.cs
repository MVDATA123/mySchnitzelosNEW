using CoreGraphics;
using UIKit;
using System;

namespace GCloudiPhone.Shared
{
    public static class TableViewHelper
    {
        public static void EmptyMessage(string message, WeakReference<UITableViewController> viewController)
        {
            if (viewController.TryGetTarget(out var ctl))
            {
                var rect = new CGRect(new CGPoint(x: 0, y: 0), new CGSize(ctl.View.Bounds.Size.Width, ctl.View.Bounds.Size.Height));
                var messageLabel = new UILabel(rect)
                {
                    Text = message,
                    TextColor = UIColor.Black,
                    Lines = 0,
                    TextAlignment = UITextAlignment.Center,
                    Font = UIFont.PreferredBody
                };
                messageLabel.SizeToFit();

                ctl.TableView.BackgroundView = messageLabel;
            }
        }
    }
}
