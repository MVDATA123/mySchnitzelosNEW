using Foundation;
using GCloudShared.Repository;
using GCloudShared.Shared;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using UIKit;

namespace GCloudiPhone
{
    public partial class InvitationCodeViewController : UIViewController
    {
        private readonly UserRepository _userRepository;
        public string invitationCode;
        private readonly UITapGestureRecognizer tap;

        public InvitationCodeViewController (IntPtr handle) : base (handle)
        {
            _userRepository = new UserRepository(DbBootstraper.Connection);
            tap = new UITapGestureRecognizer(DismissKeyboard);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var user = _userRepository.GetCurrentUser();
            EmailAddressTextLabel.Text = user.Username;
            InvitationCodeLabel.Text = invitationCode;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            View.AddGestureRecognizer(tap);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            View.RemoveGestureRecognizer(tap);
        }

        public async Task ShareInvitationCode()
        {
            var user = _userRepository.GetCurrentUser();

            string[] invitationCodes = invitationCode.Split(' ');
            string iCode = invitationCodes[1];
            //Unicode charachter for ¨¹
            char c1 = '\u00FC';
            string shareBodyText = user.Email + " schickt dir einen Freundes-Code @@" + "@@" + iCode + "@@" + "@@" + "f"+c1+"r die Anwendung: https://apps.apple.com/ca/app/myschnitzelpunktepass/id1572873813" + "@@" + "@@" +
                "Bitte diesen Code bei der Registrierung angeben.";
            shareBodyText = shareBodyText.Replace("@@", System.Environment.NewLine);
            //shareBodyText = shareBodyText.Replace(iCode, "*" + iCode + "*");

            await Share.RequestAsync(new ShareTextRequest
            {
                //Title = "Freundes-Code",
                //Text = user.Email + " schickt dir einen " + invitationCode + ", für die Anwendung: https://apps.apple.com/ca/app/myschnitzelpunktepass/id1572873813"

                Text = shareBodyText
        });
        }

        async partial void ShareInvitationCodeButton_TouchUpInside(UIButton sender)
        {
            await ShareInvitationCode();
        }

        private void DismissKeyboard()
        {
            View.EndEditing(true);
        }
    }
}