using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GCloud.Shared.Dto.Domain;
using Refit;

namespace GCloudShared.Service
{
    public interface ICashbackService
    {
        [Get("/api/Cashback")]
        Task<List<CashbackDto>> GetCashbacksForStore(string storeGuid);
    }
}
