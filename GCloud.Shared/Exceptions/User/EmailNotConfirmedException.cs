using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Shared.Exceptions.User
{
    public class EmailNotConfirmedException : BaseUserException
    {
        public EmailNotConfirmedException(string userId) : base(ExceptionStatusCode.EmailNotConfirmed, HttpStatusCode.NotAcceptable, $"E-Mail noch nicht bestätigt.", userId)
        {
        }
    }
}
