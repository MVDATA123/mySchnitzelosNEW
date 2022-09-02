using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GCloud.Shared.Dto.Domain;
using Refit;

namespace GCloudShared.Service
{
    public interface IUserCouponService
    {
        [Get("/api/UserCouponsApi?skipUserValidation={skipUserValidation}")]
        Task<List<CouponDto>> GetUserCoupons(bool skipUserValidation = false);

        [Get("/api/UserCouponsApi/{guid}")]
        Task<CouponDto> GetUserCoupon([AliasAs("guid")] string guid);

        [Get("/api/UserCouponsApi/LoadCouponQrCode/{guid}")]
        Task<Stream> GetCouponQrCode([AliasAs("guid")] string guid);

        [Get("/Coupons/LoadCouponImage/{guid}")]
        Task<Stream> GetCouponImage([AliasAs("guid")] string guid);

        [Get("/api/UserCouponsApi/store/{guid}?skipUserValidation={skipUserValidation}")]
        Task<List<CouponDto>> GetUserCouponsByStore([AliasAs("guid")]string guid, bool skipUserValidation = false, bool includeImage = true);

        [Get("/api/UserCouponsApi/GetManagerCoupons")]
        Task<List<CouponDto>> GetManagerCoupons();
    }
}
