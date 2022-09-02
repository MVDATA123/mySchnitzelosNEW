using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Shared.Exceptions.User
{
    public abstract class BaseCouponException : BaseGustavException
    {
        public Guid? CouponId { get; set; }

        protected BaseCouponException(ExceptionStatusCode errorCode, string humanReadableMessage, Guid? couponId) : base(errorCode, humanReadableMessage)
        {
            CouponId = couponId;
        }
    }
}
