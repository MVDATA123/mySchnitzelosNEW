using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Util;
using GCloud.Shared.Dto.Domain;
using mvdata.foodjet.Domain;
using mvdata.foodjet.Service;
using Optional;
using Optional.Collections;
using Optional.Linq;
using Uri = Android.Net.Uri;

namespace mvdata.foodjet.Caching
{
    public sealed class CachingHolder
    {
        private static readonly Lazy<CachingHolder> Lazy = new Lazy<CachingHolder>(() => new CachingHolder());
        public static CachingHolder Instance => Lazy.Value;

        private List<Option<StoreLocationDto>> _stores = new List<Option<StoreLocationDto>>();
        private List<Option<CouponDto>> _coupons = new List<Option<CouponDto>>();
        private List<Option<Bill_Out_Dto>> _bills = new List<Option<Bill_Out_Dto>>();

        public List<TagDto> Tags => _stores.Values().SelectMany(s => s.Tags).GroupBy(x => x.Name, x => x).Select(kvp => kvp.First()).ToList();
        private string _lastDashboardImage { get; set; }

        public List<Option<StoreLocationDto>> Stores
        {
            get => _stores;
            set
            {
                if (_stores == null)
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
            get => _coupons;
            set
            {
                if (_coupons == null)
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
        
        public List<Option<Bill_Out_Dto>> Bills
        {
            get => _bills;
            set => _bills = value;
        }

        public Bitmap NextImage(Context context)
        {
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var dirToCreate = System.IO.Path.Combine(path, "DashboardImages");
            var files = new List<object>(System.IO.Directory.GetFiles(dirToCreate))
            {
                Resource.Drawable.dashboard_pizza,
                Resource.Drawable.dashboard_schnitzel_cropped,
                Resource.Drawable.dashboard_asian_food2,
                Resource.Drawable.dashboard_coffe_and_cake,
                Resource.Drawable.dashboard_pexels_photo
            };

            files.Remove(files.FirstOrDefault(x => x.ToString().Equals(_lastDashboardImage)));

            Random random = new Random();
            int randomPosition = random.Next(files.Count);
            if (files[randomPosition] is int resourceId)
            {
                _lastDashboardImage = resourceId.ToString();
                return BitmapFactory.DecodeResource(context.Resources, resourceId);
            }
            else if (files[randomPosition] is string filePath)
            {
                _lastDashboardImage = filePath;
                return BitmapFactory.DecodeFile(filePath);
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

        private CachingHolder() { }
    }
}
