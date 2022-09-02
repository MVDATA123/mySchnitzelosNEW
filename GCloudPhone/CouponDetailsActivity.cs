using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using GCloud.Shared;
using GCloud.Shared.Dto.Domain;
using mvdata.foodjet.Control;
using GCloudShared.Service;
using GCloudShared.Shared;
using mvdata.foodjet.Caching;
using mvdata.foodjet.Utils;
using Newtonsoft.Json;
using Optional;
using Refit;
using ZXing;
using ZXing.QrCode;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace mvdata.foodjet
{
    [Activity(Label = "", ScreenOrientation = ScreenOrientation.Portrait)]
    public class CouponDetailsActivity : BaseActivity
    {
        private ImageView _imageView;
        private Toolbar _toolbar;
        private TextView _txtValidFrom;
        private TextView _txtValidTo;
        private TextView _txtRedeemsLeft;
        private TextView _txtAmount;
        private TextView _txtToolBarHeader;
        private TextView _txtCouponDescription;
        private ImageView _imgToolbarLogo;
        private ProgressBar _progressBar;

        private Option<CouponDto> _coupon;

        private IUserCouponService _userCouponService;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _userCouponService = RestService.For<IUserCouponService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);

            var couponGuid = Intent.GetStringExtra("couponGuid");

            SetContentView(Resource.Layout.CouponDetails);

            _toolbar = FindViewById<Toolbar>(Resource.Id.app_bar);
            _imageView = FindViewById<ImageView>(Resource.Id.imageViewCouponDetails);
            _txtValidFrom = FindViewById<TextView>(Resource.Id.txtCouponDetailsValidFrom);
            _txtValidTo = FindViewById<TextView>(Resource.Id.txtCouponDetailsValidTo);
            _txtRedeemsLeft = FindViewById<TextView>(Resource.Id.txtCouponDetailsRedeemsLeft);
            _txtAmount = FindViewById<TextView>(Resource.Id.txtCouponDetailsAmount);
            _imgToolbarLogo = FindViewById<ImageView>(Resource.Id.imgAppBarCouponDetailsLogo);
            _txtToolBarHeader = FindViewById<TextView>(Resource.Id.txtAppBarCouponDetailsText);
            _txtCouponDescription = FindViewById<TextView>(Resource.Id.txtCouponDetailsDescription);
            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBarCouponDetails);

            SetSupportActionBar(_toolbar);
            //SupportActionBar.SetTitle(Resource.String.couponDetailsActivityHeader);

            SetQrCode(couponGuid);
            // Create your application here
            if (Guid.TryParse(couponGuid, out var guid))
            {
                _coupon = CachingHolder.Instance.GetCouponByGuid(guid);
            }
            InitCouponView();
            _progressBar.Visibility = ViewStates.Invisible;
        }

        private void SetQrCode(string couponGuid)
        {
            var userId = GetSharedPreferences(GetString(Resource.String.sharedPreferencesKey), FileCreationMode.Private).GetString(GetString(Resource.String.sharedPreferencesUserId), "");

            if (Guid.TryParse(userId, out _))
            {
                var userRedeem = new { UserId = userId, CouponId = couponGuid };
                var bmp = QrCodeUtils.GetQrCode(JsonConvert.SerializeObject(userRedeem), 250, 250);

                if (bmp != null)
                {
                    _imageView.SetImageBitmap(bmp);
                }
            }
        }

        private void InitCouponView()
        {
            _coupon.MatchSome(c =>
            {
                if (!string.IsNullOrWhiteSpace(c.IconBase64))
                {
                    var bytes = Convert.FromBase64String(c.IconBase64);
                    Glide.With(this).AsBitmap().Load(bytes).Into(_imgToolbarLogo);
                }
                else
                {
                    Glide.With(this).AsDrawable().Load(Resource.Drawable.No_image_available).Into(_imgToolbarLogo);
                }
                _txtToolBarHeader.Text = c.Name;
                _txtCouponDescription.Text = c.ShortDescription;
                _txtValidFrom.Text = c.ValidFrom?.ToString("dd MMMM yyyy") ?? GetString(Resource.String.unlimited);
                _txtValidTo.Text = c.ValidTo?.ToString("dd MMMM yyyy") ?? GetString(Resource.String.unlimited);
                _txtRedeemsLeft.Text = c.RedeemsLeft.HasValue ? $"{c.RedeemsLeft.Value}  {GetString(Resource.String.times)}" : GetString(Resource.String.unlimited);
                _txtAmount.Text = c.CouponType == CouponTypeDto.Value ? c.Value.ToString("C", CultureInfo.CurrentCulture) : decimal.Divide(c.Value, 100m).ToString("P", CultureInfo.CurrentCulture);
            });
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.CouponDetails, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.couponDetailMenuShare:
                    var sharingIntent = new Intent(Intent.ActionSend);
                    sharingIntent.SetType("text/plain");
                    var shareBodyText = "";
                    _coupon.MatchSome(x =>
                    {
                        shareBodyText = $"{GetText(Resource.String.CouponDetailsSharingText)}\n{BaseUrlContainer.BaseUri}Coupons/Details/" + x.Id;
                    });
                    Intent shareIntent = ShareCompat.IntentBuilder.From(this)
                        .SetType("text/plain")
                        .SetText(shareBodyText)
                        .SetChooserTitle(Resource.String.CouponDetailsSharingDrawerTitle)
                        .Intent;
                    if (shareIntent.ResolveActivity(PackageManager) != null)
                    {
                        StartActivity(shareIntent);
                    }
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}