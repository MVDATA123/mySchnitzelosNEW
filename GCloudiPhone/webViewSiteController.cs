using Foundation;
using System;
using UIKit;
using WebKit;

namespace GCloudiPhone
{
    public partial class webViewSiteController : UIViewController
    {
        private readonly NSUrl url = new NSUrl("https://www.b92.net");

        public webViewSiteController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var webView = new WKWebView(View.Frame, new WKWebViewConfiguration());
            View.AddSubview(webView);

            webView.LoadRequest(new NSUrlRequest(url));
        }
    }
}