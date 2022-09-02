using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Firebase.CloudMessaging;
using Foundation;
using GCloud.Shared;
using GCloud.Shared.Dto;
using GCloudiPhone.Caching;
using GCloudiPhone.Helpers;
using GCloudiPhone.Shared;
using GCloudShared.Domain;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using Refit;
using UIKit;
using UserNotifications;

namespace GCloudiPhone
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate, IMessagingDelegate, IInternetState
    {
        // class-level declarations

        public override UIWindow Window { get; set; }
        public NetworkState State { get; set; }
        public AuthState AuthState { get; set; }

        private readonly UserRepository _userRepository;
        public LogRepository _logRepository;
        private readonly IStartupService _startupService;
        private readonly IUserStoreService _storeService;
        private UIApplicationState state;

        public AppDelegate(IntPtr handle) : base(handle)
        {
            _userRepository = new UserRepository(DbBootstraper.Connection);
            _logRepository = new LogRepository(DbBootstraper.Connection);
            _startupService = RestService.For<IStartupService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
            _storeService = RestService.For<IUserStoreService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method
            Window = new UIWindow(UIScreen.MainScreen.Bounds);
            state = UIApplicationState.Active;

            if (CrossConnectivity.Current.IsConnected)
            {
                var connectionType = CrossConnectivity.Current.ConnectionTypes.FirstOrDefault();
                State = connectionType == ConnectionType.Cellular ? NetworkState.ConnectedData : NetworkState.ConnectedWifi;
            }
            else
            {
                State = NetworkState.Disconnected;
            }

            HttpClientContainer.Instance.SetInternetState(this);

            var oldLogin = GetOldLogin();
            if (oldLogin != null && (oldLogin.AuthTokenDate - DateTime.Now).TotalDays < 30)
            {
                var cookie = new Cookie(".AspNet.ApplicationCookie", oldLogin.AuthToken);
                HttpClientContainer.Instance.CookieContainer.Add(UriContainer.BasePath, cookie);
                AuthState = AuthState.Authorized;
            }
            else
            {
                AuthState = AuthState.Unauthorized;
            }
            oldLogin = null;

            try
            {
                Task.Run(async () => await CachingService.LoadCache()).Wait();
            }
            catch (Exception e)
            {
                _logRepository.Insert(new LogMessage
                {
                    Level = LogLevel.WARNING,
                    Message = e.Message,
                    StackTrace = e.StackTrace,
                    TimeStamp = DateTime.Now
                });
            }

            using (var board = UIStoryboard.FromName("Main", null))
            {
                Window.RootViewController = board.InstantiateInitialViewController();
            }

            UITextField.Appearance.TintColor = UIColor.FromRGB(255, 87, 34);

            SetupRemoteNotifications();

            Window.MakeKeyAndVisible();

#if DEBUG
            _logRepository.Insert(new LogMessage
            {
                Level = LogLevel.DEBUG,
                Message = "Successfully launched App",
                TimeStamp = DateTime.Now
            });
#endif

            return true;
        }

        private void SetupRemoteNotifications()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
                UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (granted, error) =>
                {
                    Console.WriteLine(granted);
                });

                UNUserNotificationCenter.Current.Delegate = new CustomUserNotificationCenterDelegate();

                Messaging.SharedInstance.Delegate = UIApplication.SharedApplication.Delegate as AppDelegate;
            }
            else
            {
                var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
                using (var settings = UIUserNotificationSettings.GetSettingsForTypes(allNotificationTypes, new NSSet()))
                {
                    UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
                }
            }

            Firebase.Core.App.Configure();
            UIApplication.SharedApplication.RegisterForRemoteNotifications();

            Firebase.InstanceID.InstanceId.Notifications.ObserveTokenRefresh(UpdateFirebaseToken);
        }

        public void UpdateFirebaseToken(object sender, EventArgs e)
        {
            //now we could do something with the FCM-Token

#if DEBUG
            _logRepository.Insert(new LogMessage
            {
                Level = LogLevel.DEBUG,
                Message = "Successfully received Firebase-Token",
                TimeStamp = DateTime.Now
            });
#endif
        }

        private User GetOldLogin()
        {
            var usercount = _userRepository.Count();

            if (usercount > 0)
            {
                var user = _userRepository.FindAll().First();
                if (user.AuthToken == null)
                {
                    _userRepository.DeleteAll();
                    return null;
                }
                return user;
            }

            return null;
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            using (var alert = UIAlertController.Create("Fehler", error.LocalizedDescription, UIAlertControllerStyle.Alert))
            {
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                Window.RootViewController.PresentViewController(alert, true, null);
            }

            _logRepository.Insert(new LogMessage
            {
                Level = LogLevel.ERROR,
                Message = "Failed to receive Firebase-Token",
                TimeStamp = DateTime.Now
            });
        }

        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            if (_logRepository == null)
            {
                try
                {
                    _logRepository = new LogRepository(DbBootstraper.Connection);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            if (_logRepository != null)
            {
                _logRepository.Insert(new LogMessage
                {
                    Level = LogLevel.INFORMATION,
                    Message = "Did receive remote notification",
                    StackTrace = userInfo.ToString(),
                    TimeStamp = DateTime.Now
                });
            }
            try
            {
                if (userInfo.ContainsKey(new NSString("aps")) && (userInfo["aps"] as NSDictionary).ContainsKey(new NSString("content-available")) && int.Parse((userInfo["aps"] as NSDictionary)["content-available"].ToString()) == 1)
                {
                    UIApplication.SharedApplication.ApplicationIconBadgeNumber = UIApplication.SharedApplication.ApplicationIconBadgeNumber + 1;
                    if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
                    {
                        try
                        {
                            using (var center = UNUserNotificationCenter.Current)
                            using (var storeNotification = new UNMutableNotificationContent())
                            {
                                if (userInfo.ContainsKey(new NSString("type")) && userInfo["type"]?.ToString() == "bill")
                                {
                                    storeNotification.Title = $"{userInfo["title"]?.ToString()}";
                                }
                                else
                                {
                                    storeNotification.Title = $"{userInfo["title"]?.ToString()} - {userInfo["storeName"]?.ToString()}";
                                }
                                storeNotification.Sound = UNNotificationSound.Default;
                                storeNotification.ThreadIdentifier = "FoodJet";
                                storeNotification.Body = userInfo["body"]?.ToString();
                                storeNotification.UserInfo = userInfo;

                                using (var request = UNNotificationRequest.FromIdentifier(Guid.NewGuid().ToString(), storeNotification, null))
                                {
                                    center.AddNotificationRequest(request, (err) =>
                                    {
                                        if (err != null)
                                        {
                                            _logRepository.Insert(new LogMessage
                                            {
                                                Level = LogLevel.WARNING,
                                                Message = err.LocalizedDescription,
                                                TimeStamp = DateTime.Now
                                            });
                                        }
                                    });

#if DEBUG
                                    if (_logRepository != null)
                                    {
                                        _logRepository.Insert(new LogMessage
                                        {
                                            Level = LogLevel.DEBUG,
                                            Message = "Successfully created Notification-Request",
                                            TimeStamp = DateTime.Now
                                        });
                                    }
#endif
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (_logRepository != null)
                            {
                                _logRepository.Insert(new LogMessage
                                {
                                    Level = LogLevel.ERROR,
                                    Message = e.Message,
                                    StackTrace = e.StackTrace,
                                    TimeStamp = DateTime.Now
                                });
                            }
                        }
                    }
                    else
                    {
                        using (var notification = new UILocalNotification
                        {
                            AlertTitle = $"{userInfo["title"]?.ToString()} - {userInfo["storeName"]?.ToString()}",
                            AlertBody = userInfo["body"]?.ToString(),
                            SoundName = UILocalNotification.DefaultSoundName,
                            UserInfo = userInfo
                        })
                        {
                            UIApplication.SharedApplication.ScheduleLocalNotification(notification);
                        }
                    }
                }
                else
                {

                    var storeGuid = userInfo["storeGuid"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(storeGuid))
                    {
                        if (FilterSidebarViewController.Instance.TryGetTarget(out var filterSidebar))
                        {
                            if (filterSidebar.MainTabBarControllerRef.TryGetTarget(out var tabBarController))
                            {
                                tabBarController.TabBar.Hidden = false;
                                tabBarController.ChangeSelectedItem(1);
                                var navigationController = (tabBarController.ViewControllers[1] as UINavigationController);
                                navigationController.PopToRootViewController(false);
                                var storeListViewController = navigationController.VisibleViewController as StoreListViewController;
                                PerformStoreSegue(storeGuid, storeListViewController);

                                tabBarController = null;
                                navigationController = null;
                                filterSidebar = null;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (_logRepository != null)
                {
                    _logRepository.Insert(new LogMessage
                    {
                        Level = LogLevel.ERROR,
                        Message = e.Message,
                        StackTrace = e.StackTrace,
                        TimeStamp = DateTime.Now
                    });
                }
            }

        }

        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            var type = notification.UserInfo["type"]?.ToString();
            if (type != null && type == "bill")
            {
                var billId = notification.UserInfo["billId"]?.ToString();
                if (billId == null)
                {
                    return;
                }

                using (var board = UIStoryboard.FromName("Main", null))
                using (var invoiceDetailsController = board.InstantiateViewController("InvoiceDetailsViewController") as InvoiceDetailsViewController)
                {
                    var billService = RestService.For<IBillService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
                    invoiceDetailsController.Invoice = billService.GetById(Guid.Parse(billId)).Result.Invoice;
                    if (FilterSidebarViewController.Instance.TryGetTarget(out var filterSidebar))
                    {
                        if (filterSidebar.MainTabBarControllerRef.TryGetTarget(out var tabBarController))
                        {
                            ((UINavigationController)tabBarController.SelectedViewController).PushViewController(invoiceDetailsController, true);
                        }
                    }
                }

            }

            //storeNotification = null;
            //var storeGuid = notification.UserInfo["storeGuid"]?.ToString();
            //if (!string.IsNullOrWhiteSpace(storeGuid))
            //{
            //    var tabBarController = (window.RootViewController as RootViewController).MainTabBarController;
            //    if (tabBarController != null)
            //    {
            //        tabBarController.TabBar.Hidden = false;
            //        tabBarController.SelectedIndex = 1;
            //        var storeListViewController = (tabBarController.ViewControllers[2] as UINavigationController).VisibleViewController as StoreListViewController;
            //        PerformStoreSegue(storeGuid, storeListViewController);
            //    }
            //}

        }

        private void PerformStoreSegue(string storeGuid, StoreListViewController storeListViewController)
        {
            var store = CacheHolder.Instance.GetStoreByGuid(Guid.Parse(storeGuid));
            storeListViewController.PerformSegue("StoreDetailSegue", NSObjectWrapper.Wrap(store.ValueOr(new Extensions.StoreLocationDto())));
        }

        public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
            state = UIApplicationState.Inactive;

            _logRepository.Insert(new LogMessage
            {
                Level = LogLevel.INFORMATION,
                Message = "App-State changed to INACTIVE",
                TimeStamp = DateTime.Now
            });
        }

        public override void WillTerminate(UIApplication application)
        {
            _logRepository.Insert(new LogMessage
            {
                Level = LogLevel.WARNING,
                Message = "App-State changed to TERMINATED",
                TimeStamp = DateTime.Now
            });
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background execution this method is called instead of WillTerminate when the user quits.

            _logRepository.Insert(new LogMessage
            {
                Level = LogLevel.INFORMATION,
                Message = "App-State changed to BACKGROUND",
                TimeStamp = DateTime.Now
            });

            state = UIApplicationState.Background;
            CrossConnectivity.Current.ConnectivityTypeChanged -= UpdateNetworkStatus;

            CachingService.PersistCache();

            if (FilterSidebarViewController.Instance.TryGetTarget(out var filterSidebarController))
            {
                if (filterSidebarController.MainTabBarControllerRef.TryGetTarget(out var tabBarController))
                {
                    foreach (var viewController in tabBarController.ViewControllers)
                    {
                        if (viewController is ICanCleanUpMyself cleanUpMyself)
                        {
                            cleanUpMyself.CleanUp();
                        }
                    }
                }
            }

            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;

            Cookie cookie = null;
            var cookies = HttpClientContainer.Instance.CookieContainer.GetCookies(BaseUrlContainer.BaseUri);
            for (var i = 0; i < cookies.Count; i++)
            {
                if (cookies[i].Name == ".AspNet.ApplicationCookie")
                {
                    cookie = cookies[i];
                    break;
                }
            }

            var user = _userRepository.GetCurrentUser();
            if (cookie != null && user != null)
            {
                user.AuthToken = cookie.Value;
                user.AuthTokenDate = DateTime.Now;
                _userRepository.DeleteAll();
                _userRepository.Insert(user);
            }
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.

            if (state == UIApplicationState.Background)
            {
                if (CrossConnectivity.Current.IsConnected)
                {
                    var connectionType = CrossConnectivity.Current.ConnectionTypes.FirstOrDefault();
                    State = connectionType == ConnectionType.Cellular ? NetworkState.ConnectedData : NetworkState.ConnectedWifi;
                }
                else
                {
                    State = NetworkState.Disconnected;
                }

                HttpClientContainer.Instance.SetInternetState(this);

                var oldLogin = GetOldLogin();
                if (oldLogin != null && (oldLogin.AuthTokenDate - DateTime.Now).TotalDays < 30)
                {
                    var cookie = new Cookie(".AspNet.ApplicationCookie", oldLogin.AuthToken);
                    HttpClientContainer.Instance.CookieContainer.Add(UriContainer.BasePath, cookie);
                    AuthState = AuthState.Authorized;
                }
                else
                {
                    AuthState = AuthState.Unauthorized;
                }
                oldLogin = null;

                Task.Run(async () => await CachingService.LoadCache()).Wait();
                CrossConnectivity.Current.ConnectivityTypeChanged += UpdateNetworkStatus;

                if (FilterSidebarViewController.Instance != null && FilterSidebarViewController.Instance.TryGetTarget(out var sideBarController))
                {
                    if (sideBarController.MainTabBarControllerRef != null && sideBarController.MainTabBarControllerRef.TryGetTarget(out var tabBarController))
                    {
                        tabBarController.SelectedViewController.ViewWillAppear(true);
                    }
                }
            }

        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.

            state = application.ApplicationState;

            _logRepository.Insert(new LogMessage
            {
                Level = LogLevel.INFORMATION,
                Message = "App-State changed to ACTIVE",
                TimeStamp = DateTime.Now
            });
        }

        private async void UpdateNetworkStatus(object sender, ConnectivityTypeChangedEventArgs e)
        {
            var result = await CrossConnectivity.Current.IsRemoteReachable(BaseUrlContainer.BaseUrlHost, BaseUrlContainer.BaseUrlPort);
            if (e.IsConnected && result)
            {
                var types = CrossConnectivity.Current.ConnectionTypes;
                var connectionType = types.FirstOrDefault();
                State = connectionType == ConnectionType.Cellular ? NetworkState.ConnectedData : NetworkState.ConnectedWifi;
            }
            else
            {
                State = NetworkState.Disconnected;
                ShowNoInternetMessage();
            }
            HttpClientContainer.Instance.SetInternetState(this);
        }

        public void ShowNoInternetMessage()
        {
            _logRepository.Insert(new LogMessage
            {
                Level = LogLevel.WARNING,
                Message = "No internet connection available",
                TimeStamp = DateTime.Now
            });

            using (var noInternetAlert = UIAlertController.Create("Keine Internetverbindung", null, UIAlertControllerStyle.ActionSheet))
            {
                noInternetAlert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                Window.RootViewController?.PresentViewController(noInternetAlert, true, null);
            }
        }

        public void OnShowNoInternetMessageSuccess()
        {
            throw new NotImplementedException();
        }

        public void LogoffRedirectToLogin()
        {
            AuthState = AuthState.Unauthorized;
            InvokeOnMainThread(() =>
            {
                using (var board = UIStoryboard.FromName("Main", null))
                using (var loginViewController = board.InstantiateViewController("LoginNavigationController"))
                {
                    Window.RootViewController.PresentViewController(loginViewController, true, null);
                }
            });
        }
    }
}