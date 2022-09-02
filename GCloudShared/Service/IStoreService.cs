using System;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GCloud.Shared.Dto.Api;
using GCloud.Shared.Dto.Domain;
using Refit;

namespace GCloudShared.Service
{
    public interface IStoreService
    {
        [Get("/api/StoresApi")]
        Task<List<StoreDto>> GetStores();

        [Get("/Stores/LoadStoreImage/{guid}")]
        Task<Stream> GetStoreImage([AliasAs("guid")] string guid);
        
        [Post("/api/StoresApi")]
        Task<HttpResponseMessage> UpdateStore(StoreManagerEditModel storeManagerEditModel);
    }
}
