using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using Android.Widget;
using GCloudShared.Domain;
using GCloudShared.Repository;
using GCloudShared.Shared;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace mvdata.foodjet.Settings
{
    [Activity(Label = "Einstellungen", ScreenOrientation = ScreenOrientation.Portrait)]
    public class SettingsActivity : BaseActivity
    {
        private Toolbar _toolbar;
        private SettingsPages _page;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Settings);

            _page = (SettingsPages)Enum.Parse(typeof(SettingsPages), Intent.GetStringExtra("Page"));

            FragmentManager.BeginTransaction()
                .Add(Resource.Id.settings_fragment, new SettingsFragment(_page)).Commit();

            //_btnChangePassword = FindViewById<Button>(Resource.Id.btnSettingsChangePasswordButton);
            _toolbar = FindViewById<Toolbar>(Resource.Id.app_bar);
            //_btnChangePassword.Click += (sender, args) => { StartActivity(typeof(ChangePasswordActivity)); };
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetSupportActionBar(_toolbar);
            SupportActionBar.SetTitle(Resource.String.SettingsHeader);
        }

        class SettingsFragment : PreferenceFragment
        {
            private SettingsPages _page;
            public SettingsFragment(SettingsPages page)
            {
                _page = page;
            }
            public override void OnCreate(Bundle savedInstanceState)
            {
                var userLoginMethod = (UserLoginMethod)Activity.GetSharedPreferences(GetString(Resource.String.sharedPreferencesKey), FileCreationMode.Private).GetInt(GetString(Resource.String.sharedPreferencesLoginMethod), -1);

                base.OnCreate(savedInstanceState);
                if (userLoginMethod == UserLoginMethod.Normal)
                {
                    if (_page == SettingsPages.Settings)
                    {
                        AddPreferencesFromResource(Resource.Xml.preferenceSettings);
                    }
                    else if (_page == SettingsPages.AboutMe)
                    {
                        AddPreferencesFromResource(Resource.Xml.preferencesAboutMe);
                    }
                }
                else if (userLoginMethod == UserLoginMethod.Anonymous)
                {
                    AddPreferencesFromResource(Resource.Xml.preferencesAnonymous);
                }
            }
        }
    }

    public enum SettingsPages
    {
        Settings, AboutMe
    }
}