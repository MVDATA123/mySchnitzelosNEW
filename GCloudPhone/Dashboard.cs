using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.Constraints;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Com.Bumptech.Glide;
using GCloudShared.Domain;
using mvdata.foodjet.Receiver;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Service.Dto;
using GCloudShared.Shared;
using mvdata.foodjet.Caching;
using mvdata.foodjet.Domain;
using mvdata.foodjet.Settings;
using mvdata.foodjet.Utils;
using Newtonsoft.Json;
using Optional;
using Refit;
using ZXing.Mobile;
using Environment = System.Environment;
using Path = System.IO.Path;
using Timer = System.Threading.Timer;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace mvdata.foodjet
{

    [Activity(Label = "Meine Kundenkarte", Name = "mvdata.foodjet.Dashboard", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Dashboard : BaseActivity
    {
        private IUserStoreService _userStoresService;
        private MobilePhoneRepository _mobilePhoneRepository;

        private TextView _txtDashboardNavHeaderUsername, _txtDashboardNavHeaderEmail;
        private ImageView _imageViewQrCode;

        private ImageView _imgDashboardMap;
        private System.Timers.Timer _timer;

        private Toolbar _toolbar;
        private GoogleMap _map;

        private ConstraintLayout _dashboardButton1;
        private ConstraintLayout _dashboardButton2;
        private ConstraintLayout _dashboardButton3;
        private ConstraintLayout _dashboardButton4;
        private DrawerLayout _drawerLayout;
        private NavigationView _navigationView;
        private ImageView _imgMap;

        private DateTime _backPressed;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Dashboard);

            _userStoresService = RestService.For<IUserStoreService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
            _mobilePhoneRepository = new MobilePhoneRepository(DbBootstraper.Connection);

            _toolbar = FindViewById<Toolbar>(Resource.Id.app_bar);
            _imageViewQrCode = FindViewById<ImageView>(Resource.Id.imageViewDashboardQrCode);

            _dashboardButton1 = FindViewById<ConstraintLayout>(Resource.Id.dashboardButton1);
            _dashboardButton2 = FindViewById<ConstraintLayout>(Resource.Id.dashboardButton2);
            _dashboardButton3 = FindViewById<ConstraintLayout>(Resource.Id.dashboardButton3);
            _dashboardButton4 = FindViewById<ConstraintLayout>(Resource.Id.dashboardButton4);
            _drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawerLayoutDashboard);
            _navigationView = FindViewById<NavigationView>(Resource.Id.navViewDashboard);
            _imgMap = FindViewById<ImageView>(Resource.Id.imgDashboardMap);
            _imgDashboardMap = FindViewById<ImageView>(Resource.Id.imgDashboardMap);

            _navigationView.InflateMenu(Resource.Menu.DashboardNavMenu);
            _navigationView.InflateHeaderView(Resource.Layout.DashboardNavHeader);
            _navigationView.NavigationItemSelected += (sender, args) =>
            {
                OnOptionsItemSelected(args.MenuItem);
                args.MenuItem.SetChecked(true);
                _drawerLayout.CloseDrawers();
            };

            var preferences = GetSharedPreferences(GetString(Resource.String.sharedPreferencesKey), FileCreationMode.Private);

            _txtDashboardNavHeaderUsername = _navigationView.GetHeaderView(0).FindViewById<TextView>(Resource.Id.txtDashboardNavHeaderUsername);
            _txtDashboardNavHeaderUsername.Text = preferences.GetString(GetString(Resource.String.sharedPreferencesUsername), string.Empty);
            _txtDashboardNavHeaderEmail = _navigationView.GetHeaderView(0).FindViewById<TextView>(Resource.Id.txtDashboardNavHeaderEmail);
            _txtDashboardNavHeaderEmail.Text = preferences.GetString(GetString(Resource.String.sharedPreferencesEmail), string.Empty);

            if (Intent.DataString == "Logout")
            {
                PerformLogout();
                return;
            }

            if (UserLoginMethod == UserLoginMethod.Normal)
            {
                var userId = preferences.GetString(GetString(Resource.String.sharedPreferencesUserId), null);
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    var bmp = QrCodeUtils.GetQrCode(userId, 300, 300);
                    if (bmp != null)
                    {
                        _imageViewQrCode.SetImageBitmap(bmp);
                    }
                }
                else
                {
                    PerformLogout();
                }
            }
            else if (UserLoginMethod == UserLoginMethod.Anonymous)
            {
                Glide.With(this).AsDrawable().Load(Resource.Drawable.icon);
            }
            _dashboardButton1.Click += delegate
            {
                ShowStores();
            };
            _dashboardButton2.Click += delegate
            {
                ShowMap();
            };
            _dashboardButton3.Click += delegate
            {
                if (UserLoginMethod == UserLoginMethod.Normal)
                {
                    var intent = new Intent(this, typeof(SettingsActivity));
                    intent.PutExtra("Page", SettingsPages.Settings.ToString());
                    StartActivity(intent);
                }
                else
                {
                    Snackbar.Make(Window.DecorView.FindViewById(Android.Resource.Id.Content), GetString(Resource.String.functionNotAvailableAsAnonymous), Snackbar.LengthLong).Show();
                }
            };
            _dashboardButton4.Click += delegate
            {
                var intent = new Intent(this, typeof(SettingsActivity));
                intent.PutExtra("Page", SettingsPages.AboutMe.ToString());
                StartActivity(intent);
            };
        }

        protected override void OnResume()
        {
            SetSupportActionBar(_toolbar);
            SupportActionBar.SetTitle(Resource.String.dashboardActivityHeader);
            var settings = GetSharedPreferences(GetString(Resource.String.sharedPreferencesKey), FileCreationMode.Private);
            if (settings.GetBoolean(GetString(Resource.String.sharedPreferencesIsManager), false))
            {
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.baseline_menu_white_24);
            }
            else
            {
                _drawerLayout.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
            }

            if (_timer == null)
            {
#if DEBUG
                _timer = new System.Timers.Timer(TimeSpan.FromSeconds(5).TotalMilliseconds);
#else
            _timer = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
#endif
                _imgDashboardMap.SetImageBitmap(CachingHolder.Instance.NextImage(this));

                _timer.Elapsed += delegate (object sender, ElapsedEventArgs args)
                {
                    _imgDashboardMap.SetImageBitmap(CachingHolder.Instance.NextImage(this));
                };
            }

            _timer.Start();
            base.OnResume();
        }

        protected override void OnPause()
        {
            _timer.Stop();
            base.OnPause();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                _drawerLayout.OpenDrawer(GravityCompat.Start);
            }
            else if (item.ItemId == Resource.Id.DashboardMenuLogout)
            {
                PerformLogout();
            }
            //else if (item.ItemId == Resource.Id.DashbardMenuCoupons)
            //{
            //    //Not finished yet
            //    //StartActivity(typeof(ManagerCouponsList));
            //}
            else if (item.ItemId == Resource.Id.DashbardMenuStores)
            {
                StartActivity(typeof(ManagerStoresList));
            }
            else if (item.ItemId == Resource.Id.DashbardMenuReports)
            {
                StartActivity(typeof(ManagerReportSelectionActivity));
            }
            else if (item.ItemId == Resource.Id.DashboardMenuEbill)
            {
                StartActivity(typeof(BillsListActivity));
            }
            else
            {
                base.OnOptionsItemSelected(item);
            }

            return true;
        }

        private async void PerformLogout()
        {
            var mobilePhone = _mobilePhoneRepository.FirstOrDefault();

            if (UserLoginMethod == UserLoginMethod.Normal)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        AuthService.Logout(mobilePhone?.MobilePhoneId).Wait();
                    }
                    catch (ApiException e)
                    {
                        int a = 5;
                        // Try to logoff from the server. If no connection it doesn't matter
                    }
                });
            }
            _mobilePhoneRepository.DeleteAll();

            await Task.Run(() =>
            {
                GetSharedPreferences(GetString(Resource.String.sharedPreferencesKey), FileCreationMode.Private).Edit().Clear().Commit();
                CachingHolder.Instance.Stores.Clear();
                CachingHolder.Instance.Coupons.Clear();
            });

            var intent = new Intent(this, typeof(LoginActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            StartActivity(intent);
            FinishAffinity();
        }

        private void ShowCoupons()
        {
            if (UserLoginMethod == UserLoginMethod.Normal)
            {
                StartActivity(typeof(CouponsListActivity));
            }
            else
            {
                Snackbar.Make(Window.DecorView.FindViewById(Android.Resource.Id.Content), GetString(Resource.String.functionNotAvailableAsAnonymous), Snackbar.LengthLong).Show();
            }
        }

        private void ShowMap()
        {
            StartActivity(typeof(MapsActivity));
        }

        private void SaveKundenkarte(Bitmap image)
        {
            if (File.Exists(KundenkartePath)) return;

            using (var os = new FileStream(KundenkartePath, FileMode.CreateNew))
            {
                image.Compress(Bitmap.CompressFormat.Jpeg, 100, os);
            }
        }

        private void ShowStores()
        {
            if (UserLoginMethod == UserLoginMethod.Normal)
            {
                StartActivity(typeof(StoresListActivity));
            }
            else
            {
                Snackbar.Make(Window.DecorView.FindViewById(Android.Resource.Id.Content), GetString(Resource.String.functionNotAvailableAsAnonymous), Snackbar.LengthLong).Show();
            }
        }

        public override void OnBackPressed()
        {
            if (_backPressed.AddSeconds(2) > DateTime.Now)
            {
                FinishAffinity();
            }
            else
            {
                Snackbar.Make(_drawerLayout, Resource.String.repeatForFinish, Snackbar.LengthLong).Show();
                _backPressed = DateTime.Now;
            }
        }

        public bool IsPlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            return resultCode == ConnectionResult.Success;
        }
    }
}