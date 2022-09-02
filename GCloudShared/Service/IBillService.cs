using GCloud.Shared.Dto.Api;
using GCloud.Shared.Dto.Domain;
using Refit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GCloudShared.Service
{
    public interface IBillService
    {
        [Get("/api/BillApi/Get")]
        Task<GetBillsResponseModel> Get(List<Guid> alreadyGot);

        [Get("/api/BillApi/GetById")]
        Task<Bill_Out_Dto> GetById(Guid id);

        [Post("/api/BillApi/CSV")]
        Task<HttpContent> Csv(Guid? anonymousUserId);
    }
}
