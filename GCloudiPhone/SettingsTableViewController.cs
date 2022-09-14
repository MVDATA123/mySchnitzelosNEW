using System;
using System.IO;
using System.Threading.Tasks;
using Foundation;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using MessageUI;
using Newtonsoft.Json;
using Refit;
using UIKit;
using Xamarin.Essentials;

namespace GCloudiPhone
{
    public partial class SettingsTableViewController : UITableViewController
    {
        private readonly UserRepository _userRepository = new UserRepository(DbBootstraper.Connection);
        private readonly IAuthService _authService;
        private readonly LogRepository _logRepository = new LogRepository(DbBootstraper.Connection);

        MFMailComposeViewController mailController;

        public SettingsTableViewController(IntPtr handle) : base(handle)
        {
            _userRepository = new UserRepository(DbBootstraper.Connection);
            _authService = RestService.For<IAuthService>(HttpClientContainer.Instance.HttpClient);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            SettingsTable.SectionFooterHeight = 0.0f;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            LoginButton.RemoveFromSuperview();
            LogoutButton.RemoveFromSuperview();

            if (((AppDelegate)UIApplication.SharedApplication.Delegate).AuthState == AuthState.Unauthorized)
            {
                ChangePasswordCell.UserInteractionEnabled = false;
                LoginButton.TouchUpInside += LoginButton_TouchUpInside;
                LogoutCell.AddSubview(LoginButton);
                EmailLabel.Text = "";
                //UsernameLabel.Text = "Hier werden Kontodetails angezeigt";
                ChangePasswordCell.Accessory = UITableViewCellAccessory.None;
                ChangePasswordLabel.TextColor = UIColor.LightGray;
                //UsernameLabel.TextColor = UIColor.LightGray;
                InvitationCodeLabel.Text = "Einladungs-Code: N/A";
                
            }
            else
            {
                ChangePasswordCell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                ChangePasswordLabel.TextColor = UIColor.Black;
                //UsernameLabel.TextColor = UIColor.Black;
                var user = _userRepository.GetCurrentUser();
                //UsernameLabel.Text = user.Username;
                //UsernameLabel.Text = "Benutzer";
                EmailLabel.Text = user.Email;
                ChangePasswordCell.UserInteractionEnabled = true;
                LogoutButton.TouchUpInside += LogoutButton_TouchUpInside;
                LogoutCell.AddSubview(LogoutButton);
                InvitationCodeLabel.Text = user.InvitationCode;
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            LogoutButton.TouchUpInside -= LogoutButton_TouchUpInside;
            LoginButton.TouchUpInside -= LoginButton_TouchUpInside;
        }

        private void LogoutButton_TouchUpInside(object sender, EventArgs args)
        {
            Logout();
        }

        private void LoginButton_TouchUpInside(object sender, EventArgs args)
        {
            PerformSegue("LoginSegue", this);
        }

        public void Logout()
        {
            ((TabBarController)TabBarController).Logout(this);
        }

        //partial void SendDiagnosticsBtn_TouchUpInside(UIButton sender)
        //{
        //    if(MFMailComposeViewController.CanSendMail)
        //    {
        //        mailController = new MFMailComposeViewController();

        //        mailController.SetToRecipients(new string[] { "support@mv-data.at" });
        //        mailController.SetSubject("FoodJet-iOS Diagnoseinformationen");
        //        var logs = _logRepository.FindAll();
        //        var logJson = JsonConvert.SerializeObject(logs);

        //        var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
        //        var filename = Path.Combine (documents, "DiagnosticsInformation.json");
        //        File.WriteAllText(filename, logJson);
        //        mailController.AddAttachmentData(NSData.FromStream(new FileStream(filename, FileMode.Open)), "application/json", "DiagnosticsInformation.json");

        //        mailController.Finished += (object s, MFComposeResultEventArgs e) =>
        //        {
        //            e.Controller.DismissViewController(true, null);
        //            File.Delete(filename);
        //        };

        //        this.PresentViewController(mailController, true, null);
        //    }
        //}


        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            if (segue.Identifier == "ImprintSegue")
            {
                var webViewController = segue.DestinationViewController as SettingsWebViewController;
                webViewController.Type = "Imprint";
            }
            else if (segue.Identifier == "DataProtectionSegue")
            {
                var webViewController = segue.DestinationViewController as SettingsWebViewController;
                webViewController.Type = "DataProtection";
            }
            else if (segue.Identifier == "TermsAndConditionSegue")
            {
                var webViewController = segue.DestinationViewController as SettingsWebViewController;
                webViewController.Type = "TC";
            }
            base.PrepareForSegue(segue, sender);
        }

        public async Task ShareInvitationCode()
        {
            var user = _userRepository.GetCurrentUser();


            string iCode = InvitationCodeLabel.Text;
            //Unicode charachter for ü
            char c1 = '\u00FC';
            string shareBodyText = user.Email + " schickt dir einen Freundes-Code @@" + "@@" + iCode + "@@" + "@@" + "f" + c1 + "r die Anwendung: https://apps.apple.com/ca/app/myschnitzelpunktepass/id1572873813" + "@@" + "@@" +
                "Bitte diesen Code bei der Registrierung angeben.";
            shareBodyText = shareBodyText.Replace("@@", System.Environment.NewLine);
            //shareBodyText = shareBodyText.Replace(iCode, "*" + iCode + "*");

            await Share.RequestAsync(new ShareTextRequest
            {
                //Title = "Freundes-Code",
                //Text = user.Email + " schickt dir einen " + invitationCode + ", f黵 die Anwendung: https://apps.apple.com/ca/app/myschnitzelpunktepass/id1572873813"

                Text = shareBodyText
            });
        }

        async partial void ShareInvitationCodeButton_TouchUpInside(UIButton sender)
        {
            await ShareInvitationCode();
        }


    }
}