using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using GCloud.Shared.Exceptions;
using GCloudShared.Extensions;
using GCloudShared.Service;
using GCloudShared.Service.Dto;
using GCloudShared.Shared;
using mvdata.foodjet.Control;
using Newtonsoft.Json;
using Refit;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace mvdata.foodjet
{
    [Activity(Label = "RegisterActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class RegisterActivity : BaseActivity
    {
        private LinearLayout _mainRoot;

        private EditText _txtUsername,
            _txtPassword,
            _passwordRepeatEditText,
            _txtEmail,
            _txtBirthday,
            _txtFirstName,
            _txtLastName;

        private TextView _txtRegisterAlreadyMember;
        private Button _btnRegister;
        private Toolbar _toolbar;
        private ProgressBar _progressBar;
        private bool _datePickershown;

        private object _datePickerSync = new object();
        private DateTime? _selectedBirthday = null;

        private IAuthService _authService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Register);
            _mainRoot = FindViewById<LinearLayout>(Resource.Id.mainRootRegister);
            _txtUsername = FindViewById<EditText>(Resource.Id.txtRegisterUsername);
            _txtPassword = FindViewById<EditText>(Resource.Id.txtRegisterPassword);
            _passwordRepeatEditText = FindViewById<EditText>(Resource.Id.txtRegisterPassword);
            _txtRegisterAlreadyMember = FindViewById<TextView>(Resource.Id.txtRegisterAlreadyMember);
            _txtEmail = FindViewById<EditText>(Resource.Id.txtRegisterEmail);
            _toolbar = FindViewById<Toolbar>(Resource.Id.app_bar);
            _btnRegister = FindViewById<Button>(Resource.Id.btnRegisterRegister);
            _txtBirthday = FindViewById<EditText>(Resource.Id.txtRegisterBirthday);
            _txtFirstName = FindViewById<EditText>(Resource.Id.txtRegisterFirstName);
            _txtLastName = FindViewById<EditText>(Resource.Id.txtRegisterLastName);
            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBarRegister);
            _txtBirthday.ShowSoftInputOnFocus = false;

            _authService = RestService.For<IAuthService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);

            _btnRegister.Click += PerformRegister;
            _txtRegisterAlreadyMember.Click += (sender, args) => Finish();
            _txtUsername.FocusChange += TxtUsernameOnFocusChange;
            _txtEmail.FocusChange += TxtEmailOnFocusChange;

            _txtBirthday.FocusChange += delegate(object sender, View.FocusChangeEventArgs args)
            {
                if (args.HasFocus)
                {
                    ShowBirthdayDatePicker();
                }
            };
            _txtBirthday.Touch += delegate { ShowBirthdayDatePicker(); };
            // Create your application here
        }

        private void TxtUsernameOnFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (!e.HasFocus)
            {
                CheckUsername();
            }
        }

        private void TxtEmailOnFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (!e.HasFocus)
            {
                CheckEmail();
            }
        }

        private bool CheckEmail()
        {
            var result = false;
            var email = _txtEmail.Text;
            var errorMessage = "";
            if (string.IsNullOrWhiteSpace(email))
            {
                errorMessage = GetString(Resource.String.registerEmailEmpty);
            }
            else
            {
                bool isAvailable = false;
                try
                {
                    isAvailable = _authService.IsEmailAvailable(email).Result;
                }
                catch (Exception e)
                {
                    return false;
                }

                if (!isAvailable)
                {
                    errorMessage = GetString(Resource.String.registerEmailAlreadyTaken);
                }
                else
                {
                    errorMessage = null;
                    result = true;
                }
            }

            RunOnUiThread(() => _txtEmail.Error = errorMessage);
            return result;
        }

        private bool CheckUsername()
        {
            var result = false;
            var username = _txtUsername.Text;
            var errorMessage = "";
            if (string.IsNullOrWhiteSpace(username))
            {
                errorMessage = GetString(Resource.String.registerUsernameEmpty);
            }
            else
            {
                bool isAvailable = false;
                try
                {
                    isAvailable = _authService.IsUsernameAvailable(username).Result;
                }
                catch (Exception e)
                {
                    return false;
                }

                if (!isAvailable)
                {
                    errorMessage = GetString(Resource.String.registerUsernameAlreadyTaken);
                }
                else
                {
                    errorMessage = null;
                    result = true;
                }
            }

            RunOnUiThread(() => _txtUsername.Error = errorMessage);
            return result;
        }

        private async void PerformRegister(object sender, EventArgs eventArgs)
        {
            if (!ValidateRegister())
            {
                return;
            }

            _progressBar.Visibility = ViewStates.Visible;

            try
            {
                await _authService.RegisterUser(new RegisterRequestModel()
                {
                    ConfirmPassword = _passwordRepeatEditText.Text,
                    Username = _txtUsername.Text,
                    Password = _txtPassword.Text,
                    Email = _txtEmail.Text,
                    Birthday = _selectedBirthday ?? DateTime.Now,
                    FirstName = _txtFirstName.Text,
                    LastName = _txtLastName.Text
                });

                _progressBar.Visibility = ViewStates.Invisible;

                var intent = new Intent(this, typeof(ConfirmEmailSent));
                StartActivity(intent);
                Finish();
            }
            catch (ApiException apiException)
            {
                var errorModel = apiException.GetApiErrorResult();
                errorModel.Match(some =>
                {
                    switch (some.ErrorCode)
                    {
                        case ExceptionStatusCode.UsernameAlreadyTaken:
                            Snackbar.Make(_mainRoot, Resource.String.registerUsernameAlreadyTaken, Snackbar.LengthLong)
                                .Show();
                            break;
                    }
                }, () => { Snackbar.Make(_mainRoot, Resource.String.generalErrorMessage, Snackbar.LengthLong).Show(); });
            }
        }

        private void ShowBirthdayDatePicker()
        {
            if (!_datePickershown)
            {
                _datePickershown = true;
                lock (_datePickerSync)
                {
                    var frag = DatePickerFragment.NewInstance(delegate(DateTime date)
                    {
                        _selectedBirthday = date;
                        _txtBirthday.Text = date.ToString("dd.MM.yyyy");
                        _datePickershown = false;
                    });
                    frag.Dismissed += delegate { _datePickershown = false; };
                    frag.Canceled += delegate { _datePickershown = false; };
                    frag.Show(SupportFragmentManager, DatePickerFragment.TAG);
                }
            }
        }

        private bool ValidateRegister()
        {
            var valid = true;
            var userNameValid = CheckUsername();
            var emailValid = CheckEmail();

            valid = userNameValid && emailValid;

            if (userNameValid && _txtUsername.Text.Length < 4)
            {
                _txtUsername.Error = "Mindestens 4 Zeichen";
                _txtUsername.RequestFocus();
                valid = false;
            }
            else if (userNameValid)
            {
                _txtUsername.Error = null;
            }
            else
            {
                _txtUsername.RequestFocus();
            }

            if (emailValid && !Android.Util.Patterns.EmailAddress.Matcher(_txtEmail.Text).Matches())
            {
                _txtEmail.Error = "Gültige E-Mail Adresse eingeben";
                _txtEmail.RequestFocus();
                valid = false;
            }
            else if (emailValid)
            {
                _txtEmail.Error = null;
            }
            else
            {
                _txtEmail.RequestFocus();
            }

            if (string.IsNullOrEmpty(_txtFirstName.Text))
            {
                _txtFirstName.Error = "Vorname ist leer";
                _txtFirstName.RequestFocus();
                valid = false;
            }
            else
            {
                _txtFirstName.Error = null;
            }

            if (string.IsNullOrEmpty(_txtLastName.Text))
            {
                _txtLastName.Error = "Nachname ist leer";
                _txtLastName.RequestFocus();
                valid = false;
            }
            else
            {
                _txtLastName.Error = null;
            }

            if (string.IsNullOrEmpty(_txtPassword.Text))
            {
                _txtPassword.Error = "Passwort ist leer";
                _txtPassword.RequestFocus();
                valid = false;
            }
            else
            {
                if (_txtPassword.Text.Length < 6)
                {
                    _txtPassword.Error = "Mindestens 6 Zeichen";
                    _txtPassword.RequestFocus();
                    valid = false;
                }
                else
                {
                    _txtPassword.Error = null;
                }
            }

            if (!_selectedBirthday.HasValue || string.IsNullOrEmpty(_txtBirthday.Text))
            {
                _txtBirthday.Error = "Geburtstag erforderlich";
                _txtBirthday.RequestFocus();
                valid = false;
            }
            else
            {
                _txtBirthday.Error = null;
            }

            return valid;
        }

        protected override void OnResume()
        {
            SetSupportActionBar(_toolbar);
            SupportActionBar.SetTitle(Resource.String.registerActivityHeader);
            base.OnResume();
        }

        private void NavigateToLoginActivity()
        {
            var intent = new Intent(this, typeof(LoginActivity));
            StartActivity(intent);
        }
    }
}