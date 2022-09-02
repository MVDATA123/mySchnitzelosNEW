using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GCloud.Shared.Dto.Domain;
using Refit;

namespace GCloudShared.Service
{
    public interface IUserStoreService
    {
        [Get("/api/UserStoresApi")]
        Task<List<StoreDto>> GetUserStores();

        [Put("/api/UserStoresApi/{guid}")]
        Task AddToWatchList([AliasAs("guid")]string guid);

        [Delete("/api/UserStoresApi/{guid}")]
        Task DeleteFromWatchlist([AliasAs("guid")]string guid);

        [Get("/api/UserStoresApi/GetManagerStores")]
        Task<List<StoreDto>> GetManagerStores();
    }
}
