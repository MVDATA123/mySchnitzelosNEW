using System;
using System.Collections.Generic;
using System.Text;

namespace GCloudShared.Domain
{
    /// <summary>
    /// Represents the stores that the user is subscribed on
    /// </summary>
    public class Subscription : BasePersistable
    {
        public Guid StoreId { get; set; }
    }
}
