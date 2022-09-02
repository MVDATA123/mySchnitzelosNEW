using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Shared.Exceptions.User
{
    public class UserNotFoundException : BaseUserException
    {
        public UserNotFoundException(string userId) : base(ExceptionStatusCode.UserNotFound, $"Benutzer wurde nicht gefunden.", userId)
        {
        }
    }
}
