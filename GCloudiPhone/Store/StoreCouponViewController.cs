using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using GCloud.Shared.Dto.Domain;
using GCloudiPhone.Caching;
using GCloudiPhone.Extensions;
using GCloudiPhone.Helpers;
using GCloudiPhone.Shared;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Optional.Collections;
using Refit;
using UIKit;

namespace GCloudiPhone
{
    public partial class StoreCouponViewController : UIViewController, ICanCleanUpMyself
    {
        public StoreLocationDto Store { get; set; }
        private readonly IUserStoreService userStoreService;
        private readonly StoreWhitelistRepository whitelistRepository;

        private List<CouponDto> coupons;
        private string storeAddress;
        private NSUrl openUrl;

        private UIBarButtonItem[] all;
        private UIBarButtonItem[] navOnly;

        private StoreDetailTableHeaderView Header;
        private CouponTableSource couponTableSource;
        private LoadingOverlay loading;
        private UITableViewController couponTableController;
        private WeakReference<UITableView> tableViewRef;
        UIRefreshControl RefreshControl;
        private UIBarButtonItem backButton;

        private readonly UserRepository _userRepository;
        private readonly IAuthService _authService;

        public StoreCouponViewController(IntPtr handle) : base(handle)
        {
            whitelistRepository = new StoreWhitelistRepository(DbBootstraper.Connection);
            userStoreService = RestService.For<IUserStoreService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);

            _userRepository = new UserRepository(DbBootstraper.Connection);
            _authService = RestService.For<IAuthService>(HttpClientContainer.Instance.HttpClient);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            couponTableController = new UITableViewController { TableView = CouponsTable };
            loading = new LoadingOverlay(CouponsTable.Frame);
            //CouponsTable.AddSubview(loading);

            couponTableSource = new CouponTableSource(new List<CouponDto>());
            CouponsTable.WeakDataSource = couponTableSource;
            tableViewRef = new WeakReference<UITableView>(CouponsTable);
            //CouponsTable.WeakDataSource = couponTableSource;
            //CouponsTable.ReloadData();
            CouponsTable.TableFooterView = new UIView(new CGRect(0, 0, 0, 0));

            RefreshControl = new UIRefreshControl();

            NavigationItem.RightBarButtonItems = null;

        }

        void RefreshControl_ValueChanged(object sender, EventArgs e)
        {
            RefreshControl.BeginRefreshing();
            LoadCoupons(true); // TODO: consider to put await here
        }

        public void LoadStoreImage()
        {
            if (Store.BannerImage == null)
            {
                return;
            }
            InvokeOnMainThread(() => StoreImage.Image = CachingService.GetStoreBanner(Store));
        }

        private void LoadTableHeader()
        {
            var views = NSBundle.MainBundle.LoadNib("StoreDetailTableHeader", null, null);
            Header = ObjCRuntime.Runtime.GetNSObject(views.ValueAt(0)) as StoreDetailTableHeaderView;
            views = null;

            Header.Follow.Layer.BorderWidth = 1;
            Header.Follow.Layer.BorderColor = UIColor.FromRGB(255, 87, 34).CGColor;

            //Header.Unfollow.BackgroundColor = Header.Follow.TintColor;

            Header.UserInteractionEnabled = true;
            Header.StoreName = Store.Name;
            Header.CompanyName = Store.Company.Name;
            Header.StoreAddress = Store.Address;
            var bounds = Header.Bounds;
            bounds.Height = 85;
            Header.Bounds = bounds;

            CouponsTable.TableHeaderView = Header;
        }

        public override async void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (all == null)
            {
                all = new[] { NavigateButton, CashbackButton };
            }

            if (navOnly == null)
            {
                navOnly = new[] { NavigateButton };
            }

            if (Header == null)
            {
                LoadTableHeader();
            }

            SubscribeEventHandlers();

            if (((AppDelegate)UIApplication.SharedApplication.Delegate).AuthState == AuthState.Authorized)
                IsFollowingStore();

            CouponsTable.AddSubview(RefreshControl);

            await LoadCoupons(false);
            InvokeInBackground(LoadStoreImage);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (Header != null)
            {
                UnsubscribeEventHandlers();
            }
            RefreshControl.RemoveFromSuperview();
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            switch (segue.Identifier)
            {
                case "CashbackSegue":
                    var cashbackViewController = segue.DestinationViewController as CashbackViewController;
                    cashbackViewController.StoreGuid = Store.Id.ToString();
                    break;
                case "CouponDetailsSegue":
                    var couponDetails = segue.DestinationViewController as CouponDetailController;
                    couponDetails.Coupon = (sender as CouponListItem).Coupon;
                    break;
            }

            if (backButton == null)
            {
                backButton = new UIBarButtonItem("Zurück", UIBarButtonItemStyle.Plain, null, null);
            }
            NavigationItem.BackBarButtonItem = backButton;
            base.PrepareForSegue(segue, sender);
        }

        private void IsFollowingStore()
        {
            NavigationItem.RightBarButtonItems = Store.Company.IsCashbackEnabled ? all : navOnly;

            if (Store.IsUserFollowing)
            {
                Header.Unfollow.Hidden = false;
                Header.Follow.Hidden = true;
                IsNotificationsEnabled();
            }
            else
            {
                NavigationItem.RightBarButtonItem = null;
                Header.Unfollow.Hidden = true;
                Header.Follow.Hidden = false;

                Header.DisableNotifications.Hidden = true;
                Header.EnableNotifications.Hidden = true;
            }
        }

        private void IsNotificationsEnabled()
        {
            if (whitelistRepository.FindFirstBy(entry => entry.Store.Equals(Store.Id)) != null)
            {
                Header.DisableNotifications.Hidden = false;
                Header.EnableNotifications.Hidden = true;
            }
            else
            {
                Header.DisableNotifications.Hidden = true;
                Header.EnableNotifications.Hidden = false;
            }
        }

        private void FollowStore(object sender, EventArgs e)
        {
            CachingService.FollowStore(Store.Id.ToString());

            Header.Follow.Hidden = true;
            Header.Unfollow.Hidden = false;
            EnableNotifications(null, null);
        }

        private void UnfollowStore(object sender, EventArgs e)
        {
            CachingService.UnfollowStore(Store.Id.ToString());

            Header.Follow.Hidden = false;
            Header.Unfollow.Hidden = true;

            DisableNotifications(null, null);

            Header.EnableNotifications.Hidden = true;
            Header.DisableNotifications.Hidden = true;
        }

        private void EnableNotifications(object sender, EventArgs e)
        {
            NotificationsHelper.Instance.SubscribeStore(Store.Id);
            Header.EnableNotifications.Hidden = true;
            Header.DisableNotifications.Hidden = false;
        }

        private void DisableNotifications(object sender, EventArgs e)
        {
            NotificationsHelper.Instance.UnsubscribeStore(Store.Id);
            Header.EnableNotifications.Hidden = false;
            Header.DisableNotifications.Hidden = true;
        }

        private async Task LoadCoupons(bool refresh)
        {
            if (refresh)
            {
                await CachingService.UpdateCache(true);
                RefreshControl.EndRefreshing();
            }
            coupons = CacheHolder.Instance.GetCouponsByStore(Store.Id).Values().ToList();
            if (coupons.Count == 0)
            {
                TableViewHelper.EmptyMessage("Keine Gutscheine gefunden", new WeakReference<UITableViewController>(couponTableController));
            }
            else
            {
                CouponsTable.BackgroundView = null;
            }

            if (couponTableSource == null)
            {
                couponTableSource = new CouponTableSource(new List<CouponDto>());
                CouponsTable.WeakDataSource = couponTableSource;
                CouponsTable.ReloadData();
            }

            var user = _userRepository.GetCurrentUser();
            var totalPoints = _authService.GetTotalPointsByUserID(user.UserId).Result;
            totalPoints = totalPoints.Replace("\"", "");

            foreach (CouponDto coupon in coupons)
            {
                if (coupon.CouponType == CouponTypeDto.Points)
                {
                    if (totalPoints != "null")
                    {
                        coupon.Value = Convert.ToDecimal(totalPoints);
                    }
                    else
                    {
                        coupon.Value = Convert.ToDecimal(0);
                    }
                }
            }

            couponTableSource.UpdateTableSource(tableViewRef, coupons);
            //CouponsTable.ReloadData();
            coupons = null;
            //loading.Hide();
        }

        partial void NavigateButton_Activated(UIBarButtonItem sender)
        {
            storeAddress = Uri.EscapeUriString(Store.Address);
            storeAddress = "https://maps.apple.com/?daddr=" + storeAddress;
            openUrl = new NSUrl(storeAddress);
            if (UIApplication.SharedApplication.CanOpenUrl(openUrl))
            {
                UIApplication.SharedApplication.OpenUrl(openUrl);
            }
            else
            {
                var alert = UIAlertController.Create("Fehler", "Karten sind auf diesem Gerät nicht verfügbar", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, (obj) => alert = null));
                PresentViewController(alert, true, null);
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
            if (backButton != null)
            {
                backButton.Dispose();
                backButton = null;
            }
            if (CouponsTable != null)
            {
                CouponsTable.BackgroundView = null;
            }
            coupons = null;
            openUrl = null;
            storeAddress = null;
            if (Header != null)
            {
                UnsubscribeEventHandlers();
                Header.Dispose();
                Header = null;
            }
            all = null;
            navOnly = null;

            CachingService.ClearCachedImages();
        }

        private void SubscribeEventHandlers()
        {
            RefreshControl.ValueChanged += RefreshControl_ValueChanged;

            Header.EnableNotifications.TouchUpInside += EnableNotifications;
            Header.DisableNotifications.TouchUpInside += DisableNotifications;
            Header.Follow.TouchUpInside += FollowStore;
            Header.Unfollow.TouchUpInside += UnfollowStore;
        }

        private void UnsubscribeEventHandlers()
        {
            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;

            Header.EnableNotifications.TouchUpInside -= EnableNotifications;
            Header.DisableNotifications.TouchUpInside -= DisableNotifications;
            Header.Follow.TouchUpInside -= FollowStore;
            Header.Unfollow.TouchUpInside -= UnfollowStore;
        }

        public void CleanUp()
        {
            if (StoreImage != null)
            {
                StoreImage.Image = null;
            }

            if (Header != null)
            {
                Header.RemoveFromSuperview();
                Header.Dispose();
                Header = null;
            }

            if (couponTableSource != null)
            {
                couponTableSource.UpdateTableSource(tableViewRef, new List<CouponDto>());
            }
            if (CouponsTable != null)
            {
                CouponsTable.WeakDataSource = null;
            }
            couponTableSource = null;

            DidReceiveMemoryWarning();
        }
    }
}