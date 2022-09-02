using System.Collections.Generic;
using GCloud.Shared.Dto.Api;
using GCloudShared.Service.Dto;
using Refit;
using System.Threading.Tasks;

namespace GCloudShared.Service
{
    public interface IStartupService
    {
        [Get("/api/HomeApi/LoadInitialData")]
        Task<LoadInitialDataResponseModel> LoadInitialData(bool includeImage, bool includeBanner, bool includeCompanyLogo);

        [Get("/api/HomeApi/GetBackGroundImages")]
        Task<List<ImageViewModel>> GetBackGroundImages(string alreadyDownloaded);

    }
}
