using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace GCloudShared.Domain
{
    public class User : BasePersistable
    {
        public string UserId { get; set; }
        public string RoleName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string AuthToken { get; set; }
        public DateTime AuthTokenDate { get; set; }
        public UserLoginMethod UserLoginMethod { get; set; }
        public string InvitationCode { get; set; }
        //public string TotalPoints { get; set; }
    }
}
