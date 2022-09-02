using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Shared.Exceptions.Home
{
    public class UsernameAlreadyTakenException : BaseGustavException
    {
        public UsernameAlreadyTakenException(string username) : base(ExceptionStatusCode.UsernameAlreadyTaken, HttpStatusCode.Conflict, $"Benutzername {username} bereits vergeben.")
        {
        }
    }
}
