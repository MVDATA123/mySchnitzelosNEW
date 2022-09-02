using System;
using GCloudShared.Extensions;
using GCloudShared.Service;
using GCloudShared.Service.Dto;
using GCloudShared.Shared;
using Refit;
using UIKit;


namespace GCloudiPhone
{
    public partial class ChangePasswordViewController : UIViewController
    {
        IAuthService authService;

        public ChangePasswordViewController(IntPtr handle) : base(handle)
        {
            authService = RestService.For<IAuthService>(HttpClientContainer.Instance.HttpClient);
        }

        partial void ChangePwBtn_TouchUpInside(UIButton sender)
        {
            ToggleInput();
            ChangePassword();
        }

        private void ToggleInput()
        {
            OldPwLabel.Enabled = !OldPwLabel.Enabled;
            NewPwLabel.Enabled = !NewPwLabel.Enabled;
            ConfirmNewPwLabel.Enabled = !ConfirmNewPwLabel.Enabled;
            ChangePwBtn.Hidden = !ChangePwBtn.Hidden;
            LoadingIndicator.Hidden = !LoadingIndicator.Hidden;
        }

        private async void ChangePassword()
        {
            try
            {
                if(!IsModelValid()) {
                    ToggleInput();
                    return;   
                }
                
                await authService.ChangePassword(new ChangePasswordRequestModel()
                {
                    OldPassword = OldPwLabel.Text,
                    NewPassword = NewPwLabel.Text,
                    ConfirmPassword = ConfirmNewPwLabel.Text
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

        private bool IsModelValid() {
            if(string.IsNullOrWhiteSpace(OldPwLabel.Text)) {
                var alert = UIAlertController.Create("Password ändern fehlgeschlagen!", "Das alte Password darf nicht leer sein.", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                PresentViewController(alert, animated: true, completionHandler: null);
                return false;
            }

            if(string.IsNullOrWhiteSpace(NewPwLabel.Text)) {
                var alert = UIAlertController.Create("Password ändern fehlgeschlagen!", "Das neue Password darf nicht leer sein.", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                PresentViewController(alert, animated: true, completionHandler: null);
                return false;
            }

            if(string.IsNullOrWhiteSpace(ConfirmNewPwLabel.Text)) {
                var alert = UIAlertController.Create("Password ändern fehlgeschlagen!", "Die Passwortwiederholung darf nicht leer sein.", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                PresentViewController(alert, animated: true, completionHandler: null);
                return false;
            }

            if(!ConfirmNewPwLabel.Text.Equals(NewPwLabel.Text)) {
                var alert = UIAlertController.Create("Password ändern fehlgeschlagen!", "Das neue Passwort und die Passwortwiederholung stimmen nicht überein.", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                PresentViewController(alert, animated: true, completionHandler: null);
                return false;
            }

            return true;
        }
    }
}