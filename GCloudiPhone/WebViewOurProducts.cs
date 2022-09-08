 using Foundation;
using System;
using UIKit;
using WebKit;

namespace GCloudiPhone
{
    public partial class WebViewOurProducts : UIViewController
    {

        private readonly NSUrl url = new NSUrl("https://myschnitzel.at/apppart/speisekarten");

        public WebViewOurProducts (IntPtr handle) : base (handle)
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