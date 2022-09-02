using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Shared.Exceptions.Home
{
    public class RegistrationException : BaseGustavException
    {
        public RegistrationException() : base(ExceptionStatusCode.GeneralRegistrationException, HttpStatusCode.BadRequest, $"Bei der registrierung ist ein allgemeiner Fehler aufgetreten.")
        {
        }
    }
}
