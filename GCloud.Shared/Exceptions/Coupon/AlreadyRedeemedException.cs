using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCloud.Shared.Exceptions.User;

namespace GCloud.Shared.Exceptions.Coupon
{
    public class AlreadyRedeemedException : BaseCouponException
    {
        public AlreadyRedeemedException(Guid? couponId) : base(ExceptionStatusCode.AlreadyRedeemed, $"Der Gutschein wurde bereits einmal eingelöst.", couponId)
        {
        }
    }
}
