using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Shared.Exceptions
{
    public interface IGustavException : _Exception
    {
        ExceptionStatusCode ErrorCode { get; set; }
        HttpStatusCode HttpStatusCode { get; set; }
        string HumanReadableMessage { get; set; }
    }
}

