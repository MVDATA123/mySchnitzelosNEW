using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using GCloud.Shared.Dto.Domain;
using GCloudiPhone.Extensions;
using GCloudiPhone.Helpers;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Service.Dto;
using GCloudShared.Shared;
using Newtonsoft.Json;
using Optional;
using Optional.Collections;
using Refit;
using UIKit;
using GCloudiPhone.Extensions;

namespace GCloudiPhone.Caching
{
    public static class CachingService
    {
        private static readonly object LoadDataLock = new object();
        private static readonly object LoadDashboardImagesLock = new object();
        private static DateTime? _lastImageUpdate;
        private static DateTime? _lastCacheUpdate;
        private static DateTime? _lastDashboardImagesUpdate;
        private static readonly int _cacheUpdateIntervalInMinutes = (int)TimeSpan.FromMinutes(30).TotalMinutes;
        private static readonly int _imageUpdateIntervalInMinutes = (int)TimeSpan.FromHours(2).TotalMinutes;
        private static readonly int _dashboardImagesUpdateIntervalInMinutes = (int)TimeSpan.FromHours(24).TotalMinutes;
        private static readonly UserRepository userRepository = new UserRepository(DbBootstraper.Connection);

        private static readonly string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        private static readonly string storesFilePath = Path.Combine(path, "stores.json");
        private static readonly string couponsFilePath = Path.Combine(path, "coupons.json");

        public static async Task GetAllData(bool forceUpdate = false)
        {
            try
            {
                await UpdateCache(forceUpdate, forceUpdate, false);
            } catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            UpdateDashboardImages(forceUpdate);
        }

        public static async Task UpdateStores()
        {
            await Task.Run(async () =>
            {
                try
                {
                    UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
                    var currentStores = CacheHolder.Instance.Stores.Values().ToList();
                    var userStoreService = RestService.For<IUserStoreService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
                    var followingStores = await userStoreService.GetUserStores();
                    followingStores.ForEach(fs =>
                    {
                        var store = currentStores.FirstOrDefault(s => s.Id.Equals(fs.Id));
                        var index = currentStores.IndexOf(store);
                        currentStores.RemoveAt(index);
                        currentStores.Insert(index, new StoreLocationDto(fs));
                    });

                    CacheHolder.Instance.Stores = currentStores.Select(cs => cs.SomeNotNull()).ToList();
                    currentStores = null;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
                finally
                {
                    UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
                    UpdateCache(true);
                }
            });
        }

        public static async Task<List<CouponDto>> UpdateCoupons()
        {
            return await Task.Run(async () =>
            {
                var couponService = RestService.For<IUserCouponService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
                return await couponService.GetUserCoupons();
            });
        }

        public static async Task UnfollowStore(string storeId)
        {
            await Task.Run(async () =>
            {
                UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

                var userStoreService = RestService.For<IUserStoreService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
                await userStoreService.DeleteFromWatchlist(storeId);

                CacheHolder.Instance.Stores.Values().ToList().FirstOrDefault(s => s.Id.ToString().Equals(storeId)).IsUserFollowing = false;

                await UpdateCache();

                UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
                userStoreService = null;
            });
        }

        public static async Task FollowStore(string storeId)
        {
            await Task.Run(async () =>
            {
                UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;


                var userStoreService = RestService.For<IUserStoreService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
                await userStoreService.AddToWatchList(storeId);

                CacheHolder.Instance.Stores.Values().ToList().FirstOrDefault(s => s.Id.ToString().Equals(storeId)).IsUserFollowing = true;

                await UpdateCache();

                UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
                userStoreService = null;
            });
        }

        public static async Task UpdateCache(bool forceUpdate = false, bool forceUpdateImages = false, bool hideNetworkActivityOnCompletion = true)
        {
            if (!forceUpdate && _lastCacheUpdate.HasValue && _lastCacheUpdate.Value.AddMinutes(_cacheUpdateIntervalInMinutes) > DateTime.Now) return;

            await Task.Run(async () =>
            {
                UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
                try
                {
                    var startupService = RestService.For<IStartupService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
                    var updateImages = forceUpdateImages || !_lastImageUpdate.HasValue || _lastImageUpdate.Value.AddHours(1) < DateTime.Now.Date;
                    var result = await startupService.LoadInitialData(updateImages, updateImages, updateImages);
                    var now = DateTime.Now;
                    _lastImageUpdate = now;
                    _lastCacheUpdate = now;
                    CacheHolder.Instance.Stores = result.Stores.Select(s => new StoreLocationDto(s).SomeNotNull()).ToList();
                    CacheHolder.Instance.Coupons = result.Coupons.Select(c => c.SomeNotNull()).ToList();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
                finally
                {
                    UIApplication.SharedApplication.NetworkActivityIndicatorVisible = !hideNetworkActivityOnCompletion;
                }
            });
        }

        public static async Task UpdateDashboardImages(bool forceUpdate = false)
        {
            if (!forceUpdate && _lastDashboardImagesUpdate.HasValue && _lastDashboardImagesUpdate.Value.AddMinutes(_dashboardImagesUpdateIntervalInMinutes) > DateTime.Now) return;

            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
            await Task.Run(() =>
            {
                //Abort if the Last update was in the given interval
                if (Monitor.TryEnter(LoadDashboardImagesLock))
                {
                    try
                    {
                        var startupService = RestService.For<IStartupService>(HttpClientContainer.Instance.HttpClient);
                        string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                        var dirToCreate = Path.Combine(path, "DashboardImages");
                        if (!Directory.Exists(dirToCreate))
                        {
                            Directory.CreateDirectory(dirToCreate);
                        }

                        DirectoryInfo d = new DirectoryInfo(dirToCreate);
                        FileInfo[] files = d.GetFiles();
                        var alreadyDownloaded = new List<string>();

                        foreach (var file in files)
                        {
                            alreadyDownloaded.Add(file.Name);
                        }

                        startupService.GetBackGroundImages(String.Join(",", alreadyDownloaded))
                            .ContinueWith(t =>
                            {
                                if (t.IsCompletedSuccessfully)
                                {
                                    _lastDashboardImagesUpdate = DateTime.Now;
                                    var images = t.Result;
                                    foreach (var imageViewModel in images)
                                    {
                                        string filename = Path.Combine(dirToCreate, imageViewModel.Name);

                                        if (imageViewModel.StateEnum == ImageViewModelState.New)
                                        {
                                            using (var image = UIImage.LoadFromData(NSData.FromArray(imageViewModel.Image)))
                                            {
                                                WriteToFile(filename, image);
                                            }
                                        }
                                        else if (imageViewModel.StateEnum == ImageViewModelState.Deleted)
                                        {
                                            File.Delete(filename);
                                        }
                                    }
                                }
                            }, TaskContinuationOptions.OnlyOnRanToCompletion).Wait();
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message);
                    }
                    finally
                    {
                        UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
                        Monitor.Exit(LoadDashboardImagesLock);
                    }
                }
            });
        }

        public static void WriteToFile(string fileName, UIImage image)
        {

            var imageData = image.AsJPEG();
            if (!imageData.Save(fileName, false, out var err))
            {
                System.Diagnostics.Debug.Write(err.LocalizedDescription);
            }
            image.Dispose();
        }

        public static UIImage GetStoreBanner(StoreLocationDto store)
        {
            var image = CacheHolder.Instance.StoreBannerImages.FirstOrDefault(i => i.Guid.Equals(store.Id.ToString()))?.Image;
            if (image == null)
            {
                image = store.BannerImage.GetImageFromBase64();
                CacheHolder.Instance.StoreBannerImages.Add(new ImageHelper { Guid = store.Id.ToString(), Image = image });
            }
            return image;
        }

        public static UIImage GetCouponImage(CouponDto coupon)
        {
            var image = CacheHolder.Instance.CouponImages.FirstOrDefault(i => i.Guid.Equals(coupon.Id.ToString()))?.Image;
            if (image == null)
            {
                image = coupon.IconBase64.GetImageFromBase64();
                CacheHolder.Instance.CouponImages.Add(new ImageHelper { Guid = coupon.Id.ToString(), Image = image });
            }
            return image;
        }

        public static UIImage GetCouponQrCode(CouponDto coupon)
        {
            var image = CacheHolder.Instance.CouponQrCodes.FirstOrDefault(i => i.Guid.Equals(coupon.Id.ToString()))?.Image;
            if (image == null)
            {
                var userRedeem = new { UserId = userRepository.GetCurrentUser().UserId, CouponId = coupon.Id.ToString() };
                //image = QrCodeUtils.GetQrCode(JsonConvert.SerializeObject(userRedeem));
                image = JsonConvert.SerializeObject(userRedeem).GenerateQrCode();
                CacheHolder.Instance.CouponQrCodes.Add(new ImageHelper { Guid = coupon.Id.ToString(), Image = image });
            }
            return image;
        }

        public static UIImage GetLoyaltyCard()
        {
            if (CacheHolder.Instance.LoyaltyCard == null)
            {
                CacheHolder.Instance.LoyaltyCard = userRepository.GetCurrentUser().UserId.GenerateQrCode();

            }
            return CacheHolder.Instance.LoyaltyCard;
        }

        public static void ClearCachedImages()
        {
            CacheHolder.Instance.StoreBannerImages.ForEach(sb => { System.Diagnostics.Debug.WriteLine(sb.Image.RetainCount); sb.Image.Dispose(); sb.Image = null; });
            CacheHolder.Instance.StoreBannerImages = null;
            CacheHolder.Instance.CouponImages.ForEach(ci => { System.Diagnostics.Debug.WriteLine(ci.Image.RetainCount); ci.Image.Dispose(); ci.Image = null; });
            CacheHolder.Instance.CouponImages = null;
            CacheHolder.Instance.CouponQrCodes.ForEach(ci => { System.Diagnostics.Debug.WriteLine(ci.Image.RetainCount); ci.Image.Dispose(); ci.Image = null; });
            CacheHolder.Instance.CouponQrCodes = null;

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();

            CacheHolder.Instance.StoreBannerImages = new List<ImageHelper>();
            CacheHolder.Instance.CouponImages = new List<ImageHelper>();
            CacheHolder.Instance.CouponQrCodes = new List<ImageHelper>();
        }

        public static void PersistCache()
        {
            try
            {
                using (var storesFile = File.Open(storesFilePath, FileMode.Create, FileAccess.Write))
                using (var strm = new StreamWriter(storesFile, System.Text.Encoding.UTF8))
                {
                    strm.Write(JsonConvert.SerializeObject(CacheHolder.Instance.Stores.Values().ToList()));
                }

                using (var couponsFile = File.Open(couponsFilePath, FileMode.Create, FileAccess.Write))
                using (var strm = new StreamWriter(couponsFile, System.Text.Encoding.UTF8))
                {
                    strm.Write(JsonConvert.SerializeObject(CacheHolder.Instance.Coupons.Values().ToList()));
                }

                CacheHolder.Instance.Stores = null;
                CacheHolder.Instance.Coupons = null;
            }
            catch (IOException ioe)
            {
                System.Diagnostics.Debug.WriteLine(ioe.Message);
            }
        }

        public static async Task LoadCache()
        {
            await Task.Run(async () =>
            {
                if (File.Exists(storesFilePath) && File.Exists(couponsFilePath))
                {
                    try
                    {
                        using (var storesFile = File.Open(storesFilePath, FileMode.Open))
                        using (var strm = new StreamReader(storesFile, System.Text.Encoding.UTF8))
                        {
                            var value = await strm.ReadToEndAsync();
                            CacheHolder.Instance.Stores = JsonConvert.DeserializeObject<List<StoreLocationDto>>(value).Select(s => s.SomeNotNull()).ToList();
                        }

                        using (var couponsFile = File.Open(couponsFilePath, FileMode.Open))
                        using (var strm = new StreamReader(couponsFile, System.Text.Encoding.UTF8))
                        {
                            CacheHolder.Instance.Coupons = JsonConvert.DeserializeObject<List<CouponDto>>(await strm.ReadToEndAsync()).Select(c => c.SomeNotNull()).ToList();
                        }

                        //File.Delete(storesFilePath);
                        //File.Delete(couponsFilePath);

                        //Asynchronously load data anyway to have the lastest if they haven't been loaded lately.
                        GetAllData();
                    }
                    catch (IOException ioe)
                    {
                        System.Diagnostics.Debug.WriteLine(ioe.Message);
                    }
                }
                else
                {
                    await GetAllData(true);
                }
            });
        }

        public static List<Option<StoreLocationDto>> GetStores()
        {
            if (CacheHolder.Instance.Stores == null)
            {
                LoadCache();
            }

            return CacheHolder.Instance.Stores;
        }

        public static List<Option<CouponDto>> GetCoupons()
        {
            if (CacheHolder.Instance.Coupons == null)
            {
                LoadCache();
            }

            return CacheHolder.Instance.Coupons;
        }
    }
}