using System;
using System.Globalization;
using GCloud.Shared.Dto.Domain;
using GCloudiPhone.Caching;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Refit;
using UIKit;

namespace GCloudiPhone
{
    public partial class CouponListItem : UITableViewCell
    {
        public CouponDto Coupon { get; set; }
        private readonly CultureInfo cultureInfo = new CultureInfo("de-AT");

        private readonly UserRepository _userRepository;
        private readonly IAuthService _authService;

        public bool couponValidation = false;

        public CouponListItem(IntPtr handle) : base(handle)
        {
            _userRepository = new UserRepository(DbBootstraper.Connection);
            _authService = RestService.For<IAuthService>(HttpClientContainer.Instance.HttpClient);
        }

        public CouponListItem()
        {
            //SetCellData(coupon);
        }

        public void UpdateCell(CouponDto coupon)
        {
            SetCellData(coupon);
        }

        public void SetCellData(CouponDto coupon)
        {
            Coupon = coupon;
            CouponRedeemsLeft.TextColor = UIColor.LightGray;
            Accessory = UITableViewCellAccessory.DisclosureIndicator;
            BackgroundColor = UIColor.Clear;
            UserInteractionEnabled = true;

            LoadCouponImage(coupon);

            CouponTitle.Text = coupon.Name;

            if (coupon.RedeemsLeft != null)
            {
                if (coupon.RedeemsLeft <= 0)
                {
                    CouponRedeemsLeft.Text = $@"aufgebraucht";
                    coupon.IsValid = false;
                }
                else
                {
                    CouponRedeemsLeft.Text = $@"{coupon.RedeemsLeft}x einlösbar";

                }

            }
            else
            {
                CouponRedeemsLeft.Text = "∞";
            }

            var now = DateTime.Now;
            var daydifferenceToFrom = coupon.ValidFrom.HasValue && coupon.ValidFrom.Value.Date >= now.Date
                ? (coupon.ValidFrom.Value.Date - now.Date).Days
                : -1;
            var daydifferenceToTo = coupon.ValidTo.HasValue && coupon.ValidTo.Value.Date >= now.Date
                ? (coupon.ValidTo.Value.Date - now.Date).Days
                : -1;

            if (CouponValidUntil != null)
            {
                if (daydifferenceToFrom > 1)
                {
                    CouponValidUntil.Text = $@"Gültig in {daydifferenceToFrom} Tagen";
                }
                else if (daydifferenceToFrom == 1)
                {
                    CouponValidUntil.Text = $@"Gültig in {daydifferenceToFrom} Tag";
                }
                else if (daydifferenceToFrom == 0)
                {
                    CouponValidUntil.Text = $@"Ab heute gültig!";
                }

                if (daydifferenceToTo > 1 && daydifferenceToFrom <= 0)
                {
                    CouponValidUntil.Text = $@"Noch {daydifferenceToTo} Tage gültig!";
                }
                else if (daydifferenceToTo == 1)
                {
                    CouponValidUntil.Text = $@"Noch {daydifferenceToTo} Tag gültig!";
                }
                else if (daydifferenceToTo == 0)
                {
                    CouponValidUntil.Text = $@"Nur noch heute gültig!";
                }
                else
                {
                    CouponValidUntil.Text = "Gültig bis auf Widerruf";
                }
            }

            var user = _userRepository.GetCurrentUser();
            var totalPoints = _authService.GetTotalPointsByUserID(user.UserId).Result;
            var totalPointsNew = totalPoints.Replace("\"", "");

            switch (coupon.CouponType)
            {
                case CouponTypeDto.Value:
                    CouponValue.Text = $@"Wert: {coupon.Value.ToString("C", cultureInfo)}";
                    break;
                case CouponTypeDto.Percent:
                    CouponValue.Text = $@"Wert: {(coupon.Value / 100).ToString("P", cultureInfo)}";
                    break;
                case CouponTypeDto.Points:
                    CouponValue.Text = $@"Wert: {(coupon.Value).ToString()}"+" Punkte";
                    break;
                case CouponTypeDto.SpecialProductPoints:
                    if(Convert.ToInt32(totalPointsNew) > coupon.Value )
                    {
                        CouponValue.Text = $@"Wert: {(coupon.Value).ToString()}" + " Punkte";
                    }
                    else
                    {
                        var punkteFehlen = (-1*(Convert.ToInt32(totalPointsNew) - coupon.Value));
                        CouponValue.Text = $@"{punkteFehlen.ToString()}" + " Punkte fehlen";
                        //CouponRedeemsLeft.TextColor = UIColor.Red;
                        CouponValue.TextColor = UIColor.Red;
                        //this.Accessory = UITableViewCellAccessory.None;
                        //this.BackgroundColor = UIColor.LightGray;
                        //UserInteractionEnabled = false;
                    }
                   
                    break;
                default:
                    break;
            }

            if (((AppDelegate)UIApplication.SharedApplication.Delegate).AuthState == AuthState.Authorized)
            {
                UserInteractionEnabled = true;
                Accessory = UITableViewCellAccessory.DisclosureIndicator;
            }
            else
            {
                UserInteractionEnabled = false;
                Accessory = UITableViewCellAccessory.None;
            }

       
            if (Convert.ToInt32(totalPointsNew) > coupon.Value)
            {
                couponValidation = false;
            }
            else
            {
                couponValidation = true;
            }

            if (!coupon.IsValid || couponValidation)
            {
                CouponRedeemsLeft.TextColor = UIColor.Red;
                this.Accessory = UITableViewCellAccessory.None;
                this.BackgroundColor = UIColor.LightGray;
                UserInteractionEnabled = false;

                if (CouponValidUntil != null)
                {
                    CouponValidUntil.Text = "";
                }

                if (coupon.ValidFrom.HasValue && coupon.ValidFrom.Value.Date > DateTime.Now)
                {
                    CouponRedeemsLeft.Text = "noch nicht gültig";

                }
                else if (coupon.ValidTo.HasValue && coupon.ValidTo.Value.Date < DateTime.Now)
                {
                    CouponRedeemsLeft.Text = "abgelaufen";
                }
            }


        }

        private void LoadCouponImage(CouponDto coupon)
        {
            if (string.IsNullOrWhiteSpace(coupon.IconBase64))
            {
                CouponImage.Image = UIImage.FromBundle("No_image_available");
            }
            else
            {
                CouponImage.Image = CachingService.GetCouponImage(coupon);
            }
        }
    }
}