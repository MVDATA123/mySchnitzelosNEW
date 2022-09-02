using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Shared.Exceptions.General
{
    public class GustavArgumentNullException : GustavArgumentException
    {
        public GustavArgumentNullException(string propertyName) : base(propertyName, null)
        {
            HumanReadableMessage = $"\"{propertyName.ToUpper()}\" darf nicht NULL sein.";
        }
    }
}
