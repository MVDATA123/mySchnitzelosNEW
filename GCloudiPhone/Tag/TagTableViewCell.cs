using System;
using GCloud.Shared.Dto.Domain;
using UIKit;

namespace GCloudiPhone
{
    public partial class TagTableViewCell : UITableViewCell
    {
        public TagDto TagDto { get; set; }

        public TagTableViewCell(IntPtr handle) : base(handle)
        {
        }

        public TagTableViewCell(TagDto tag)
        {
            SetCellData(tag);
        }

        public void UpdateCell(TagDto tag)
        {
            SetCellData(tag);
        }

        public void SetCellData(TagDto tag)
        {
            TagDto = tag;
            TagLabel.Text = TagDto.Name;
        }
    }
}