using Foundation;
using System;
using UIKit;
using WebKit;
using GCloud.Shared;
using GCloudiPhone.Helpers;

namespace GCloudiPhone
{
    public partial class SettingsWebViewController : UIViewController, ICanCleanUpMyself
    {
        public string Type { get; set; }
        private WKWebView webView;

        public SettingsWebViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();


        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            var webViewConfig = new WKWebViewConfiguration
            {
                Preferences = new WKPreferences
                {
                    JavaScriptEnabled = false
                }

            };
            var userController = new WKUserContentController();

            var cssString = @".cc-window.cc-banner { display: none; }";
            var jsHideCookieConsent = $@"var hideConsentAndNavbar = document.createElement('style'); hideConsentAndNavbar.innerHTML = '{cssString}'; document.head.appendChild(hideConsentAndNavbar)";

            var hideNavBarAndCookieConsentScript = new WKUserScript(new NSString(jsHideCookieConsent), WKUserScriptInjectionTime.AtDocumentEnd, false);
            userController.AddUserScript(hideNavBarAndCookieConsentScript);

            webViewConfig.UserContentController = userController;

            webView = new WKWebView(View.Frame, webViewConfig)
            {
                NavigationDelegate = new SettingsWebView.SettingsWebViewNavigationDelegate()
            };

            switch (Type)
            {
                case "TC":
                    NavigationItem.Title = "AGBs";
                    webView.LoadRequest(new NSUrlRequest(new NSUrl($@"{BaseUrlContainer.BaseUri}/Home/AGB?fullscreen=true")));
                    break;
                case "Imprint":
                    NavigationItem.Title = "Impressum";
                    webView.LoadRequest(new NSUrlRequest(new NSUrl($@"{BaseUrlContainer.BaseUri}/Home/Impressum?fullscreen=true")));
                    break;
                case "DataProtection":
                    NavigationItem.Title = "Datenschutzhinweise";
                    webView.LoadRequest(new NSUrlRequest(new NSUrl($@"{BaseUrlContainer.BaseUri}/Home/Datenschutzhinweise?fullscreen=true")));
                    break;
            }
            View.AddSubview(webView);

        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            webView.NavigationDelegate = null;
            webView = null;
        }

        public void CleanUp()
        {
            webView.Dispose();
        }
    }
}