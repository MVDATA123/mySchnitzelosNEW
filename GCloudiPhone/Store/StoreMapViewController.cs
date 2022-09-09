using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using CoreLocation;
using Foundation;
using GCloud.Shared.Dto.Domain;
using GCloudiPhone.Extensions;
using GCloudiPhone.Shared;
using GCloudShared.Service;
using GCloudShared.Shared;
using MapKit;
using Optional.Collections;
using Refit;
using SidebarNavigation;
using GCloudiPhone.Helpers;
using UIKit;
using System.Timers;
using StoreKit;

namespace GCloudiPhone
{
    public partial class StoreMapViewController : UIViewController, ICanCleanUpMyself
    {
        #region attributes
        private readonly IUserStoreService _userStoreService;

        private bool isStoreInfoShown;
        private int selectedPinIndex = -1;

        private List<StoreLocationDto> stores = new List<StoreLocationDto>();
        private List<CouponDto> coupons = new List<CouponDto>();

        private CLLocationManager locationManager;
        private WeakReference<FilterMenuTableViewController> FilterControllerRef;

        private UIVisualEffectView storeListBlurEffectView;
        private UIVisualEffectView focusOnUserBtnBlurEffectView;
        private UIVisualEffectView storeInfoBlur;

        private StoreLocationDto selectedStore;

        private List<StoreMKPointAnnotation> pins;
        private List<StoreMKPointAnnotation> filteredPins;

        private CGPoint storeInfoCenter;
        private CGPoint newStoreInfoCenter;
        private CGRect newStoreInfoBounds;
        private CGRect initialStoreInfoBounds;
        private CGRect maxStoreInfoBounds;

        private UIPanGestureRecognizer panGestureRecognizer;

        private UITableViewController couponsTableController;

        private UIBarButtonItem backButton;

        private StoreTableSource storeTableSource;
        private WeakReference<UITableView> storeTableRef;

        private CouponTableSource couponTableSource;
        private WeakReference<UITableView> couponTableRef;

        private MKMapViewDelegate mapViewDelegate;

        private FilterMenuTableViewController FilterMenuController
        {
            get
            {
                if (FilterSidebarViewController.Instance.TryGetTarget(out var filterSidebar))
                {
                    FilterControllerRef = filterSidebar.FilterMenuControllerRef;
                    return FilterControllerRef.TryGetTarget(out var filterMenuController) ? filterMenuController : null;
                }
                return null;
            }
        }

        private CLLocationCoordinate2D? lastUserPosition;

        private SidebarController SidebarController
        {
            get
            {
                if (FilterSidebarViewController.Instance.TryGetTarget(out var filterSidebar))
                {
                    return filterSidebar.SidebarController;
                }
                return null;
            }
        }

        private Timer reviewTimer;
        #endregion

        public StoreMapViewController(IntPtr handle) : base(handle)
        {
            _userStoreService = RestService.For<IUserStoreService>(HttpClientContainer.Instance.HttpClient);
            locationManager = new CLLocationManager();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            couponTableRef = new WeakReference<UITableView>(StoreInfoCouponsTable);
            storeTableRef = new WeakReference<UITableView>(StoreTableView);

            couponsTableController = new UITableViewController { TableView = StoreInfoCouponsTable };

            // NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIImage.FromBundle("FilterIcon"), UIBarButtonItemStyle.Plain, (sender, e) => SidebarController.ToggleMenu());

            FindSegmentControl.RemoveFromSuperview();
            //StoreMapView.Add(storeListBlurEffectView);
            StoreMapView.Add(StoreTableView);
            
            

            //NavigationItem.TitleView = FindSegmentControl;

            storeInfoCenter = StoreInfoView.Center;
            StoreInfoView.Center = new CGPoint(storeInfoCenter.X, storeInfoCenter.Y + 300);
            initialStoreInfoBounds = StoreInfoView.Bounds;
            maxStoreInfoBounds = new CGRect(0, View.Bounds.Y + 100, View.Bounds.Width, View.Bounds.Height - 100);

            StoreInfoView.UserInteractionEnabled = true;

            #region Blur Effect
            var blurEffect = UIBlurEffect.FromStyle(UIBlurEffectStyle.ExtraLight);

            //var statusBarWindow = (UIView)UIApplication.SharedApplication.ValueForKey(new NSString("statusBarWindow"));
            //var statusBar = statusBarWindow.Subviews[0];
            //var statusBarBlurEffectView = new UIVisualEffectView(blurEffect)
            //{
            //    Frame = statusBar.Frame,
            //    AutoresizingMask = UIViewAutoresizing.All
            //};
            //View.Add(statusBarBlurEffectView);

            FocusOnUserBtn.BackgroundColor = UIColor.Clear;
            focusOnUserBtnBlurEffectView = new UIVisualEffectView(blurEffect)
            {
                Frame = FocusOnUserBtn.Frame,
                AutoresizingMask = FocusOnUserBtn.AutoresizingMask,
                ClipsToBounds = true
            };
            focusOnUserBtnBlurEffectView.Layer.CornerRadius = 5;
            focusOnUserBtnBlurEffectView.Layer.BorderColor = UIColor.Black.CGColor;
            focusOnUserBtnBlurEffectView.Layer.BorderWidth = 0.1f;
            View.AddSubview(focusOnUserBtnBlurEffectView);

            var storeInfoFrame = StoreInfoView.Frame;
            storeInfoFrame.Y = 0;
            storeInfoBlur = new UIVisualEffectView(blurEffect)
            {
                Frame = storeInfoFrame,
                AutoresizingMask = UIViewAutoresizing.All,
                ClipsToBounds = true
            };
            storeInfoBlur.Layer.CornerRadius = 10;
            storeInfoBlur.Layer.BorderColor = UIColor.Black.CGColor;
            storeInfoBlur.Layer.BorderWidth = 0.1f;

            StoreInfoView.AddSubview(storeInfoBlur);
            StoreInfoView.SendSubviewToBack(storeInfoBlur);

            //View.SendSubviewToBack(FocusOnUserBtn);
            View.SendSubviewToBack(focusOnUserBtnBlurEffectView);
            View.SendSubviewToBack(StoreMapView);

            StoreTableView.BackgroundColor = UIColor.FromRGB(255, 206, 51);
            storeListBlurEffectView = new UIVisualEffectView(blurEffect)
            {
                Frame = StoreMapView.Frame,
                AutoresizingMask = UIViewAutoresizing.All
            };
            #endregion

            locationManager.RequestWhenInUseAuthorization();
            FilterMenuController.StoreMapViewController = this;

            //StoreTableView.RemoveFromSuperview();
            StoreInfoView.RemoveFromSuperview();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);


            if (mapViewDelegate == null)
            {
                mapViewDelegate = new StoreMapViewDelegate(new WeakReference<StoreMapViewController>(this));
            }
            StoreMapView.Delegate = mapViewDelegate;
            StoreMapView.MapType = MKMapType.Standard;

            panGestureRecognizer = new UIPanGestureRecognizer(DidPan);
            StoreInfoView.AddGestureRecognizer(panGestureRecognizer);

            locationManager.LocationsUpdated += DidUpdateUserLocation;

            SidebarController.Disabled = false;
            StoreMapView.ShowsUserLocation = true;
            locationManager.StartUpdatingLocation();
            //StoreMapView.UserTrackingMode = MKUserTrackingMode.Follow;

            FindSegmentControl.ValueChanged += FindSegmentControl_ValueChanged;
            FollowButton.TouchUpInside += FollowStore;
            UnFollowButton.TouchUpInside += UnfollowStore;

            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
            //initialUserFocus = true;
            LoadStores();

            var counter = NSUserDefaults.StandardUserDefaults.IntForKey(UserDefaultsKeys.MapVisitCount);
            counter++;
            NSUserDefaults.StandardUserDefaults.SetInt(counter, UserDefaultsKeys.MapVisitCount);

            if (NSUserDefaults.StandardUserDefaults.IntForKey(UserDefaultsKeys.MapVisitCount) % 5 == 0)
            {
                reviewTimer = new Timer(TimeSpan.FromSeconds(15).TotalMilliseconds);
                reviewTimer.Elapsed += ReviewTimer_Elapsed;
                reviewTimer.Start();
                reviewTimer.AutoReset = false;

                NSUserDefaults.StandardUserDefaults.SetInt(0, UserDefaultsKeys.MapVisitCount);
            }
        }

        void ReviewTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 3))
            {
                SKStoreReviewController.RequestReview();
            }
        }


        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            StoreInfoView.RemoveGestureRecognizer(panGestureRecognizer);
            panGestureRecognizer = null;

            SidebarController.Disabled = true;
            StoreMapView.ShowsUserLocation = false;
            locationManager.StopUpdatingLocation();

            locationManager.LocationsUpdated -= DidUpdateUserLocation;
            FollowButton.TouchUpInside -= FollowStore;
            UnFollowButton.TouchUpInside -= UnfollowStore;
            FindSegmentControl.ValueChanged -= FindSegmentControl_ValueChanged;

            if (couponTableSource != null)
            {
                couponTableSource.UpdateTableSource(couponTableRef, new List<CouponDto>());
            }

            if(reviewTimer != null)
            {
                reviewTimer.Stop();
                reviewTimer.Elapsed -= ReviewTimer_Elapsed;
                reviewTimer = null;
            }

            mapViewDelegate = null;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            StoreMapView.RemoveAnnotations(StoreMapView.Annotations);

            if (isStoreInfoShown)
            {
                CloseStoreInfo();
            }
        }

        private void LoadStores()
        {
            pins = new List<StoreMKPointAnnotation>();

            try
            {
                stores = Caching.CacheHolder.Instance.Stores.Values().ToList();
            }
            catch (ApiException apiEx)
            {
                if (apiEx.StatusCode == 0)
                {
                    stores = new List<StoreLocationDto>();
                }
            }
            foreach (var store in stores)
            {
                var storeCoupons = new List<CouponDto>();
                try
                {
                    storeCoupons = Caching.CacheHolder.Instance.GetCouponsByStore(store.Id).Values().ToList();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
                string subtitle;
                var couponsCount = storeCoupons.Count(c => c.IsValid);
                if (couponsCount == 1)
                {
                    subtitle = $@"{couponsCount} neues Angebot!";
                }
                else if (couponsCount == 0)
                {
                    subtitle = $@"keine neuen Angebote!";
                }
                else
                {
                    subtitle = $@"{couponsCount} neue Angebote!";
                }

                var pin = new StoreMKPointAnnotation
                {
                    Title = store.Name,
                    Subtitle = subtitle,
                    Coordinate = new CLLocationCoordinate2D(store.Latitude, store.Longitude),
                    Store = store
                };
                pins.Add(pin);
            }
            filteredPins = pins;

            if (!CLLocationManager.LocationServicesEnabled)
            {
                if (CLLocationManager.Status == CLAuthorizationStatus.Denied)
                {
                    var missingLocationPermissionAlert = UIAlertController.Create("Standort nicht verfügbar!", "Bitte aktiviere die Standort-Berichtigung für FoodJet in den Einstellungen.", UIAlertControllerStyle.Alert);
                    missingLocationPermissionAlert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                    PresentViewController(missingLocationPermissionAlert, true, null);
                    missingLocationPermissionAlert = null;
                }

                else if (CLLocationManager.Status == CLAuthorizationStatus.Restricted)
                {
                    var missingLocationPermissionAlert = UIAlertController.Create("Standort nicht verfügbar!", "Du hast nicht die Berechtigung den Standort zu aktivieren, bitte frage deinen Administrator.", UIAlertControllerStyle.Alert);
                    missingLocationPermissionAlert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                    PresentViewController(missingLocationPermissionAlert, true, null);
                    missingLocationPermissionAlert = null;
                }
            }

            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
        }

        void FindSegmentControl_ValueChanged(object sender, EventArgs e)
        {
            switch (FindSegmentControl.SelectedSegment)
            {
                case 0:
                    //storeListBlurEffectView.RemoveFromSuperview();
                    //StoreTableView.RemoveFromSuperview();
                    //FocusOnUserBtn.Hidden = false;
                    //focusOnUserBtnBlurEffectView.Hidden = false;
                    //break;
                    StoreMapView.Add(storeListBlurEffectView);
                    StoreMapView.Add(StoreTableView);
                    FocusOnUserBtn.Hidden = true;
                    focusOnUserBtnBlurEffectView.Hidden = true;
                    if (isStoreInfoShown)
                    {
                        CloseStoreInfo();
                    }
                    break;
                case 1:
                    StoreMapView.Add(storeListBlurEffectView);
                    StoreMapView.Add(StoreTableView);
                    FocusOnUserBtn.Hidden = true;
                    focusOnUserBtnBlurEffectView.Hidden = true;
                    if (isStoreInfoShown)
                    {
                        CloseStoreInfo();
                    }
                    break;
            }
        }

        private double CalcDistanceToUser(StoreLocationDto store)
        {
            var storeCoordinate = new CLLocationCoordinate2D(store.Latitude, store.Longitude);
            var userCoordinate = StoreMapView.UserLocation.Coordinate;
            var storeLocation = new CLLocation(storeCoordinate.Latitude, storeCoordinate.Longitude);
            var userLocation = new CLLocation(userCoordinate.Latitude, userCoordinate.Longitude);

            return storeLocation.DistanceFrom(userLocation);
        }

        private void DidUpdateUserLocation(object sender, CLLocationsUpdatedEventArgs e)
        {
            StoreMapView.MapType = MKMapType.Standard;
            StoreMapView.UserLocation.Title = "";
            SortPinsByDistanceToUser();
            double distanceBetweenUserLoc = 0d;
            if (lastUserPosition.HasValue)
            {
                var lastUserLoc = new CLLocation(lastUserPosition.Value.Latitude, lastUserPosition.Value.Longitude);
                var currentUserLoc = new CLLocation(StoreMapView.UserLocation.Coordinate.Latitude, StoreMapView.UserLocation.Coordinate.Longitude);

                distanceBetweenUserLoc = lastUserLoc.DistanceFrom(currentUserLoc);
            }
            lastUserPosition = StoreMapView.UserLocation.Coordinate;

            if (filteredPins != null)
            {
                FilterAnnotations(FilterMenuController.Tags, FilterMenuController.DistanceSliderValue);
                if (distanceBetweenUserLoc > 1000)
                {
                    ScrollToUserLocation();
                }
                //initialUserFocus = false;
            }
        }

        private void SortPinsByDistanceToUser()
        {
            if (pins != null && pins.Count > 0)
            {
                foreach (var pin in pins)
                {
                    pin.Store.DistanceToUser = CalcDistanceToUser(pin.Store);
                }
                pins = pins.OrderBy(pin => pin.Store.DistanceToUser).ToList();
            }
        }

        partial void FocusOnUserBtn_TouchUpInside(UIButton sender)
        {
            FilterAnnotations(FilterMenuController.Tags, FilterMenuController.DistanceSliderValue);
            ScrollToUserLocation();
        }

        private void ScrollToUserLocation()
        {
            if (filteredPins != null)
            {
                if (filteredPins.Count >= 3)
                {
                    var focusPins = filteredPins.GetRange(0, 3).ToList<IMKAnnotation>();
                    var userPin = new MKPointAnnotation { Coordinate = StoreMapView.UserLocation.Coordinate };
                    focusPins.Add(userPin);
                    StoreMapView.ShowAnnotations(focusPins.ToArray(), true);
                    StoreMapView.RemoveAnnotation(userPin);
                }
                else if ((filteredPins.Count < 3 && filteredPins.Count > 0))
                {
                    var focusPins = filteredPins.GetRange(0, filteredPins.Count).ToList<IMKAnnotation>();
                    var userPin = new MKPointAnnotation { Coordinate = StoreMapView.UserLocation.Coordinate };
                    focusPins.Add(userPin);
                    StoreMapView.ShowAnnotations(focusPins.ToArray(), true);
                    StoreMapView.RemoveAnnotation(userPin);
                }
                else
                {
                    StoreMapView.UserTrackingMode = MKUserTrackingMode.Follow;
                }
            }
            else
            {
                StoreMapView.UserTrackingMode = MKUserTrackingMode.Follow;
            }
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            if (segue.Identifier == "StoreDetailSegue")
            {
                var storeDetailViewController = segue.DestinationViewController as StoreCouponViewController;
                var storeListItem = sender as StoreListItem;
                storeDetailViewController.Store = storeListItem.Store;
            }

            if (segue.Identifier == "CouponDetailsSegue")
            {
                var couponDetailController = segue.DestinationViewController as CouponDetailController;
                //var couponListItem = sender as CouponListItem;
                //couponDetailController.Coupon = couponListItem.Coupon;
            }

            if (segue.Identifier == "PopUpSegue")
            {
                var storeDetailViewController = segue.DestinationViewController as StoreCouponViewController;
                //storeDetailViewController.Store = ((NSObjectWrapper)sender).Context as StoreLocationDto;
                //if (backButton == null)
                //{
                //    backButton = new UIBarButtonItem("Karte", UIBarButtonItemStyle.Plain, null, null);
                //}
                //NavigationItem.BackBarButtonItem = backButton;
            }

            base.PrepareForSegue(segue, sender);
        }

        public void StoreMapView_DidSelectAnnotationView(object sender, MKAnnotationViewEventArgs e)
        {
            StoreMapView.SetCenterCoordinate(e.View.Annotation.Coordinate, true);

            for (int i = 0; i < filteredPins.Count; i++)
            {
                if (filteredPins.ElementAt(i).Store.Id.Equals(((StoreMKPointAnnotation)e.View.Annotation).Store.Id))
                {
                    selectedPinIndex = i;
                    break;
                }
                selectedPinIndex = -1;
            }
            selectedStore = ((StoreMKPointAnnotation)e.View.Annotation).Store;
            StoreInfoStoreName.Text = selectedStore.Name;
            StoreInfoCompanyName.Text = selectedStore.Company.Name;

            if (!isStoreInfoShown)
            {
                OpenStoreInfo();
            }

            LoadCoupons(selectedStore.Id);
        }

        private void LoadCoupons(Guid storeGuid)
        {
            if (((AppDelegate)UIApplication.SharedApplication.Delegate).AuthState == AuthState.Authorized)
            {
                Caching.CacheHolder.Instance.GetStoreByGuid(storeGuid).MatchSome(store =>
                {
                    if (store.IsUserFollowing)
                    {
                        FollowButton.Hidden = true;
                        UnFollowButton.Hidden = false;
                    }
                    else
                    {
                        FollowButton.Hidden = false;
                        UnFollowButton.Hidden = true;
                    }
                });

            }
            else
            {
                FollowButton.Hidden = true;
                UnFollowButton.Hidden = true;
            }

            coupons = Caching.CacheHolder.Instance.GetCouponsByStore(storeGuid).Values().ToList();
            if (coupons.Count == 0)
            {
                TableViewHelper.EmptyMessage("Keine Gutscheine gefunden", new WeakReference<UITableViewController>(couponsTableController));
            }
            else
            {
                StoreInfoCouponsTable.BackgroundView = null;
            }

            if (couponTableSource == null)
            {
                couponTableSource = new CouponTableSource(coupons);
                StoreInfoCouponsTable.WeakDataSource = couponTableSource;
                StoreInfoCouponsTable.ReloadData();
                StoreInfoCouponsTable.TableFooterView = new UIView(new CGRect(0, 0, 0, 0));
            }
            else
            {
                couponTableSource.UpdateTableSource(couponTableRef, coupons);
            }

        }

        public void OpenStoreInfo()
        {
            View.Add(StoreInfoView);
            var focusOnUserBtnFrame = FocusOnUserBtn.Frame;
            focusOnUserBtnFrame.Y = focusOnUserBtnFrame.Y - StoreInfoView.Frame.Height + 40;

            UIView.Animate(0.4, () =>
            {
                StoreInfoView.Center = storeInfoCenter;
                FocusOnUserBtn.Frame = focusOnUserBtnFrame;
                focusOnUserBtnBlurEffectView.Frame = focusOnUserBtnFrame;
            });

            isStoreInfoShown = true;
        }

        public void CloseStoreInfo()
        {
            if (isStoreInfoShown)
            {
                var focusOnUserBtnFrame = FocusOnUserBtn.Frame;
                focusOnUserBtnFrame.Y = focusOnUserBtnFrame.Y + StoreInfoView.Frame.Height - 40;

                UIView.Animate(0.4, () =>
                {
                    StoreInfoView.Bounds = initialStoreInfoBounds;
                    StoreInfoView.Center = new CGPoint(storeInfoCenter.X, storeInfoCenter.Y + 300);
                    FocusOnUserBtn.Frame = focusOnUserBtnFrame;
                    focusOnUserBtnBlurEffectView.Frame = focusOnUserBtnFrame;
                });

                isStoreInfoShown = false;
                StoreInfoView.RemoveFromSuperview();
            }
        }

        private void DidPan(UIPanGestureRecognizer sender)
        {
            var location = sender.LocationInView(View);
            var velocity = sender.VelocityInView(View);
            var translation = sender.TranslationInView(View);
            switch (sender.State)
            {
                case UIGestureRecognizerState.Began:
                    newStoreInfoCenter = StoreInfoView.Center;
                    newStoreInfoBounds = StoreInfoView.Bounds;
                    break;

                case UIGestureRecognizerState.Changed:
                    var calculatedBounds = new CGRect(newStoreInfoBounds.X, newStoreInfoBounds.Y, newStoreInfoBounds.Width, newStoreInfoBounds.Height - translation.Y);
                    if (calculatedBounds.Height < initialStoreInfoBounds.Height || calculatedBounds.Height > maxStoreInfoBounds.Height)
                    {
                        return;
                    }
                    StoreInfoView.Center = new CGPoint(newStoreInfoCenter.X, newStoreInfoCenter.Y + (translation.Y / 2));
                    StoreInfoView.Bounds = calculatedBounds;
                    StoreInfoView.SetNeedsDisplay();
                    break;

                case UIGestureRecognizerState.Ended:
                    if (velocity.Y > 100)
                    {
                        UIView.Animate(0.3, () =>
                        {
                            StoreInfoView.Center = storeInfoCenter;
                            StoreInfoView.Bounds = initialStoreInfoBounds;
                        });
                    }
                    break;
            }
        }

        private void FollowStore(object sender, EventArgs e)
        {
            Caching.CachingService.FollowStore(selectedStore.Id.ToString());
            FollowButton.Hidden = true;
            UnFollowButton.Hidden = false;
            NotificationsHelper.Instance.SubscribeStore(selectedStore.Id);
        }

        private void UnfollowStore(object sender, EventArgs e)
        {
            Caching.CachingService.UnfollowStore(selectedStore.Id.ToString());
            FollowButton.Hidden = false;
            UnFollowButton.Hidden = true;
            NotificationsHelper.Instance.UnsubscribeStore(selectedStore.Id);
        }

        partial void PrevStoreButton_TouchUpInside(UIButton sender)
        {
            if (filteredPins.Count > 0)
            {
                if (selectedPinIndex > 0)
                {
                    selectedPinIndex--;
                }
                else
                {
                    selectedPinIndex = filteredPins.Count - 1;
                }
                StoreMapView.SelectAnnotation(filteredPins.ElementAt(selectedPinIndex), true);
            }
        }

        partial void NextStoreButton_TouchUpInside(UIButton sender)
        {
            if (filteredPins.Count > 0)
            {

                if (selectedPinIndex < filteredPins.Count - 1)
                {
                    selectedPinIndex++;
                }
                else
                {
                    selectedPinIndex = 0;
                }
                StoreMapView.SelectAnnotation(filteredPins.ElementAt(selectedPinIndex), true);
            }
        }

        public void FilterAnnotations(List<TagDto> tags, double distance)
        {
            SortPinsByDistanceToUser();
            var mapAnnotations = StoreMapView.Annotations.Select(a => a as StoreMKPointAnnotation).ToList();
            //StoreMapView.RemoveAnnotations(StoreMapView.Annotations);
            filteredPins = new List<StoreMKPointAnnotation>();
            foreach (var pin in pins)
            {
                if (distance < 10.0d)
                {
                    if ((pin.Store.DistanceToUser / 1000) > distance)
                    {
                        continue;
                    }
                }
                if (tags.Count > 0)
                {
                    if (!pin.Store.Tags.Any(s => tags.Any(t => t.Name.ToLower().Contains(s.Name.ToLower()))))
                    {
                        continue;
                    }
                }
                filteredPins.Add(pin);
            }
            if (mapAnnotations != null && mapAnnotations.Count > 0)
            {
                foreach (var pin in mapAnnotations)
                {
                    if (pin == null)
                    {
                        continue;
                    }
                    var filteredPin = filteredPins.FirstOrDefault(p => p.Store.Id.Equals(pin.Store.Id));
                    if (filteredPin == null)
                    {
                        StoreMapView.RemoveAnnotation(pin);
                    }
                }
                foreach (var filteredPin in filteredPins)
                {
                    var pin = mapAnnotations.FirstOrDefault(p => p != null && p.Store.Id.Equals(filteredPin.Store.Id));
                    if (pin == null)
                    {
                        StoreMapView.AddAnnotation(filteredPin);
                    }
                }
            }
            else
            {
                StoreMapView.AddAnnotations(filteredPins.ToArray());

            }
            if (storeTableSource == null)
            {
                storeTableSource = new StoreTableSource(filteredPins.Select(p => p.Store).ToList(), false, true);
                StoreTableView.Source = storeTableSource;
                StoreTableView.ReloadData();
                StoreTableView.TableFooterView = new UIView(new CGRect(0, 0, 0, 0));
            }
            else
            {
                if (storeTableRef == null)
                {
                    storeTableRef = new WeakReference<UITableView>(StoreTableView);
                }
                storeTableSource.UpdateTableSource(storeTableRef, filteredPins.Select(p => p.Store).ToList());
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.

            NavigationItem.BackBarButtonItem = null;
            if (backButton != null)
            {
                backButton.Dispose();
                backButton = null;
            }

            if (ViewIfLoaded?.Window == null && StoreMapView != null)
            {
                StoreMapView.RemoveAnnotations(StoreMapView.Annotations);
                StoreMapView.MapType = MKMapType.Satellite;
                StoreMapView.MapType = MKMapType.Standard;
                StoreMapView.Delegate = null;
            }
        }

        public void CleanUp()
        {
            foreach (var viewController in ChildViewControllers)
            {
                if (viewController is ICanCleanUpMyself canCleanUpMyself)
                {
                    canCleanUpMyself.CleanUp();
                }
            }

            if (StoreTableView != null)
            {
                StoreTableView.Source = null;
            }
            if (StoreInfoCouponsTable != null)
            {
                StoreInfoCouponsTable.WeakDataSource = null;
            }

            storeTableSource = null;
            couponTableSource = null;

            if (isStoreInfoShown)
            {
                CloseStoreInfo();
            }

            if (StoreMapView != null)
            {
                StoreMapView.RemoveAnnotations(StoreMapView.Annotations);
                StoreMapView.MapType = MKMapType.Satellite;
                StoreMapView.Delegate = null;
            }
        }
    }
}