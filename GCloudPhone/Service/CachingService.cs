using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using GCloudShared.Domain;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Service.Dto;
using GCloudShared.Shared;
using Java.IO;
using mvdata.foodjet.Caching;
using mvdata.foodjet.Domain;
using Newtonsoft.Json;
using Optional;
using Refit;
using File = Java.IO.File;
using Thread = Java.Lang.Thread;

namespace mvdata.foodjet.Service
{
    public class CachingService
    {
        private static readonly object LoadDataLock = new object();
        private static readonly object LoadDashboardImagesLock = new object();
        private static DateTime? _lastImageUpdate;
        private static DateTime? _lastCacheUpdate;
        private static DateTime? _lastDashboardImagesUpdate;
        private static readonly int _cacheUpdateIntervalInMinutes = (int)TimeSpan.FromMinutes(30).TotalMinutes;
        private static readonly int _imageUpdateIntervalInMinutes = (int)TimeSpan.FromHours(2).TotalMinutes;
        private static readonly int _dashboardImagesUpdateIntervalInMinutes = (int)TimeSpan.FromHours(24).TotalMinutes;

        /// <summary>
        /// Macht das beim nächsten Request wieder Bilder mitgeladen werden
        /// </summary>
        public void ResetImageUpdate()
        {
            _lastCacheUpdate = null;
            _lastImageUpdate = null;
            _lastDashboardImagesUpdate = null;
        }

        public static async Task GetAllData(Activity context, bool forceUpdate = false)
        {
            await ExecuteGetAllData(context, forceUpdate);
            await ExeciteDashboardImages(context, forceUpdate);
        }

        public static async Task ExecuteGetAllData(Activity context, bool forceUpdate = false)
        {
            await Task.Run(() =>
            {
                //Abort if the Last update was in the given interval
                if (!forceUpdate && _lastCacheUpdate.HasValue && _lastCacheUpdate.Value.AddMinutes(_cacheUpdateIntervalInMinutes) > DateTime.Now) return;
                if (Monitor.TryEnter(LoadDataLock))
                {
                    try
                    {
                        var startupService = RestService.For<IStartupService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
                        var settings = context.GetSharedPreferences(context.GetString(Resource.String.sharedPreferencesKey), FileCreationMode.Private);
                        var userLoginMethod = (UserLoginMethod)settings.GetInt(context.GetString(Resource.String.sharedPreferencesLoginMethod), -1);

                        if (userLoginMethod == UserLoginMethod.Normal)
                        {
                            var authToken = settings.GetString(context.GetString(Resource.String.sharedPreferencesAuthToken), string.Empty);
                            HttpClientContainer.Instance.CookieContainer.Add(UriContainer.BasePath, new Cookie(".AspNet.ApplicationCookie", authToken));
                        }

                        try
                        {
                            try
                            {
                                var updateImages = forceUpdate || !_lastImageUpdate.HasValue || _lastImageUpdate.Value.AddHours(1) < DateTime.Now.Date;
                                startupService.LoadInitialData(updateImages, updateImages, updateImages).ContinueWith(t =>
                                {
                                    if (!t.IsFaulted)
                                    {
                                        var now = DateTime.Now;
                                        _lastImageUpdate = now;
                                        _lastCacheUpdate = now;
                                        CachingHolder.Instance.Stores = t.Result.Stores.Select(s => new StoreLocationDto(s).SomeNotNull()).ToList();
                                        CachingHolder.Instance.Coupons = t.Result.Coupons.Select(c => c.SomeNotNull()).ToList();
                                    }

                                }, TaskContinuationOptions.OnlyOnRanToCompletion).Wait();
                            }
                            catch (AggregateException ex)
                            {

                            }
                        }
                        catch (ApiException apiException)
                        {
                        }
                        catch (JsonReaderException jsonReaderException)
                        {
                        }
                    }
                    finally
                    {
                        Monitor.Exit(LoadDataLock);
                    }
                }
            });
        }

        public static async Task ExeciteDashboardImages(Activity context, bool forceUpdate = false)
        {
            await Task.Run(() =>
            {
                //Abort if the Last update was in the given interval
                if (!forceUpdate && _lastDashboardImagesUpdate.HasValue &&
                    _lastDashboardImagesUpdate.Value.AddMinutes(_dashboardImagesUpdateIntervalInMinutes) > DateTime.Now) return;
                if (Monitor.TryEnter(LoadDashboardImagesLock))
                {
                    try
                    {
                        var startupService = RestService.For<IStartupService>(HttpClientContainer.Instance.HttpClient);
                        string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                        var dirToCreate = System.IO.Path.Combine(path, "DashboardImages");
                        if (!System.IO.Directory.Exists(dirToCreate))
                        {
                            System.IO.Directory.CreateDirectory(dirToCreate);
                        }

                        DirectoryInfo d = new DirectoryInfo(dirToCreate);
                        FileInfo[] files = d.GetFiles();
                        var alreadyDownloaded = new List<string>();

                        foreach (var file in files)
                        {
                            alreadyDownloaded.Add(file.Name);
                        }

                        startupService.GetBackGroundImages(string.Join(",", alreadyDownloaded))
                            .ContinueWith(t =>
                            {
                                if (t.IsCompletedSuccessfully)
                                {
                                    _lastDashboardImagesUpdate = DateTime.Now;
                                    var images = t.Result;
                                    foreach (var imageViewModel in images)
                                    {
                                        string filename = System.IO.Path.Combine(dirToCreate, imageViewModel.Name);

                                        if (imageViewModel.StateEnum == ImageViewModelState.New)
                                        {
                                            WriteToFile(filename, imageViewModel.Image);
                                        }
                                        else if (imageViewModel.StateEnum == ImageViewModelState.Deleted)
                                        {
                                            System.IO.File.Delete(filename);
                                        }
                                    }
                                }
                            }, TaskContinuationOptions.OnlyOnRanToCompletion).Wait();
                    }
                    finally
                    {
                        Monitor.Exit(LoadDashboardImagesLock);
                    }
                }
            });
        }

        public static void WriteToFile(string fileName, byte[] fileContent)
        {
            using (var fileOutputStream = new FileOutputStream(fileName))
            {
                fileOutputStream.Write(fileContent);
            }
        }
    }
}