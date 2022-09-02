using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Shared.Exceptions.Home
{
    public class OldPasswordInvalidException : BaseGustavException
    {
        public OldPasswordInvalidException() : base(ExceptionStatusCode.OldPasswordInvalid, $"Das alte Password ist falsch.")
        {
        }
    }
}
