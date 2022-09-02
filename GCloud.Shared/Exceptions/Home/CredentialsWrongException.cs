using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Shared.Exceptions.Home
{
    public class CredentialsWrongException : BaseGustavException
    {
        public CredentialsWrongException() : base(ExceptionStatusCode.CredentialsInvalid, HttpStatusCode.NotFound, $"Die eingegebenen Zugangsdaten sind falsch.")
        {
        }
    }
}
