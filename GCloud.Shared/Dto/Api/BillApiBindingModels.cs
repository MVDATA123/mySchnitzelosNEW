using System;
using GCloud.Shared.Dto.Domain;
using System.Collections.Generic;

namespace GCloud.Shared.Dto.Api
{
    public class GetBillsResponseModel : List<Bill_Out_Dto>
    {
        public GetBillsResponseModel (IEnumerable<Bill_Out_Dto> collection) : base(collection) { }
    }

    public class BillAddRequestModel
    {
        public Guid CashRegisterId { get; set; }
        public string StoreApiToken { get; set; }
        public string UserId { get; set; }
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}