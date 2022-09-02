using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using GCloud.Shared.Dto.Api;
using Refit;

namespace GCloudScanner.Service
{
    public interface ISettingsService
    {
        [Get("/api/Config/AvailableStores?username={username}&password={password}")]
        Task<List<AvailableStoresResponse>> GetAvailableStores(string username, string password);

        [Put("/api/Config/ApiToken/{storeGuid}?username={username}&password={password}&macAddress={macAddress}&deviceName={deviceName}")]
        Task<DeviceConfigResponse> GetStoreApiToken(string username, string password, Guid storeGuid, string macAddress, string deviceName);
    }
}