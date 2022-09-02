using System;
using GCloudiPhone.Helpers;
using UIKit;

namespace GCloudiPhone.Shared
{
    public class MemoryOptimizedUINavigationController : UINavigationController, ICanCleanUpMyself
    {
        public MemoryOptimizedUINavigationController(IntPtr handle) : base(handle)
        {
        }

        public void CleanUp()
        {
            foreach (var viewController in ViewControllers)
            {
                if (viewController is ICanCleanUpMyself canCleanUpMyself)
                {
                    canCleanUpMyself.CleanUp();
                }
            }
        }
    }
}
