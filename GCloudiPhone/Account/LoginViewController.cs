using System;
using System.Linq;
using System.Net;
using GCloudiPhone.Helpers;
using GCloudiPhone.Shared;
using GCloudShared;
using GCloudShared.Domain;
using GCloudShared.Extensions;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Refit;
using UIKit;
using Foundation;
using Firebase.InstanceID;

namespace GCloudiPhone
{
    public partial class LoginViewController : UIViewController, ICanCleanUpMyself
    {
        private IAuthService _authService;
        private UserRepository _userRepository;
        private MobilePhoneRepository _mobilePhoneRepository;
        private LogRepository logRepository;

        private IDisposable loginHandler;
        private UITapGestureRecognizer tap;

        //NSUserDefaults storevalues = new NSUserDefaults();

        public LoginViewController(IntPtr handle) : base(handle)
        {
            tap = new UITapGestureRecognizer(DismissKeyboard);
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();


            _authService = RestService.For<IAuthService>(HttpClientContainer.Instance.HttpClient);
            _userRepository = new UserRepository(DbBootstraper.Connection);
            _mobilePhoneRepository = new MobilePhoneRepository(DbBootstraper.Connection);
            logRepository = new LogRepository(DbBootstraper.Connection);

            // storevalues.SetString(PasswordText.Text, "stringvalue");
            //Prenosimo vrednost passworda na WebShopViewController
            //CommonClass.value = PasswordText.Text;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            View.AddGestureRecognizer(tap);

            UsernameText.ShouldReturn = TextFieldShouldReturn;
            PasswordText.ShouldReturn = TextFieldShouldReturn;
        }

        public override void ViewDidDisappear(bool animated)
        {
            View.RemoveGestureRecognizer(tap);

            UsernameText.ShouldReturn = null;
            PasswordText.ShouldReturn = null;

            if ((NavigationController == null && IsMovingFromParentViewController) || (ParentViewController != null && ParentViewController.IsBeingDismissed))
            {
                MemoryUtility.ReleaseUIViewWithChildren(this.View);
            }

            base.ViewDidDisappear(animated);
        }

        public bool TextFieldShouldReturn(UITextField textField)
        {
            if (textField == UsernameText)
            {
                PasswordText.BecomeFirstResponder();
            }
            else
            {
                View.EndEditing(true);
            }

            return true;
        }

        private void DismissKeyboard()
        {
            View.EndEditing(true);
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

        partial void LoginButton_TouchUpInside(UIButton sender)
        {
            ToggleInputs();
            PerformLogin();
        }

        private void PerformLogin()
        {
            var mobilePhone = _mobilePhoneRepository.FirstOrDefault();

            try
            {
                loginHandler = _authService.Login(new LoginRequestModel
                {
                    Username = UsernameText.Text,
                    Password = PasswordText.Text,
                    
                // storevalues.SetString(PasswordText.Text, "stringvalue");
                DeviceId = mobilePhone?.MobilePhoneId,
                    FirebaseInstanceId = InstanceId.SharedInstance.Token
                }).Subscribe(response =>
                {
                    if (response != null)
                    {
                        //ovo ispod zakomentarisati kada radimo na lokalu
                        var cookies = HttpClientContainer.Instance.CookieContainer.GetCookies(UriContainer.BasePath)
                            .Cast<Cookie>().ToList();
                        PersistUser(new User
                        {
                            //ovo ispod zakomentarisati kada radimo na lokalu
                            AuthToken = cookies.FirstOrDefault(x => x.Name == ".AspNet.ApplicationCookie")?.Value,
                            //ovo ispod odkomentarisati kada radimo na lokalu
                            //AuthToken = "J5aeY7pqOsHD6-onpsWtovDl-1ltAVr2NRkX2HtjumylL6a6lywbnRge5wmTz0X5_1tR7AB79QxiBndxfz1upqquLT73KpRDfatDzOU0FeC0QsJPsX_-OYPYs6UdZ2HsDp8yyl35jAsIDW7eVLXfgqIJLpCmreRzF2hFA6Dr5wJZfEZQLKHW7Hpf7yAvTcA-0IfOscss9KVs7ozsBOgG3HHSRDiS_AGU57WPwzwB8YkYdzsu0pHlT1vm8YPb9Ozvd4r2JDIcz0aXR-Py8JzDg8wnSsPyi8CffNQ5_zqIRtXIjnIZcRPaUzQCcfIjcj5YQAO2ovw7AsMkzpsnv-wGDSLP-HyVqJzF0r6VqTasWaRyYdmpjYASoLjDZ-tmp23m",
                            //mi smo dodali ovo ispod
                            //RoleName = "Managers",
                            //ovo ispod zakomentarisati kada radimo na lokalu
                            RoleName = response.Role,
                            UserId = response.UserId,
                            Email = response.Email,
                            Username = response.Username,
                            UserLoginMethod = UserLoginMethod.Normal,
                            AuthTokenDate = DateTime.Now,
                            InvitationCode = response.InvitationCode,
                            //TotalPoints = response.TotalPoints
                        });
                        PersistMobilePhoneId(new MobilePhone
                        {
                            MobilePhoneId = response.MobilePhoneGuid.ToString()
                        });

                        Caching.CachingService.UpdateStores();

                        InvokeOnMainThread(() =>
                        {
                            DismissViewController(true, null);
                            ((AppDelegate)UIApplication.SharedApplication.Delegate).AuthState = AuthState.Authorized;
                            NotificationsHelper.Instance.SubscribeAll();
                        });
                    }
                   
                }, ex =>
                {
                    if (ex is ApiException apiException)
                    {
                        InvokeOnMainThread(() =>
                        {
                            var errorModel = apiException.GetApiErrorResult();
                            errorModel.Match(some =>
                            {
                                using (var alertController = UIAlertController.Create("Login fehlgeschlagen!", some.Message, UIAlertControllerStyle.Alert))
                                using (var okAction = UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, action => ToggleInputs()))
                                {
                                    alertController.AddAction(okAction);
                                    PresentViewController(alertController, true, null);
                                }
                            }, () =>
                            {
                                using (var alertController = UIAlertController.Create("Login fehlgeschlagen!", "Es ist ein unbekannter Fehler aufgetreten.", UIAlertControllerStyle.Alert))
                                using (var okAction = UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, action => ToggleInputs()))
                                {
                                    alertController.AddAction(okAction);
                                    PresentViewController(alertController, true, null);
                                }

                            });
                        });
                    }
                    else
                    {
                        InvokeOnMainThread(() =>
                        {
                            using (var alertController = UIAlertController.Create("Fehler!", "Es ist ein Fehler aufgetreten. Bitte versuche es erneut", UIAlertControllerStyle.Alert))
                            using (var okAction = UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, action => ToggleInputs()))
                            {
                                alertController.AddAction(okAction);
                                PresentViewController(alertController, true, null);
                            }
                        });
                    }
                }, ToggleInputs);
            }
            catch (Exception e)
            {
                logRepository.Insert(new LogMessage
                {
                    Level = LogLevel.ERROR,
                    Message = e.Message,
                    StackTrace = e.StackTrace,
                    TimeStamp = DateTime.Now
                });

                using (var alertController = UIAlertController.Create("Login fehlgeschlagen!", "Der Login ist fehlgeschlagen, bitte versuche es erneut", UIAlertControllerStyle.Alert))
                using (var okAction = UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null))
                {
                    alertController.AddAction(okAction);
                    PresentViewController(alertController, true, null);
                }
                ToggleInputs();
            }
            CommonClass.value = PasswordText.Text;
        }

        private void PersistUser(User user)
        {
            var currentStoredUsers = _userRepository.Count();

            if (currentStoredUsers > 0)
            {
                _userRepository.DeleteAll();
            }
            _userRepository.Insert(user);
        }

        private void PersistMobilePhoneId(MobilePhone phone)
        {
            var currentMobilePhones = _userRepository.Count();

            if (currentMobilePhones > 0)
            {
                _mobilePhoneRepository.DeleteAll();
            }
            _mobilePhoneRepository.Insert(phone);
        }

        private void ToggleInputs()
        {
            LoginButton.Enabled = !LoginButton.Enabled;
            RegisterButton.Enabled = !RegisterButton.Enabled;
            UsernameText.Enabled = !UsernameText.Enabled;
            PasswordText.Enabled = !PasswordText.Enabled;

            LoginButton.Hidden = !LoginButton.Hidden;
            RegisterButton.Hidden = !RegisterButton.Hidden;
            ForgotPasswordBtn.Hidden = !ForgotPasswordBtn.Hidden;

            if (LoginButton.Enabled)
            {
                LoginAcitivityIndicator.StopAnimating();
            }
            else
            {
                LoginAcitivityIndicator.StartAnimating();
            }
        }

        partial void CancelButton_Activated(UIBarButtonItem sender)
        {
            DismissViewController(true, null);
        }



        public void CleanUp()
        {
            ReleaseDesignerOutlets();
            _authService = null;
            _userRepository = null;
            _mobilePhoneRepository = null;
            loginHandler.Dispose();
            loginHandler = null;
        }
    }
}