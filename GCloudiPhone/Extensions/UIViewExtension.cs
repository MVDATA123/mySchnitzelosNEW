using Foundation;
using UIKit;

namespace GCloudiPhone.Extensions
{
    public static class UIViewExtension
    {
        public static UIImage AsImage(this UIView view)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                var renderer = new UIGraphicsImageRenderer(view.Bounds.Size);
                return renderer.CreateImage(context =>
                {
                    view.Layer.RenderInContext(context.CGContext);
                });
            }
            else
            {
                //Do something
                return new UIImage();
            }
        }

        public static NSData AsPdf(this UIView view)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                var renderer = new UIGraphicsPdfRenderer(view.Bounds, UIGraphicsPdfRendererFormat.DefaultFormat);
                return renderer.CreatePdf(context =>
                {
                    context.BeginPage();
                    view.Layer.RenderInContext(context.CGContext);
                });
            }
            else
            {
                //Do something
                return new NSData();
            }
        }
    }
}
