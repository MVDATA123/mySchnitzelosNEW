using System;
namespace GCloudShared.Domain
{
    public class StoreWhitelist : BasePersistable
    {
        public Guid Store { get; set; }
        public string User { get; set; }
    }
}
