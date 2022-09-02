using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Shared.Exceptions.Home
{
    public class SendMailException : BaseGustavException
    {
        public SendMailException() : base(ExceptionStatusCode.SendMailException, HttpStatusCode.BadRequest, $"Beim versenden des Bestätigungs-Links ist ein allgemeiner Fehler aufgetreten.")
        {
        }
    }
}
