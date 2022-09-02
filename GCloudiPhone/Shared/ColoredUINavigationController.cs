using System;
using UIKit;
using CoreGraphics;

namespace GCloudiPhone.Shared
{
    public class ColoredUINavigationController : UINavigationController
    {
        public ColoredUINavigationController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //var rect = new CGRect(0f, 0f, 1.0f, 1.0f);
            //UIGraphics.BeginImageContext(rect.Size);
            //using (var context = UIGraphics.GetCurrentContext())
            //{
            //    context.SetFillColor(new UIColor(1f, 87f / 255f, 34f / 255f, 1f).CGColor);
            //    context.FillRect(rect);
            //    var image = UIGraphics.GetImageFromCurrentImageContext();
            //    UIGraphics.EndImageContext();
            //    NavigationBar.SetBackgroundImage(image, UIBarMetrics.Default);
            //    NavigationBar.SetBackgroundImage(image, UIBarMetrics.DefaultPrompt);
            //}
        }
    }
}
