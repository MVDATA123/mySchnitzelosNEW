using UIKit;
using WebKit;

namespace GCloudiPhone.SettingsWebView
{
    public class SettingsWebViewNavigationDelegate : WKNavigationDelegate
    {
        public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;  
        }

        public override void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
        {
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;  
        }
    }
}
