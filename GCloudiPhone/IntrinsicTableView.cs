using System;
using CoreGraphics;
using UIKit;

namespace GCloudiPhone
{
    partial class IntrinsicTableView : UITableView
    {
        public IntrinsicTableView(IntPtr handle): base(handle) { }

        public override CGSize ContentSize { get => base.ContentSize; set => InvalidateIntrinsicContentSize(); }

        public override CGSize IntrinsicContentSize
        {
            get
            {
                this.LayoutIfNeeded();
                return new CGSize(UIView.NoIntrinsicMetric, this.ContentSize.Height);
            }
        }
    }
}
