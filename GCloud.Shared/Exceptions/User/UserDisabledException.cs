using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Shared.Exceptions.User
{
    public class UserDisabledException : BaseUserException
    {
        public UserDisabledException(string userId) : base(ExceptionStatusCode.UserDisabled, HttpStatusCode.PreconditionFailed, $"Der Benutzer wurde deaktiviert.", userId)
        {
        }
    }
}
