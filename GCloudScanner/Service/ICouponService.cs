using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using GCloud.Shared.Dto.Api;
using GCloud.Shared.Dto.Domain;
using Refit;

namespace GCloudScanner.Service
{
    public interface ICouponService
    {
        [Put("/api/StoreCouponsApi")]
        Task<HttpResponseMessage> RedeemCoupon([Body] StoreCouponApiBindingModel.StoreCouponApiPutModel model);

        [Get("/api/StoreCouponsApi?couponId={couponId}&userId={userId}&storeApiToken={storeApiToken}&cashRegisterId={cashRegisterId}")]
        Task<CouponDto> GetCoupon(Guid couponId, string userId, string storeApiToken, Guid cashRegisterId);

        [Get("/api/StoreCouponsApi?storeApiToken={storeApiToken}&userId={userId}&cashRegisterId={cashRegisterId}")]
        Task<List<CouponDto>> GetCouponsByUser(string storeApiToken, string userId, Guid cashRegisterId);

        [Get("/Coupons/LoadCouponImage/{couponId}")]
        Task<Stream> GetCouponImage(Guid couponId);
    }
}