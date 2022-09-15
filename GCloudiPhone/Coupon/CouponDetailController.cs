using System;
using System.Globalization;
using CoreGraphics;
using Foundation;
using GCloud.Shared.Dto.Domain;
using GCloudShared.Repository;
using GCloudShared.Shared;
using UIKit;

namespace GCloudiPhone
{
    public partial class CouponDetailController : UIViewController
    {
        private CouponDto coupon;
        public CouponDto Coupon
        {
            get
            {
                return coupon;
            }
            set
            {
                coupon = value;
                UpdateUI();
            }
        }

        private UIBarButtonItem shareButton;
        private UserRepository userRepository;

        public CouponDetailController(IntPtr handle) : base(handle)
        {
            userRepository = new UserRepository(DbBootstraper.Connection);
        }

        public override void ViewDidLoad()
        {
            shareButton = new UIBarButtonItem(UIBarButtonSystemItem.Action, ShareEventHandler);
            NavigationItem.RightBarButtonItem = shareButton;

            ShadowView.Layer.ShadowColor = UIColor.Black.CGColor;
            ShadowView.Layer.ShadowOffset = new CGSize(5.0d, 5.0d);
            ShadowView.Layer.ShadowRadius = 5.0f;
            ShadowView.Layer.ShadowOpacity = 0.5f;
            ShadowView.Layer.ShadowRadius = 5.0f;

            base.ViewDidLoad();
        }

        private void UpdateUI()
        {
            LoadViewIfNeeded();

            NavigationItem.Title = Coupon.Name;

            CouponDescriptionTextField.Text = Coupon.ShortDescription;
            var culture = new CultureInfo("de-AT");

            if (Coupon.ValidFrom == null && Coupon.ValidTo != null)
            {
                ValiditySpanTextField.Text = string.Format("Gültig bis {0:dd.MM.yyyy}", Coupon.ValidTo);
            }
            else if (Coupon.ValidFrom == null && Coupon.ValidTo == null)
            {
                ValiditySpanTextField.Text = string.Format("Gültig bis auf Widerruf");
            }
            else
            {
                ValiditySpanTextField.Text = string.Format("Gültig von {0:dd.MM.yyyy} bis {1:dd.MM.yyyy}", Coupon.ValidFrom, Coupon.ValidTo);
            }

            RedeemsLeftTextField.Text = Coupon.RedeemsLeft != null ? string.Format("Noch {0} Mal verwendbar", Coupon.RedeemsLeft) : "unbegrenzt einlösbar";

            switch (Coupon.CouponType)
            {
                case CouponTypeDto.Value:
                    CouponValueLabel.Text = string.Format("Gutscheinwert: {0}", Coupon.Value.ToString("C", culture));
                    break;
                case CouponTypeDto.Percent:
                    CouponValueLabel.Text = string.Format("Gutscheinwert: {0}", (Coupon.Value / 100).ToString("P", culture));
                    break;
                case CouponTypeDto.Points:
                    CouponValueLabel.Text = string.Format("Gutscheinwert: {0} Punkte", Coupon.Value.ToString());
                    break;
                case CouponTypeDto.SpecialProductPoints:
                    CouponValueLabel.Text = string.Format("Gutscheinwert: {0} Punkte", Coupon.Value.ToString());
                    break;
            }

            LoadQrCodeImage();
            LoadCouponImage();
        }

        //public override void ViewWillAppear(bool animated)
        //{
        //    base.ViewWillAppear(animated);

        //    NavigationController.NavigationBar.SetBackgroundImage(new UIImage(), UIBarMetrics.Default);
        //    NavigationController.NavigationBar.BarTintColor = UIColor.Clear;
        //    NavigationController.NavigationBar.BackgroundColor = UIColor.Clear;
        //    NavigationController.View.BackgroundColor = UIColor.Clear;
        //    NavigationController.NavigationBar.Translucent = true;

        //    NavigationController.NavigationBar.TintColor = UIColor.FromRGB(255, 87, 34);

        //}

        //public override void ViewWillDisappear(bool animated)
        //{
        //    base.ViewDidDisappear(animated);

        //    NavigationController.NavigationBar.SetBackgroundImage(null, UIBarMetrics.Default);
        //    NavigationController.NavigationBar.BarTintColor = UIColor.FromRGB(255, 87, 34);
        //    NavigationController.NavigationBar.TintColor = UIColor.White;
        //    NavigationController.NavigationBar.Translucent = false;
        //}

        void ShareEventHandler(object sender, EventArgs e)
        {
            NSObject[] activityItems = {
                new NSString("Ich habe einen neuen Gutschein auf FoodJet gefunden!"),
                new NSString(""),
                new NSString(Coupon.ShortDescription),
                new NSUrl(UriContainer.BasePath.ToUriString() + "Coupons/Details/" + Coupon.Id.ToString())
                };
            UIActivityViewController activityViewController = new UIActivityViewController(activityItems, null)
            {
                ExcludedActivityTypes = new NSString[] { }
            };
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
            {
                activityViewController.PopoverPresentationController.SourceView = View;
                activityViewController.PopoverPresentationController.SourceRect = new CGRect((View.Bounds.Width - 32), (NavigationController.NavigationBar.Frame.Height * 1.33), 0, 0);
            }
            this.PresentViewController(activityViewController, true, null);
        }

        public void LoadQrCodeImage()
        {
            CouponQrCode.Image = Caching.CachingService.GetCouponQrCode(Coupon);
        }

        public void LoadCouponImage()
        {
            if (string.IsNullOrWhiteSpace(Coupon.IconBase64))
            {
                CouponImage.Image = UIImage.FromBundle("No_image_available");
            }
            else
            {
                CouponImage.Image = Caching.CachingService.GetCouponImage(Coupon);
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}