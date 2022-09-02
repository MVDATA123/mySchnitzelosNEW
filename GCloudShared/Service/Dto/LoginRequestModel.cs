using System;
using System.Collections.Generic;
using System.Text;

namespace GCloudShared
{
    public class LoginRequestModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string DeviceId { get; set; }
        public string FirebaseInstanceId { get; set; }
    }
}
