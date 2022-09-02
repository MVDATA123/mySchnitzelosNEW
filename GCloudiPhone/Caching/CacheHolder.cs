using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GCloud.Shared.Dto.Domain;
using GCloudiPhone.Extensions;
using GCloudiPhone.Helpers;
using Optional;
using Optional.Collections;
using UIKit;

namespace GCloudiPhone.Caching
{
    public sealed class CacheHolder
    {
        private static readonly Lazy<CacheHolder> Lazy = new Lazy<CacheHolder>(() => new CacheHolder());
        public static CacheHolder Instance => Lazy.Value;

        private List<Option<StoreLocationDto>> _stores = new List<Option<StoreLocationDto>>();
        private List<Option<CouponDto>> _coupons = new List<Option<CouponDto>>();

        public List<ImageHelper> StoreBannerImages { get; set; } = new List<ImageHelper>();
        public List<ImageHelper> CouponImages { get; set; } = new List<ImageHelper>();
        public List<ImageHelper> CouponQrCodes { get; set; } = new List<ImageHelper>();
        public UIImage LoyaltyCard { get; set; }

        public List<TagDto> Tags => _stores.Values().SelectMany(s => s.Tags).GroupBy(x => x.Name, x => x).Select(kvp => kvp.First()).ToList();
        private string _lastDashboardImage { get; set; }
        private List<object> defaultImages;
        Random random = new Random();

        public List<Option<StoreLocationDto>> Stores
        {
            get
            {
                if (_stores == null)
                {
                    Task.Run(async () => await CachingService.LoadCache()).Wait();
                }
                return _stores;
            }
            set
            {
                if (_stores == null || value == null)
                {
                    _stores = value;
                    return;
                }
                foreach (var storeDto in value.Values())
                {

                    //Bestehende Bilder sollen nicht so einfach verschwinden.
                    _stores.Values()
                        .Where(x => x.Id == storeDto.Id && !string.IsNullOrWhiteSpace(x.BannerImage))
                        .Select(x => x.BannerImage)
                        .FirstOrNone()
                        .MatchSome(banner =>
                        {
                            storeDto.BannerImage = banner;
                        });
                }

                _stores = value;
            }
        }

        public List<Option<CouponDto>> Coupons
        {
            get
            {
                if(_coupons == null)
                {
                    CachingService.LoadCache();
                }
                return _coupons;
            }
            set
            {
                if (_coupons == null || value == null)
                {
                    _coupons = value;
                    return;
                }
                foreach (var couponDto in value.Values())
                {

                    //Bestehende Bilder sollen nicht so einfach verschwinden.
                    _coupons.Values()
                        .Where(x => x.Id == couponDto.Id && !string.IsNullOrWhiteSpace(x.IconBase64))
                        .Select(x => x.IconBase64)
                        .FirstOrNone()
                        .MatchSome(icon => couponDto.IconBase64 = icon);
                }

                _coupons = value;
            }
        }

        public List<Option<Bill_Out_Dto>> Bills { get; set; } = new List<Option<Bill_Out_Dto>>();

        public UIImage NextImage()
        {
            if (defaultImages == null)
            {
                defaultImages = new List<object>
            {
                UIImage.FromBundle("Dashboard-Image-1"),
                UIImage.FromBundle("Dashboard-Image-2"),
                UIImage.FromBundle("Dashboard-Image-3"),
                UIImage.FromBundle("Dashboard-Image-4"),
                UIImage.FromBundle("Dashboard-Image-5")
            };
            }

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var dirToCreate = System.IO.Path.Combine(path, "DashboardImages");
            List<object> files;
            if (!System.IO.Directory.Exists(dirToCreate))
            {
                files = defaultImages;
            }
            else
            {
                files = new List<object>(System.IO.Directory.GetFiles(dirToCreate));
                files.AddRange(defaultImages);
            }

            files.Remove(files.FirstOrDefault(x => x.ToString().Equals(_lastDashboardImage)));


            int randomPosition = random.Next(files.Count);
            if (files[randomPosition] is UIImage image)
            {
                _lastDashboardImage = image.ToString();
                return image;
            }
            if (files[randomPosition] is string filePath)
            {
                _lastDashboardImage = filePath;
                return UIImage.FromFile(filePath);
            }


            return null;
        }

        public List<Option<CouponDto>> GetCouponsByStore(Guid storeId)
        {
            return Coupons.Where(c => c.Exists(x => x.AssignedStores.Any(assignment => assignment == storeId))).ToList();
        }

        public Option<CouponDto> GetCouponByGuid(Guid id)
        {
            return Coupons.Values().FirstOrNone(c => c.Id == id);
        }

        public List<Option<Bill_Out_Dto>> GetBills()
        {
            return Bills.ToList();
        }

        public Option<string> GetStoreBannerAsBase64(Guid storeGuid)
        {
            return Stores.Values().Where(x => x.Id == storeGuid).Select(x => x.BannerImage).FirstOrDefault().SomeNotNull();
        }

        public void SetStoreFollowStatus(Guid storeGuid, bool followStatus)
        {
            var store = Stores.Values().FirstOrDefault(x => x.Id == storeGuid).SomeNotNull();
            store.MatchSome(s => s.IsUserFollowing = followStatus);
        }

        public void SetCouponsOfStore(Guid storeGuid, List<CouponDto> coupons)
        {
            Coupons.RemoveAll(x => x.Exists(c => c.AssignedStores.Contains(storeGuid)));
            Coupons.AddRange(coupons.Select(c => c.SomeNotNull()));
        }

        public void SetBills(List<Bill_Out_Dto> bills)
        {
            Bills.RemoveAll(b => true);
            Bills.AddRange(bills.Select(c => c.SomeNotNull()));
        }

        public Option<string> GetCompanyLogoAsBase64(Guid storeId)
        {
            return Stores.Values().Where(s => s.Id == storeId && s.Company != null).Select(s => s.Company.CompanyLogoBase64).FirstOrDefault().SomeNotNull();
        }

        public Option<StoreLocationDto> GetStoreByGuid(Guid id)
        {
            return Stores.Values().FirstOrDefault(s => s.Id == id).SomeNotNull();
        }

        private CacheHolder() { }
    }
}
