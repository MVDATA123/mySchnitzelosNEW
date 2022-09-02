using System;
using Foundation;
using GCloudShared.Domain;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Refit;
using UIKit;

namespace GCloudiPhone
{
    public partial class ForgotPasswortViewController : UIViewController
    {
        private readonly IAuthService authService;
        private readonly LogRepository logRepository;

        public ForgotPasswortViewController(IntPtr handle) : base(handle)
        {
            authService = RestService.For<IAuthService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
            logRepository = new LogRepository(DbBootstraper.Connection);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            View.EndEditing(true);
        }

        partial void SendButton_TouchUpInside(UIButton sender)
        {
            ToggleInputs();
            var inputString = UsernameEmailTextField.Text;
            if (string.IsNullOrWhiteSpace(inputString))
            {
                using (var failureAlert = UIAlertController.Create("Eingabe ist leer", "Bitte gib deine Benutzername oder deine E-Mail Adresse ein.", UIAlertControllerStyle.Alert))
                using (var okAction = UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null))
                {
                    failureAlert.AddAction(okAction);
                    this.PresentViewController(failureAlert, true, null);
                }
                ToggleInputs();
                return;
            }

            RequestResetLink(inputString);
        }

        private async void RequestResetLink(string usernameEmail)
        {
            try
            {
                var success = await authService.ForgotPassword(usernameEmail);

                if (success)
                {
                    using (var successAlert = UIAlertController.Create("Link erfolgreich gesendet", "Dein Passwort-zurücksetzten-Link wurde erfolgreich gesendet. Bitte kontrolliere auch deinen Spam-/Junk-E-Mail Ordner.", UIAlertControllerStyle.Alert))
                    using (var okAction = UIAlertAction.Create("Ok", UIAlertActionStyle.Default, obj => this.NavigationController.PopToRootViewController(true)))
                    {
                        successAlert.AddAction(okAction);
                        this.PresentViewController(successAlert, true, null);
                    }
                }
                else
                {
                    using (var failureAlert = UIAlertController.Create("Link senden fehlgeschlagen", "Beim Senden ist eine Fehler aufgetreten. Bitte kontrolliere deine Internetverbindung und versuche es erneut", UIAlertControllerStyle.Alert))
                    using (var okAction = UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null))
                    {
                        failureAlert.AddAction(okAction);
                        this.PresentViewController(failureAlert, true, null);
                    }
                }
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

                using (var failureAlert = UIAlertController.Create("Link senden fehlgeschlagen", "Beim Senden ist eine Fehler aufgetreten. Bitte kontrolliere deine Internetverbindung und versuche es erneut", UIAlertControllerStyle.Alert))
                using (var okAction = UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null))
                {
                    failureAlert.AddAction(okAction);
                    this.PresentViewController(failureAlert, true, null);
                }
            }
            finally
            {
                ToggleInputs();
            }
        }

        private void ToggleInputs()
        {
            UsernameEmailTextField.Enabled = !UsernameEmailTextField.Enabled;
            SendButton.Hidden = !SendButton.Hidden;

            if (SendButton.Hidden)
            {
                SendActivityIndicator.StartAnimating();
            }
            else
            {
                SendActivityIndicator.StopAnimating();
            }
        }
    }
}