using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace mvdata.foodjet
{
    [Activity(Label = "ConfirmEmailSend", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ConfirmEmailSent : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ConfirmEmailSent);
            // Create your application here

            var button = FindViewById<Button>(Resource.Id.backToLogin);
            button.Click += delegate(object sender, EventArgs args)
            {
                Finish();
            };
        }
    }
}