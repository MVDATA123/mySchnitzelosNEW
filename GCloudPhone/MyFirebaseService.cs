using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Firebase.Iid;

namespace mvdata.foodjet
{
    [Service]
    [IntentFilter(new [] {"com.google.firebase.INSTANCE_ID_EVENT"})]
    public class MyFirebaseService : FirebaseInstanceIdService
    {
        const string TAG = "MyFirebaseIIDService";

        public override void OnTokenRefresh()
        {
            var refreshToken = FirebaseInstanceId.Instance.Token;
            Log.Debug(TAG, "Refreshed Token: " + refreshToken);
            SendRegistrationToServer(refreshToken);
        }

        void SendRegistrationToServer(string token)
        {

        }

        
    }
}