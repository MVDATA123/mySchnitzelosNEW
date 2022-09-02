using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using GCloudShared.Domain;
using GCloudShared.Extensions;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using mvdata.foodjet.Caching;
using mvdata.foodjet.Domain;
using mvdata.foodjet.Service;
using mvdata.foodjet.Settings;
using Newtonsoft.Json;
using Optional;
using Refit;

namespace mvdata.foodjet
{
    [Activity(Label = "FoodJet", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class StartupActivity : AppCompatActivity
    {
        private IStartupService _startupService;

        protected UserLoginMethod UserLoginMethod
        {
            get
            {
                var loginMethod = GetSharedPreferences(GetString(Resource.String.sharedPreferencesKey), FileCreationMode.Private).GetInt(GetString(Resource.String.sharedPreferencesLoginMethod), -1);
                return (GCloudShared.Domain.UserLoginMethod)loginMethod;
            }
        }

        public const string PrefKey = "FoodJetSettings";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);  

            // Create your application here
            SetContentView(Resource.Layout.LoadingScreen);
            _startupService = RestService.For<IStartupService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);

            new Thread(async () =>
            {
                var settings = GetSharedPreferences(GetString(Resource.String.sharedPreferencesKey), FileCreationMode.Private);

                //Es ist bereits ein benutzer auf dem Gerät angemeldet weil die Einstellung gesetzt ist.
                if (settings.Contains(GetString(Resource.String.sharedPreferencesLoginMethod)) || settings.Contains(GetString(Resource.String.sharedPreferencesUserId)))
                {
                    if (UserLoginMethod == UserLoginMethod.Normal)
                    {
                        HttpClientContainer.Instance.CookieContainer.Add(UriContainer.BasePath, new Cookie(".AspNet.ApplicationCookie", settings.GetString(GetString(Resource.String.sharedPreferencesAuthToken), string.Empty)));
                    }
                    await CachingService.GetAllData(this, true);
                    StartActivity(typeof(Dashboard));
                }
                else
                {
                    StartActivity(typeof(LoginActivity));
                }

                Finish();
            }).Start();
        }

        //protected override void OnResume()
        //{
        //    base.OnResume();
        //    CachingService.GetAllData(this, true).ContinueWith(result =>
        //    {
        //        var settings = GetSharedPreferences(GetString(Resource.String.sharedPreferencesKey), FileCreationMode.Private);

        //        //Es ist bereits ein benutzer auf dem Gerät angemeldet weil die Einstellung gesetzt ist.
        //        if (UserLoginMethod == UserLoginMethod.Anonymous || settings.Contains(GetString(Resource.String.sharedPreferencesUserId)))
        //        {
        //            if (UserLoginMethod == UserLoginMethod.Normal)
        //            {
        //                HttpClientContainer.Instance.CookieContainer.Add(UriContainer.BasePath, new Cookie(".AspNet.ApplicationCookie", settings.GetString(GetString(Resource.String.sharedPreferencesAuthToken), string.Empty)));
        //            }
        //            StartActivity(typeof(Dashboard));
        //        }
        //        else
        //        {
        //            StartActivity(typeof(LoginActivity));
        //        }

        //        Finish();
        //    }, TaskContinuationOptions.OnlyOnRanToCompletion);
        //}

        public override void OnBackPressed()
        {
        }
    }
}