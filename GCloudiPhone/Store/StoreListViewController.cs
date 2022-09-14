using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using GCloud.Shared.Dto.Domain;
using GCloudiPhone.Caching;
using GCloudiPhone.Extensions;
using GCloudiPhone.Helpers;
using GCloudiPhone.Shared;
using GCloudShared.Extensions;
using GCloudShared.Service;
using GCloudShared.Shared;
using Optional.Collections;
using Refit;
using UIKit;
using ZXing.Mobile;

namespace GCloudiPhone
{
    public partial class StoreListViewController : UITableViewController, IUISearchResultsUpdating, ICanCleanUpMyself
    {
        readonly IUserStoreService _userStoreService;
        List<StoreLocationDto> _stores;

        private LoadingOverlay loading;
        private UISearchController search;
        private StoreTableSource tableSource;
        private WeakReference<UITableView> tableViewRef;
        private UIBarButtonItem loginBtn;

        public StoreListViewController(IntPtr handle) : base(handle)
        {
            _stores = new List<StoreLocationDto>();
            _userStoreService = RestService.For<IUserStoreService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
            loginBtn = new UIBarButtonItem(UIImage.FromBundle("LoginIcon"), UIBarButtonItemStyle.Plain, (sender, e) => TabBarController.PerformSegue("LoginSegue", this));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            tableSource = new StoreTableSource(_stores, true);
            tableViewRef = new WeakReference<UITableView>(StoreList);
            StoreList.Source = tableSource;
            //StoreList.RowHeight = 75;
   

            search = new ColoredUISearchController(searchResultsController: null)
            {
                DimsBackgroundDuringPresentation = false
            };
            search.SearchResultsUpdater = this;
            search.SearchBar.Placeholder = "Nach Kategorien durchsuchen";
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                NavigationItem.SearchController = search;
                NavigationItem.HidesSearchBarWhenScrolling = true;
            }
            else
            {
                search.SearchBar.SearchBarStyle = UISearchBarStyle.Default;
                search.SearchBar.SizeToFit();
                StoreList.TableHeaderView = search.SearchBar;
            }

            RefreshControl = new UIRefreshControl();
            loading = new LoadingOverlay(TableView.Frame);
            StoreList.AddSubview(RefreshControl);
            StoreList.TableFooterView = new UIView(new CGRect(0, 0, 0, 0));
        }

        void RefreshControl_ValueChanged(object sender, EventArgs e)
        {
            RefreshControl.BeginRefreshing();
            LoadStores(true);
        }


        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (((AppDelegate)UIApplication.SharedApplication.Delegate).AuthState == AuthState.Unauthorized)
            {
                NavigationItem.RightBarButtonItem = loginBtn;
            }
            else
            {
                //NavigationItem.RightBarButtonItem = AddStoreButton;
            }

            RefreshControl.ValueChanged += RefreshControl_ValueChanged;

            LoadStores(false);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;

            NavigationItem.RightBarButtonItem = null;
        }

        public async void LoadStores(bool refreshAll)
        {
            if (((AppDelegate)UIApplication.SharedApplication.Delegate).AuthState == AuthState.Unauthorized)
            {
                TableViewHelper.EmptyMessage("Wenn Du angemeldet bist werden hier Deine Lieblingsgeschäfte angezeigt.", new WeakReference<UITableViewController>(this));
                if (tableSource == null)
                {
                    tableSource = new StoreTableSource(_stores, true);
                    if (tableViewRef == null)
                    {
                        tableViewRef = new WeakReference<UITableView>(StoreList);
                    }
                    StoreList.Source = tableSource;
                }
                else
                {
                    if (tableViewRef == null)
                    {
                        tableViewRef = new WeakReference<UITableView>(StoreList);
                    }
                    tableSource.UpdateTableSource(tableViewRef, new List<StoreLocationDto>());
                }
                RefreshControl.EndRefreshing();
                return;
            }

            if (refreshAll)
            {
                try
                {
                    await CachingService.UpdateCache(true);
                }
                catch (ApiException)
                {
                    _stores = new List<StoreLocationDto>();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    ((AppDelegate)UIApplication.SharedApplication.Delegate).LogoffRedirectToLogin();
                }
            }
            //_stores = CacheHolder.Instance.Stores.Values().Where(store => store.IsUserFollowing).ToList();
            _stores = CacheHolder.Instance.Stores.Values().OrderByDescending(s => s.CreationDateTime).ToList();

            if (tableSource == null)
            {
                tableSource = new StoreTableSource(_stores, true);
                if (tableViewRef == null)
                {
                    tableViewRef = new WeakReference<UITableView>(StoreList);
                }
                StoreList.Source = tableSource;
                StoreList.ReloadData();
            }
            else
            {
                if (tableViewRef == null)
                {
                    tableViewRef = new WeakReference<UITableView>(StoreList);
                }
                tableSource.UpdateTableSource(tableViewRef, _stores, true);
            }

            if (_stores.Count <= 0)
            {
                TableViewHelper.EmptyMessage("Keine Geschäfte vorhanden.\nFüge ein Geschäft hinzu, indem Du rechts oben auf das Plus drückst und den Qr-Code scannst.", new WeakReference<UITableViewController>(this));
            }
            else
            {
                TableView.BackgroundView = null;
            }

            if (refreshAll)
            {
                RefreshControl.EndRefreshing();
            }
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            //if (segue.Identifier == "StoreDetailSegue")
            //{
            //    var storeDetailViewController = segue.DestinationViewController as StoreCouponViewController;
            //    var storeListItem = sender as StoreListItem;
            //    //storeDetailViewController.Store = sender is NSObjectWrapper ? ((NSObjectWrapper)sender).Context as StoreLocationDto : storeListItem.Store;

            //    if (search != null && search.Active)
            //        {
            //            search.Active = false;
            //    }
            //}

            if (segue.Identifier == "webShopSegue")
            {
                var webShopViewController = segue.DestinationViewController as WebShopViewController;
                var storeListItem = sender as StoreListItem;
                webShopViewController.Store = storeListItem.Store;
            }

            base.PrepareForSegue(segue, sender);
        }

        public void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            var query = searchController.SearchBar.Text;
            if (!string.IsNullOrWhiteSpace(query))
            {
                SearchStoresByTags(query);
            }
            else
            {
                LoadStores(false);
            }
        }

        private void SearchStoresByTags(string query)
        {
            var searchResult = CacheHolder.Instance.Stores.Values().Where(s => s.HasTag(query)).ToList();
            if (searchResult.Count > 0)
            {
                TableView.BackgroundView = null;
            }
            else
            {
                TableViewHelper.EmptyMessage("Keine Geschäfte zu den eingegebenen Tags gefunden.", new WeakReference<UITableViewController>(this));
            }

            if (tableSource == null)
            {
                tableSource = new StoreTableSource(_stores, true);
                if (tableViewRef == null)
                {
                    tableViewRef = new WeakReference<UITableView>(StoreList);
                }
                StoreList.Source = tableSource;
            }
            else
            {
                if (tableViewRef == null)
                {
                    tableViewRef = new WeakReference<UITableView>(StoreList);
                }
                tableSource.UpdateTableSource(tableViewRef, searchResult);
            }
        }

        #region QR-Code-Scanner
        //partial void AddStoreButton_Activated(UIBarButtonItem sender)
        //{
        //    StartScanner();
        //}

        async void StartScanner()
        {
            var options = new MobileBarcodeScanningOptions
            {
                CameraResolutionSelector = HandleCameraResolutionSelectorDelegate,
                TryHarder = true
            };

            var scanner = new MobileBarcodeScanner(this)
            {
                TopText = "Filiale folgen",
                BottomText = "Scanne den QR-Code der Filiale, um dieser zu folgen.",
                CancelButtonText = "Abbrechen",
                FlashButtonText = "Licht",
                CameraUnsupportedMessage = "Kamera ist nicht verfügbar!"
            };
            scanner.AutoFocus();

            //call scan with options created above
            var result = await scanner.Scan(options, true);

            if (result != null)
            {
                try
                {
                    await _userStoreService.AddToWatchList(result.Text);
                    RefreshControl_ValueChanged(this, null);
                }
                catch (ApiException apiException)
                {
                    InvokeOnMainThread(() =>
                    {
                        var errorModel = apiException.GetApiErrorResult();
                        errorModel.Match(some =>
                        {
                            using (var alertController = UIAlertController.Create("Fehler", some.Message, UIAlertControllerStyle.Alert))
                            {
                                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                                PresentViewController(alertController, true, null);
                            }
                        }, () =>
                        {
                            using (var alertController = UIAlertController.Create("Fehler", "Es ist ein unbekannter Fehler aufgetreten", UIAlertControllerStyle.Alert))
                            {
                                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                                PresentViewController(alertController, true, null);
                            }
                        });

                    });

                }
            }

            options = null;
            scanner = null;
            result = null;
        }

        CameraResolution HandleCameraResolutionSelectorDelegate(List<CameraResolution> availableResolutions)
        {
            //Don't know if this will ever be null or empty
            if (availableResolutions == null || availableResolutions.Count < 1)
                return new CameraResolution { Width = 800, Height = 600 };

            //Debugging revealed that the last element in the list
            //expresses the highest resolution. This could probably be more thorough.
            return availableResolutions[availableResolutions.Count - 1];
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

            if (tableSource != null)
            {
                if (tableViewRef == null)
                {
                    tableViewRef = new WeakReference<UITableView>(StoreList);
                }
                tableSource.UpdateTableSource(tableViewRef, new List<StoreLocationDto>());
            }
            tableSource = null;
            if (TableView != null)
            {
                TableView.Source = null;
            }
            tableViewRef = null;
        }
        #endregion
    }
}