using Foundation;
using GCloudiPhone.Extensions;
using System;
using UIKit;
using WebKit;

namespace GCloudiPhone
{
    public partial class WebShopViewController : UIViewController
    {
        //ovde treba ucitati username i password sa login stranice

        private readonly NSUrl url1 = new NSUrl("https://esterhazystrasse.myschnitzel.at/");
        private readonly NSUrl url2 = new NSUrl("https://fischauergasse.myschnitzel.at/");
        private readonly NSUrl url3 = new NSUrl("https://grazerstrasse.myschnitzel.at/");
        private readonly NSUrl url4 = new NSUrl("https://neunkirchen.myschnitzel.at/");

        public StoreLocationDto Store { get; set; }

        public WebShopViewController (IntPtr handle) : base (handle)
        {
        }


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var storeName = Store.Name;
            var webView = new WKWebView(View.Frame, new WKWebViewConfiguration());
            View.AddSubview(webView);

            if (storeName == "Eisenstadt")
            {
                webView.LoadRequest(new NSUrlRequest(url1));
            }
            else if (storeName == "Fischauer Gasse")
            {
                webView.LoadRequest(new NSUrlRequest(url2));
            }
            else if (storeName == "Grazer Straße")
            {
                webView.LoadRequest(new NSUrlRequest(url3));
            }
            else
            {
                webView.LoadRequest(new NSUrlRequest(url4));
            }
        }
    }
}