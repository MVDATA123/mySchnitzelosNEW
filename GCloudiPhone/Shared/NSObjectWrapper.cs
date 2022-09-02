using Foundation;

namespace GCloudiPhone.Shared
{
    public class NSObjectWrapper : NSObject
    {
        public object Context;

        public NSObjectWrapper(object obj) : base()
        {
            this.Context = obj;
        }

        public static NSObjectWrapper Wrap(object obj)
        {
            return new NSObjectWrapper(obj);
        }
    }
}
