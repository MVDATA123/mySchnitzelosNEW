using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Shared.Exceptions.User
{
    public abstract class BaseCashbackException : BaseGustavException
    {
        public Guid? CashbackId { get; set; }

        protected BaseCashbackException(ExceptionStatusCode errorCode, string humanReadableMessage, Guid? cashbackId) : base(errorCode, humanReadableMessage)
        {
            CashbackId = cashbackId;
        }
    }
}
