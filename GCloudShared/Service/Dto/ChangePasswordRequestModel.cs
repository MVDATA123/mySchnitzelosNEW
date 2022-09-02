using System;
using System.Collections.Generic;
using System.Text;

namespace GCloudShared.Service.Dto
{
    public class ChangePasswordRequestModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
