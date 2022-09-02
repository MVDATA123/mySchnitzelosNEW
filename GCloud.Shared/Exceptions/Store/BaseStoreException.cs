using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Shared.Exceptions.User
{
    public abstract class BaseStoreException : BaseGustavException
    {
        public Guid? StoreId { get; set; }

        protected BaseStoreException(ExceptionStatusCode errorCode, string humanReadableMessage, Guid? storeId) : base(errorCode, humanReadableMessage)
        {
            StoreId = storeId;
        }
    }
}
