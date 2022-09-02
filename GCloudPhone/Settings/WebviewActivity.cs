using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using GCloud.Shared;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Java.Net;
using Refit;
using CookieManager = Android.Webkit.CookieManager;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace mvdata.foodjet.Settings
{
    [Activity(Label = "AgbActivity", Name = "mvdata.foodjet.Settings.WebviewActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class WebviewActivity : BaseActivity
    {
        private Toolbar _toolbar;
        private int _titleTextResId;
        private ProgressBar _progressBar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            var cookieString = GetSharedPreferences(GetString(Resource.String.sharedPreferencesKey), FileCreationMode.Private).GetString(GetString(Resource.String.sharedPreferencesAuthToken), null);

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SettingsWebview);
            var webView = FindViewById<WebView>(Resource.Id.agb_webview);
            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBarSettingsWebView);
            
            webView.SetWebChromeClient(new MyWebChromeClient(this));
            webView.SetWebViewClient(new WebViewClient());
            webView.ClearCache(true);
            webView.Settings.SetAppCacheEnabled(false);
            webView.Settings.CacheMode = CacheModes.NoCache;
            webView.Settings.JavaScriptEnabled = true;
            
            var cookie = new HttpCookie(".AspNet.ApplicationCookie", cookieString)
            {
                Domain = BaseUrlContainer.BaseUrlHost,
                Path = "/"
            };
            CookieManager.Instance.SetAcceptCookie(true);
            CookieManager.Instance.SetAcceptThirdPartyCookies(webView, true);
            CookieManager.Instance.SetCookie(cookie.Domain, $".AspNet.ApplicationCookie={cookieString}; path=/; Domain={cookie.Domain};");
            CookieManager.Instance.Flush();

            //Workaround for being the DataString Readonly
            var data = Intent.DataString ?? Intent.GetStringExtra("DataString");

            switch (data)
            {
                case "AGB":
                    webView.LoadUrl(BaseUrlContainer.BaseUri + "Home/AGB?fullscreen=true");
                    _titleTextResId = Resource.String.agbs;
                    break;
                case "Impressum":
                    webView.LoadUrl(BaseUrlContainer.BaseUri + "Home/Impressum?fullscreen=true");
                    _titleTextResId = Resource.String.impressum;
                    break;
                case "Datenschutzhinweise":
                    webView.LoadUrl(BaseUrlContainer.BaseUri + "Home/Datenschutzhinweise?fullscreen=true");
                    _titleTextResId = Resource.String.datenschutzhinweise;
                    break;
                case "ManagerReports":
                    var dateFromString = Intent.GetStringExtra("dateFrom");
                    var dateToString = Intent.GetStringExtra("dateTo");
                    var storeGuidString = Intent.GetStringExtra("storeGuid");
                    var homeUrlPart = Intent.GetStringExtra("HomeUrlPart");
                    if (DateTime.TryParse(dateFromString, out var dateFrom) && DateTime.TryParse(dateToString, out var dateTo) && Guid.TryParse(storeGuidString, out var storeGuid))
                    {
                        var parameters = new Dictionary<string, string>{{"fullscreen", "true"}};
                        if (storeGuid != Guid.Empty)
                        {
                            parameters.Add("storeGuid",storeGuid.ToString());
                        }
                        parameters.Add("dateFrom", dateFrom.ToString("O"));
                        parameters.Add("dateTo", dateTo.ToString("O"));

                        var urlBuilder = new StringBuilder($"{BaseUrlContainer.BaseUri}Reports/{homeUrlPart}?");

                        urlBuilder.Append(string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}")));
                        webView.LoadUrl(urlBuilder.ToString());
                        _titleTextResId = Resource.String.reportActivityHeader;
                    }
                    break;
            }

            _toolbar = FindViewById<Toolbar>(Resource.Id.app_bar);
        }

        protected override void OnResume()
        {
            SetSupportActionBar(_toolbar);
            SupportActionBar.SetTitle(_titleTextResId);
            base.OnResume();
        }

        private class MyWebChromeClient : WebChromeClient
        {
            private readonly WebviewActivity _activity;

            public MyWebChromeClient(WebviewActivity activity)
            {
                _activity = activity;
            }

            public override void OnProgressChanged(WebView view, int newProgress)
            {
                _activity.RunOnUiThread(() => _activity._progressBar.Progress = newProgress);

                if (newProgress == 100)
                {
                    _activity.RunOnUiThread(() => _activity._progressBar.Visibility = ViewStates.Gone);
                }
            }


        }
    }
}