using System;
using System.Resources;
using System.Threading;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.Animations;
using GCloud.Shared.Dto.Api;
using GCloudScanner.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.Json;
using Refit;
using ZXing.Mobile;
using Environment = System.Environment;

namespace GCloudScanner
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        private TextView _txtContent;
        private FloatingActionButton _fab;
        private CoordinatorLayout _mainRoot;
        private ImageView _imgResult;

        private ICouponService _couponService;

        private const int SelectCouponRequestCode = 1;
        private const int SetSettingsRequestCode = 2;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            _mainRoot = FindViewById<CoordinatorLayout>(Resource.Id.mainRootMain);
            _txtContent = FindViewById<TextView>(Resource.Id.txtMainContent);
            _couponService = RestService.For<ICouponService>(SettingsActivity.BaseUrl);
            _imgResult = FindViewById<ImageView>(Resource.Id.imgMainResult);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            _fab = FindViewById<FloatingActionButton>(Resource.Id.fab);

            if (AreSettingsAvailable())
            {
                _txtContent.Text = "Zum Einlösen Button drücken";
                _fab.SetImageResource(Resource.Drawable.baseline_camera_white_24);
                _fab.Click += FabOnClick;
            }
            else
            {
                _txtContent.Text = "Vor Scanvorgang Einstellungen anpassen";
                _fab.SetImageResource(Resource.Drawable.baseline_settings_white_24);
                _fab.Click += StartSettingsActivity;
            }
        }

        private void StartSettingsActivity(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(SettingsActivity));
            StartActivityForResult(intent, SetSettingsRequestCode);
        }

        private bool AreSettingsAvailable()
        {
            var pref = GetSharedPreferences(SettingsActivity.PrefKey, FileCreationMode.Private);
            return pref.Contains(SettingsActivity.StoreGuidPrefKey) && pref.Contains(SettingsActivity.ApiTokenPrefKey);

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            if (!AreSettingsAvailable())
            {
                MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            }
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                StartSettingsActivity(null, null);
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private async void FabOnClick(object sender, EventArgs eventArgs)
        {
#if __ANDROID__
            // Initialize the scanner first so it can track the current context
            MobileBarcodeScanner.Initialize(Application);
#endif

            var progress = new ProgressDialog(this)
            {
                Indeterminate = true
            };
            progress.SetCancelable(false);
            progress.SetCanceledOnTouchOutside(false);
            progress.SetMessage($"Wird entwertet.{Environment.NewLine}Bitte warten...");

            var scanner = new ZXing.Mobile.MobileBarcodeScanner();

            var result = await scanner.Scan();
            try
            {
                if (result != null)
                {
                    progress.Show();
                    try
                    {

                        UserCouponDto couponModel = null;

                        try
                        {
                            couponModel = JsonConvert.DeserializeObject<UserCouponDto>(result.Text);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        if (couponModel != null)
                        {
                            RedeemCoupon(Guid.Parse(couponModel.CouponId), couponModel.UserId);
                        }
                        else
                        {
                            if (Guid.TryParse(result.Text, out var userGuid))
                            {
                                var pref = GetSharedPreferences(SettingsActivity.PrefKey, FileCreationMode.Private);
                                var storeId = pref.GetString(SettingsActivity.StoreGuidPrefKey, null);

                                var intent = new Intent(this, typeof(CouponSelectionActivity));
                                intent.PutExtra("userId", userGuid.ToString());
                                intent.PutExtra("storeGuid", storeId);
                                StartActivityForResult(intent, SelectCouponRequestCode);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                    }

                }
            }
            finally
            {
                progress.Dismiss();
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == SelectCouponRequestCode && resultCode == Result.Ok)
            {
                var progress = new ProgressDialog(this)
                {
                    Indeterminate = true
                };
                progress.SetCancelable(false);
                progress.SetCanceledOnTouchOutside(false);
                progress.SetMessage($"Wird entwertet.{Environment.NewLine}Bitte warten...");

                var couponId = Guid.Parse(data.GetStringExtra("couponId"));
                var userId = data.GetStringExtra("userId");
                try
                {
                    RedeemCoupon(couponId, userId);
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                }
            } else if (requestCode == SetSettingsRequestCode && resultCode == Result.Ok)
            {
                RunOnUiThread(() =>
                {
                    _txtContent.Text = "Zum Einlösen Button drücken";
                    _fab.SetImageResource(Resource.Drawable.baseline_camera_white_24);
                    _fab.Click -= StartSettingsActivity;
                    _fab.Click += FabOnClick;
                });
            }
        }

        private async void RedeemCoupon(Guid couponId, string userId)
        {
            var pref = GetSharedPreferences(SettingsActivity.PrefKey, FileCreationMode.Private);
            var apiToken = pref.GetString(SettingsActivity.ApiTokenPrefKey, null);
            var cashRegisterId = Guid.Parse(pref.GetString(SettingsActivity.CashRegisterIdPrefKey, null));

            var coupon = await _couponService.GetCoupon(couponId, userId, apiToken, cashRegisterId);

            var httpResult = await _couponService.RedeemCoupon(
                new StoreCouponApiBindingModel.StoreCouponApiPutModel
                {
                    CashRegisterId = cashRegisterId,
                    CashValue = coupon.Value,
                    RedeemDateTime = DateTime.Now,
                    StoreApiToken = apiToken,
                    UserGuid = Guid.Parse(userId),
                    CouponId = couponId
                });
            RunOnUiThread(() =>
            {
                if (httpResult.IsSuccessStatusCode)
                {
                    _imgResult.SetImageResource(Resource.Drawable.check);
                }
                else
                {
                    _imgResult.SetImageResource(Resource.Drawable.cross);
                }

                FadeInImage();

                _imgResult.Visibility = ViewStates.Visible;
                _txtContent.Visibility = ViewStates.Invisible;
            });
        }

        private void FadeInImage()
        {
            //var fadeIn = ObjectAnimator.OfFloat(_imgResult, "alpha", .3f, 1f);
            //fadeIn.SetDuration(2000);

            //var animsationSet = new AnimatorSet();
            //animsationSet.Play(fadeIn);
            //animsationSet.Start();

            var moveDownToUp = new TranslateAnimation(0, 0, 2000, 0)
            {
                Duration = 2000,
                FillAfter = true
            };

            _imgResult.StartAnimation(moveDownToUp);
        }

        public override void OnBackPressed()
        {
        }
    }
}

