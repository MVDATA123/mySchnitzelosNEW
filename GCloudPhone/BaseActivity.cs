using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Widget;
using GCloudShared.Domain;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Service.Dto;
using GCloudShared.Shared;
using mvdata.foodjet.Receiver;
using mvdata.foodjet.Service;
using Org.Apache.Http.Authentication;
using Refit;
using AlertDialog = Android.App.AlertDialog;
using AuthState = GCloudShared.Shared.AuthState;
using Environment = System.Environment;

namespace mvdata.foodjet
{
    public abstract class BaseActivity : AppCompatActivity, IInternetState
    {
        public const string KundenkarteFileName = "kundenkarte.jpg";
        

        public NetworkState State { get; set; }
        public AuthState AuthState { get; set; }
        protected UserRepository UserRepository;
        protected IAuthService AuthService;

        public UserLoginMethod UserLoginMethod
        {
            get
            {
                var loginMethod = GetSharedPreferences(GetString(Resource.String.sharedPreferencesKey),FileCreationMode.Private).GetInt(GetString(Resource.String.sharedPreferencesLoginMethod), -1);
                return (GCloudShared.Domain.UserLoginMethod) loginMethod;
            }
        }

        public string KundenkartePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), KundenkarteFileName);

        public void ShowNoInternetMessage()
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle(Resource.String.noInternetTitle);
            alert.SetMessage(Resource.String.noInternetDescription);
            alert.SetPositiveButton(Resource.String.noInternetButtonText, (senderAlert, args) =>
            {
                OnShowNoInternetMessageSuccess();
            });
            Dialog dialog = alert.Create();
            dialog.Show();
        }

        public virtual void OnShowNoInternetMessageSuccess()
        {
        }

        private NetworkStatusBroadcastReceiver _broadcastReceiver;
        private Snackbar _offlineSnackbar;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            UserRepository = new UserRepository(DbBootstraper.Connection);
            AuthService = RestService.For<IAuthService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
            UpdateNetworkStatus();
            HttpClientContainer.Instance.SetInternetState(this);
            await CachingService.GetAllData(this);
        }

        protected override void OnResume()
        {
            base.OnResume();
            RegisterReceiver();
            HttpClientContainer.Instance.SetInternetState(this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            UnregisterReceiver();
            HttpClientContainer.Instance.SetInternetState(null);
        }
        public void UpdateNetworkStatus()
        {
            State = NetworkState.Unknown;

            // Retrieve the connectivity manager service
            var connectivityManager = (ConnectivityManager)
                Application.Context.GetSystemService(
                    Context.ConnectivityService);

            // Check if the network is connected or connecting.
            // This means that it will be available, 
            // or become available in a few seconds.
            var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;

            if (activeNetworkInfo != null && activeNetworkInfo.IsConnectedOrConnecting)
            {
                // Now that we know it's connected, determine if we're on WiFi or something else.
                State = activeNetworkInfo.Type == ConnectivityType.Wifi ?
                    NetworkState.ConnectedWifi : NetworkState.ConnectedData;
            }
            else
            {
                State = NetworkState.Disconnected;
            }
        }
        public void RegisterReceiver()
        {
            if (_broadcastReceiver != null)
            {
                throw new InvalidOperationException(
                    "Network status monitoring already active.");
            }

            // Create the broadcast receiver and bind the event handler
            // so that the app gets updates of the network connectivity status
            _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;

            // Register the broadcast receiver
            Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction));
        }
        public void UnregisterReceiver()
        {
            if (_broadcastReceiver == null)
            {
                throw new InvalidOperationException(
                    "Network status monitoring not active.");
            }

            // Unregister the receiver so we no longer get updates.
            Application.Context.UnregisterReceiver(_broadcastReceiver);

            // Set the variable to nil, so that we know the receiver is no longer used.
            _broadcastReceiver.ConnectionStatusChanged -= OnNetworkStatusChanged;
            _broadcastReceiver = null;
        }

        public virtual void OnNetworkStatusChanged(object sender, EventArgs e)
        {
            UpdateNetworkStatus();
            if (this.State == NetworkState.Disconnected)
            {
                _offlineSnackbar = Snackbar.Make(Window.DecorView.FindViewById(Android.Resource.Id.Content), "Sie sind Offline", Snackbar.LengthIndefinite);
                _offlineSnackbar.Show();
            }
            else
            {
                _offlineSnackbar?.Dismiss();
            }
        }

        public async void LogoffRedirectToLogin()
        {
            var mobilePhoneRepository = new MobilePhoneRepository(DbBootstraper.Connection);

            var logoutTask = AuthService.Logout(mobilePhoneRepository.FirstOrDefault()?.MobilePhoneId);
            if (File.Exists(KundenkartePath))
            {
                File.Delete(KundenkartePath);
            }

            try
            {
                await logoutTask;
            }
            catch (ApiException e)
            {
                if (e.StatusCode == 0)
                {
                    return;
                }
            }
            finally
            {
                UserRepository.DeleteAll();
                mobilePhoneRepository.DeleteAll();
                StartActivity(typeof(LoginActivity));
                Finish();
            }
        }
    }
}