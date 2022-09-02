using Foundation;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Refit;
using System;
using UIKit;

namespace GCloudiPhone
{
    public partial class TotalPointsViewController : UIViewController
    {
        private readonly UserRepository _userRepository;
        private readonly IAuthService _authService;

        public TotalPointsViewController (IntPtr handle) : base (handle)
        {
            _userRepository = new UserRepository(DbBootstraper.Connection);
            _authService = RestService.For<IAuthService>(HttpClientContainer.Instance.HttpClient);
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            if (segue.Identifier == "invitationCodeSegue")
            {
                var invitationCodeViewController = segue.DestinationViewController as InvitationCodeViewController;
                invitationCodeViewController.invitationCode = InvitationCodeLabel.Text; 
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (((AppDelegate)UIApplication.SharedApplication.Delegate).AuthState == AuthState.Unauthorized)
            {
                TotalPointsLabel.Text = "Punkte gesamt: N/A";
                InvitationCodeLabel.Text = "Einladungs-Code: N/A";
                freundeWerbenButton.Hidden = true;
            }
            else
            {
                var user = _userRepository.GetCurrentUser();
                var totalPoints = _authService.GetTotalPointsByUserID(user.UserId).Result;
                InvitationCodeLabel.Text = "Einladungs-Code: " + user.InvitationCode;
                TotalPointsLabel.Text = "Punkte gesamt: " + totalPoints.Replace("\"", "");
                freundeWerbenButton.Hidden = false;
            }
        }
    }
}