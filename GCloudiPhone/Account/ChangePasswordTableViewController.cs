using Foundation;
using System;
using UIKit;
using GCloudShared.Extensions;
using GCloudShared.Service;
using GCloudShared.Service.Dto;
using GCloudShared.Shared;
using Refit;
using CoreGraphics;
using GCloudShared.Service.WebShopRegisterServices;
using GCloudShared.Repository;
using GCloudShared.Domain;

namespace GCloudiPhone
{
    public partial class ChangePasswordTableViewController : UITableViewController
    {
        IAuthService authService;
        private IWebShopService webShopService;
        private readonly UserRepository _userRepository;
        private UITapGestureRecognizer tap;
        private User currentUser;

        public ChangePasswordTableViewController(IntPtr handle) : base(handle)
        {
            authService = RestService.For<IAuthService>(HttpClientContainer.Instance.HttpClient);
            webShopService = RestService.For<IWebShopService>("https://neunkirchen.myschnitzel.at/");
            
            _userRepository = new UserRepository(DbBootstraper.Connection);
            currentUser = _userRepository.GetCurrentUser();
            tap = new UITapGestureRecognizer(DismissKeyboard);
        }

        public override void ViewDidLoad()
        {
            //ChangePwTable.TableFooterView = new UIView(new CGRect(0, 0, 0, 0));
            //ChangePwTable.TableFooterView.BackgroundColor = UIColor.Clear;
            ChangePwTable.SectionFooterHeight = 0.0f;
            OldPwLabel.BecomeFirstResponder();

            base.ViewDidLoad();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            OldPwLabel.ShouldReturn = TextFieldShouldReturn;
            NewPwLabel.ShouldReturn = TextFieldShouldReturn;
            ConfirmNewPwLabel.ShouldReturn = TextFieldShouldReturn;

            View.AddGestureRecognizer(tap);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            View.RemoveGestureRecognizer(tap);

            OldPwLabel.ShouldReturn = null;
            NewPwLabel.ShouldReturn = null;
            ConfirmNewPwLabel.ShouldReturn = null;
        }

        public bool TextFieldShouldReturn(UITextField textField)
        {
            if (textField == OldPwLabel)
            {
                NewPwLabel.BecomeFirstResponder();
            }
            else if(textField == NewPwLabel)
            {
                ConfirmNewPwLabel.BecomeFirstResponder();
            }
            else
            {
                View.EndEditing(true);
            }

            return true;
        }

        partial void ChangePwBtn_Activated(UIBarButtonItem sender)
        {
            ToggleInput();
            ChangePassword();
        }

        private void DismissKeyboard()
        {
            View.EndEditing(true);
        }

        private void ToggleInput()
        {
            OldPwLabel.Enabled = !OldPwLabel.Enabled;
            NewPwLabel.Enabled = !NewPwLabel.Enabled;
            ConfirmNewPwLabel.Enabled = !ConfirmNewPwLabel.Enabled;
        }

        private async void ChangePassword()
        {
            try
            {
                if (!isModelValid())
                {
                    ToggleInput();
                    return;
                }

                await authService.ChangePassword(new ChangePasswordRequestModel()
                {
                    OldPassword = OldPwLabel.Text,
                    NewPassword = NewPwLabel.Text,
                    ConfirmPassword = ConfirmNewPwLabel.Text
                });

                await webShopService.ResetPasswordInWebShopFromGcloud(new RecoveryPasswordToWebShopModel
                {
                    Email = currentUser.Email,
                    NewPassword = NewPwLabel.Text,
                    Result = null
                });

                InvokeOnMainThread(() =>
                {
                    var alert = UIAlertController.Create("Password ändern erfolgreich!", "Dein Passwort wurde erfolgreich geändert.", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, action => NavigationController.PopViewController(true)));
                    PresentViewController(alert, animated: true, completionHandler: null);
                    ToggleInput();
                });
            }
            catch (ApiException apiException)
            {
                var errorModel = apiException.GetApiErrorResult();
                errorModel.Match(some =>
                {
                    InvokeOnMainThread(() =>
                    {
                        var alert = UIAlertController.Create("Password ändern fehlgeschlagen!", some.Message, UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                        PresentViewController(alert, animated: true, completionHandler: null);
                        ToggleInput();
                        NavigationController.PopViewController(true);
                    });
                }, () =>
                {
                    InvokeOnMainThread(() =>
                    {
                        var alert = UIAlertController.Create("Password ändern fehlgeschlagen!", "Es ist ein unbekannter Fehler aufgetreten.", UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                        PresentViewController(alert, animated: true, completionHandler: null);
                        ToggleInput();
                    });
                });

            }
        }

        private bool isModelValid()
        {
            if (string.IsNullOrWhiteSpace(OldPwLabel.Text))
            {
                var alert = UIAlertController.Create("Password ändern fehlgeschlagen!", "Das alte Password darf nicht leer sein.", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                PresentViewController(alert, animated: true, completionHandler: null);
                return false;
            }

            if (string.IsNullOrWhiteSpace(NewPwLabel.Text))
            {
                var alert = UIAlertController.Create("Password ändern fehlgeschlagen!", "Das neue Password darf nicht leer sein.", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                PresentViewController(alert, animated: true, completionHandler: null);
                return false;
            }

            if (string.IsNullOrWhiteSpace(ConfirmNewPwLabel.Text))
            {
                var alert = UIAlertController.Create("Password ändern fehlgeschlagen!", "Die Passwortwiederholung darf nicht leer sein.", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                PresentViewController(alert, animated: true, completionHandler: null);
                return false;
            }

            if (!ConfirmNewPwLabel.Text.Equals(NewPwLabel.Text))
            {
                var alert = UIAlertController.Create("Password ändern fehlgeschlagen!", "Das neue Passwort und die Passwortwiederholung stimmen nicht überein.", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                PresentViewController(alert, animated: true, completionHandler: null);
                return false;
            }

            return true;
        }
    }
}