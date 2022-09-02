using System;
using System.Collections.Generic;
using System.Text;

namespace GCloudShared
{
    public class LoginResponseModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string AuthToken { get; set; }
        public Guid MobilePhoneGuid { get; set; }
        public string InvitationCode { get; set; }
        //public string TotalPoints { get; set; }
    }
}
