using System;
using MapKit;

namespace GCloudiPhone.Extensions
{
    public class StoreMKPointAnnotation : MKPointAnnotation
    {
        public StoreLocationDto Store { get; set; }
    }
}
