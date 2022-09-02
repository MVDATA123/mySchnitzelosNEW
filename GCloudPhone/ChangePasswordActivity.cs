using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using GCloudShared.Extensions;
using GCloudShared.Service;
using GCloudShared.Service.Dto;
using GCloudShared.Shared;
using Refit;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace mvdata.foodjet
{
    [Activity(Label = "Passwort Ändern", Name = "mvdata.foodjet.ChangePasswordActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ChangePasswordActivity : BaseActivity
    {
        private Toolbar _toolbar;
        private EditText _txtOldPassword, _txtNewPassword, _txtNewPasswordRepeat;
        private Button _btnSubmit;

        private IAuthService _authService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.ChangePassword);

            _toolbar = FindViewById<Toolbar>(Resource.Id.app_bar);
            _txtOldPassword = FindViewById<EditText>(Resource.Id.txtChangePasswordOldPassword);
            _txtNewPassword = FindViewById<EditText>(Resource.Id.txtChangePasswordNewPassword);
            _txtNewPasswordRepeat = FindViewById<EditText>(Resource.Id.txtChangePasswordNewPasswordRepeat);
            _btnSubmit = FindViewById<Button>(Resource.Id.btnChangePasswordSubmit);
            _authService = RestService.For<IAuthService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);

            _btnSubmit.Click += ChangePassword;
        }

        private async void ChangePassword(object sender, EventArgs e)
        {
            if (IsFormValid())
            {
                try
                {
                    await _authService.ChangePassword(new ChangePasswordRequestModel()
                    {
                        OldPassword = _txtOldPassword.Text,
                        NewPassword = _txtNewPassword.Text,
                        ConfirmPassword = _txtNewPasswordRepeat.Text
                    });
                    Toast.MakeText(this, GetString(Resource.String.settingsChangePasswordSuccessMessage), ToastLength.Long).Show();
                    Finish();
                }
                catch (ApiException apiException)
                {
                    var errorModel = apiException.GetApiErrorResult();
                    errorModel.Match(some =>
                    {
                        Snackbar.Make(FindViewById(Resource.Id.changePwRoot), some.Message, Snackbar.LengthLong).Show();
                    }, () => Snackbar.Make(FindViewById(Resource.Id.changePwRoot), Resource.String.generalErrorMessage, Snackbar.LengthLong).Show());
                }
            }
        }

        private bool IsFormValid()
        {
            var isValid = true;

            isValid = _txtOldPassword.HasContent() && _txtNewPassword.HasContent() && _txtNewPasswordRepeat.HasContent()
                      && _txtNewPassword.ContentIsEqualTo(_txtNewPasswordRepeat)
                      && _txtNewPassword.ContentMinLength(6) && _txtNewPasswordRepeat.ContentMinLength(6);

            return isValid;
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetSupportActionBar(_toolbar);
            SupportActionBar.SetTitle(Resource.String.ChangePasswordActivityHeader);
        }
    }
}