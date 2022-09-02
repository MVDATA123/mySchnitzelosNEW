using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Shared.Exceptions
{
    public class GustavArgumentException : ArgumentException, IGustavException
    {
        public ExceptionStatusCode ErrorCode { get; set; } = ExceptionStatusCode.ArgumentInvalid;
        public HttpStatusCode HttpStatusCode { get; set; } = HttpStatusCode.BadRequest;
        public string HumanReadableMessage { get; set; }

        public GustavArgumentException(string propertyName, string value)
        {
            HumanReadableMessage = $"Dem Property oder Field {propertyName} kann nicht der Wert {value} zugewiesen werden.";
        }
    }
}
