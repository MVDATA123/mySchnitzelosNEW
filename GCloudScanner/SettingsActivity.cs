using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using GCloudScanner.Service;
using Refit;

namespace GCloudScanner
{
    [Activity(Label = "Einstellungen")]
    public class SettingsActivity : AppCompatActivity
    {
        private EditText _txtUsername, _txtPassword;
        private Button _btn;
        private ISettingsService _settingsService;

        public static readonly string BaseUrl = GCloud.Shared.BaseUrlContainer.BaseUri.ToString();
        public const string StoreGuidPrefKey = "StoreGuid";
        public const string ApiTokenPrefKey = "ApiToken";
        public const string CashRegisterIdPrefKey = "CashRegisterId";
        public const string PrefKey = "FoodjetSettings";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.settings);

            _btn = FindViewById<Button>(Resource.Id.btnCheckSettings);
            _txtUsername = FindViewById<EditText>(Resource.Id.txtSettingsUsername);
            _txtPassword = FindViewById<EditText>(Resource.Id.txtSettingsPassword);
            _settingsService = RestService.For<ISettingsService>(BaseUrl);

            _btn.Click += ApplySettings;
        }

        private async void ApplySettings(object sender, EventArgs e)
        {
            var progressDialog = new ProgressDialog(this)
            {
                Indeterminate = true
            };
            progressDialog.SetMessage("Wird eingerichtet.");
            progressDialog.SetCancelable(false);
            progressDialog.SetCanceledOnTouchOutside(false);

            var isValid = true;
            var username = _txtUsername.Text;
            var password = _txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username))
            {
                _txtUsername.Error = "Darf nicht leer sein";
                isValid = false;
            }
            else
            {
                _txtUsername.Error = null;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                _txtPassword.Error = "Darf nicht leer sein";
                isValid = false;

            }
            else
            {
                _txtPassword.Error = null;
            }

            if (!isValid)
            {
                return;
            }

            progressDialog.Show();

            try
            {
                var availableStores = await _settingsService.GetAvailableStores(username, password);
                if (availableStores.Any())
                {
                    var storeId = availableStores.First().StoreId;
                    var preferences = GetSharedPreferences(PrefKey, FileCreationMode.Private);
                    var editor = preferences.Edit();
                    editor.PutString(StoreGuidPrefKey, storeId.ToString());
                    editor.Commit();

                    var macAddress = "02:00:00:00:00:00";

                    var apiToken = await _settingsService.GetStoreApiToken(username, password, storeId, macAddress,
                        "Android Device");
                    if (apiToken != null)
                    {
                        editor.PutString(CashRegisterIdPrefKey, apiToken.CashRegisterId.ToString());
                        editor.PutString(StoreGuidPrefKey, storeId.ToString());
                        editor.PutString(ApiTokenPrefKey, apiToken.ApiToken);
                        editor.Commit();
                        SetResult(Result.Ok);
                        Finish();
                    }
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
            finally
            {
                progressDialog.Dismiss();
            }
        }
    }
}