using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using GCloud.Shared.Dto.Domain;
using Java.Lang;
using mvdata.foodjet.Mapper;

namespace mvdata.foodjet.Filter
{
    public class TagAdapter : RecyclerView.Adapter
    {
        public IList<TagViewModel> Tags { get; set; }
        public bool AllSelected => Tags.All(t => t.IsChecked);
        public bool AnySelected => Tags.Any(t => t.IsChecked);
        public IEnumerable<TagViewModel> Selected => Tags.Where(t => t.IsChecked);

        public TagAdapter(List<TagDto> data)
        {
            Tags = TransformData(data);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var feedItem = Tags[position];
            if (holder is TagViewHolder customViewHolder)
            {
                customViewHolder.TextView.Text = feedItem.Name;
                customViewHolder.CheckBox.Checked = feedItem.IsChecked;
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.MapsDialogTagRowItem, null);
            var viewHolder = new TagViewHolder(view, (position, isChecked) =>
            {
                var item = Tags[position];
                item.IsChecked = isChecked;
            });
            return viewHolder;
        }

        public override int ItemCount => Tags?.Count ?? 0;

        public void SetData(List<TagViewModel> items)
        {
            Tags = items;
            NotifyDataSetChanged();
        }
        public static IList<TagViewModel> TransformData(List<TagDto> items)
        {
            return AutoMapperConfig.Instance.Mapper.Map<IList<TagViewModel>>(items);
        }

        public void CheckTags(ICollection<Guid> tagIds)
        {
            foreach (var tag in Tags)
            {
                if (tagIds.Contains(tag.Id))
                {
                    tag.IsChecked = true;
                }
            }
            NotifyDataSetChanged();
        }

        public void ClearChecks()
        {
            foreach (var tag in Tags)
            {
                tag.IsChecked = false;
            }
            NotifyDataSetChanged();
        }
    }

    public class TagViewHolder : RecyclerView.ViewHolder
    {
        public CheckBox CheckBox { get; set; }
        public TextView TextView { get; set; }

        public TagViewHolder(View view, Action<int, bool> checkboxAction) : base(view)
        {
            this.CheckBox = (CheckBox)view.FindViewById(Resource.Id.MapsDialogRowItemCheckbox);
            this.TextView = (TextView)view.FindViewById(Resource.Id.MapsDialogRowItemCheckbox);

            this.CheckBox.CheckedChange += (sender, args) => checkboxAction(base.LayoutPosition, args.IsChecked);
        }

    }
}