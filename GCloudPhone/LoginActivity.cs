using System;
using System.Linq;
using System.Net;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Locations;
using Android.Nfc;
using Android.Widget;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Firebase.Iid;
using GCloud.Shared.Exceptions;
using mvdata.foodjet.Caching;
using mvdata.foodjet.Domain;
using GCloudShared;
using GCloudShared.Domain;
using GCloudShared.Extensions;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Newtonsoft.Json;
using Optional;
using Optional.Unsafe;
using Refit;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace mvdata.foodjet
{
    [Activity(Label = "FoodJet", MainLauncher = false, ScreenOrientation = ScreenOrientation.Portrait)]
    public class LoginActivity : AppCompatActivity
    {
        public static readonly int InstallGooglePlayServicesId = 1000;
        private bool _isGooglePlayServicesInstalled;
        public static readonly string Tag = "GCloud";

        private View _mainRoot;
        private EditText _txtUsername, _txtPassword;
        private Button _buttonLogin;
        private TextView _txtRegister, _txtContinueWithoutLogin;
        private Toolbar _toolbar;
        private ProgressBar _progressBar;


        private IStartupService _startupService;
        private IAuthService _authService;
        private MobilePhoneRepository _mobilePhoneRepository;

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            switch (resultCode)
            {
                case Result.Ok:
                    // Try again.
                    _isGooglePlayServicesInstalled = true;
                    break;

                default:
                    Log.Debug("MainActivity", "Unknown resultCode {0} for request {1}", resultCode, requestCode);
                    break;
            }
        }


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Toast.MakeText(this, (args.ExceptionObject as Exception)?.Message, ToastLength.Long).Show();
            };

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _mobilePhoneRepository = new MobilePhoneRepository(DbBootstraper.Connection);

            _txtUsername = FindViewById<EditText>(Resource.Id.txtLoginUsernameInputText);
            _txtPassword = FindViewById<EditText>(Resource.Id.txtLoginPasswordInputText);
            _buttonLogin = FindViewById<Button>(Resource.Id.ButtonLogin);
            _txtRegister = FindViewById<TextView>(Resource.Id.ButtonMainRegister);
            _txtContinueWithoutLogin = FindViewById<TextView>(Resource.Id.btnMainContinueWithoutAccount);
            _toolbar = FindViewById<Toolbar>(Resource.Id.app_bar);
            _mainRoot = FindViewById<LinearLayout>(Resource.Id.mainRoot);
            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBarLoginScreen);

            _startupService = RestService.For<IStartupService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
            _authService = RestService.For<IAuthService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);

            _buttonLogin.Click += PerformLogin;
            _txtRegister.Click += NavigateToRegisterActivity;
            _txtContinueWithoutLogin.Click += TxtContinueWithoutLoginOnClick;
            _isGooglePlayServicesInstalled = TestIfGooglePlayServicesIsInstalled();

#if DEBUG
            _txtUsername.Text = "Manager";
            _txtPassword.Text = "Kassa1234!";
#endif
        }

        private void TxtContinueWithoutLoginOnClick(object sender, EventArgs e)
        {
            var editor = GetSharedPreferences(GetString(Resource.String.sharedPreferencesKey), FileCreationMode.Private).Edit();
            editor.PutInt(GetString(Resource.String.sharedPreferencesLoginMethod), (int)UserLoginMethod.Anonymous);
            editor.Commit();
            StartActivity(typeof(StartupActivity));
        }

        private void NavigateToRegisterActivity(object sender, EventArgs eventArgs)
        {
            var intent = new Intent(this, typeof(RegisterActivity));
            StartActivity(intent);
        }

        protected override void OnResume()
        {
            SetSupportActionBar(_toolbar);
            SupportActionBar.SetTitle(Resource.String.MainActivityHeader);
            base.OnResume();
        }

        private void PerformLogin(object sender, EventArgs args)
        {
            var mobilePhone = _mobilePhoneRepository.FirstOrDefault();

            if (!ValidateLogin())
            {
                return;
            }

            _progressBar.Visibility = ViewStates.Visible;

            _authService.Login(new LoginRequestModel
            {
                Username = _txtUsername.Text,
                Password = _txtPassword.Text,
                DeviceId = mobilePhone?.MobilePhoneId,
                FirebaseInstanceId = FirebaseInstanceId.Instance.Token
            }).Subscribe(response =>
                {
                    if (response != null)
                    {
                        var cookies = HttpClientContainer.Instance.CookieContainer.GetCookies(UriContainer.BasePath).Cast<Cookie>().ToList();
                        var settings = GetSharedPreferences(GetString(Resource.String.sharedPreferencesKey), FileCreationMode.Private);
                        var editor = settings.Edit();
                        editor.PutBoolean(GetString(Resource.String.sharedPreferencesIsManager), response.Role.Equals("Managers"));
                        editor.PutString(GetString(Resource.String.sharedPreferencesUsername), response.Username);
                        editor.PutString(GetString(Resource.String.sharedPreferencesEmail), response.Email);
                        editor.PutString(GetString(Resource.String.sharedPreferencesAuthToken), cookies.FirstOrDefault(x => x.Name == ".AspNet.ApplicationCookie")?.Value);
                        editor.PutString(GetString(Resource.String.sharedPreferencesUserId), response.UserId);
                        editor.PutInt(GetString(Resource.String.sharedPreferencesLoginMethod), (int)UserLoginMethod.Normal);
                        editor.Commit();
                        _mobilePhoneRepository.Insert(new MobilePhone
                        {
                            MobilePhoneId = response.MobilePhoneGuid.ToString()
                        });
                        Finish();
                        StartActivity(typeof(StartupActivity));
                    }
                },
                ex =>
                {
                    if (ex is ApiException apiException)
                    {
                        var result = apiException.GetApiErrorResult();
                        RunOnUiThread(() =>
                        {
                            result.Match(some =>
                            {
                                if (some.ErrorCode == ExceptionStatusCode.EmailNotConfirmed)
                                {
                                    Snackbar.Make(_mainRoot, some.Message, Snackbar.LengthLong).SetAction(
                                        GetString(Resource.String.loginResendMail),
                                        async view =>
                                        {
                                            try
                                            {
                                                var resendResult =
                                                    await _authService.ResendActivationEmail(_txtUsername.Text);
                                                if (resendResult)
                                                {
                                                    Snackbar.Make(_mainRoot,
                                                        GetString(Resource.String.loginResendMailSent),
                                                        Snackbar.LengthLong).Show();
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Log.Debug("MainActivity",
                                                    "Unknown Error when trying to resend Email: {0}", e.Message);
                                            }
                                        }).SetActionTextColor(Color.Orange).Show();
                                }
                                else
                                {
                                    Snackbar.Make(_mainRoot, some.Message, Snackbar.LengthLong).Show();
                                }
                            }, () =>
                            {
                                Snackbar.Make(_mainRoot, GetString(Resource.String.generalErrorMessage), Snackbar.LengthLong).Show();
                            });
                        });
                    }
                    _progressBar.Visibility = ViewStates.Invisible;
                },
                () =>
                {
                    _progressBar.Visibility = ViewStates.Invisible;
                }
            );

        }

        private bool ValidateLogin()
        {
            var valid = true;

            if (string.IsNullOrEmpty(_txtUsername.Text))
            {
                _txtUsername.Error = GetString(Resource.String.loginEnterValidUsername);
                valid = false;
            }
            else
            {
                _txtUsername.Error = null;
            }

            if (string.IsNullOrEmpty(_txtPassword.Text))
            {
                _txtPassword.Error = GetString(Resource.String.loginEnterValidPassword);
                valid = false;
            }
            else
            {
                _txtPassword.Error = null;
            }

            return valid;
        }

        private bool TestIfGooglePlayServicesIsInstalled()
        {
            int queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (queryResult == ConnectionResult.Success)
            {
                Log.Info(Tag, "Google Play Services is installed on this device.");
                return true;
            }

            if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
            {
                string errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
                Log.Error(Tag, "There is a problem with Google Play Services on this device: {0} - {1}", queryResult, errorString);
                Dialog errorDialog = GoogleApiAvailability.Instance.GetErrorDialog(this, queryResult, InstallGooglePlayServicesId);
                ErrorDialogFragment dialogFrag = new ErrorDialogFragment();

                dialogFrag.Show(FragmentManager, "GooglePlayServicesDialog");
            }
            return false;
        }
    }
}

