using Foundation;
using System;
using UIKit;
using GCloud.Shared.Dto.Domain;
using GCloudiPhone.Sidebar;
using System.Collections.Generic;

namespace GCloudiPhone
{
    public partial class FilterMenuTableViewController : UITableViewController
    {
        public StoreMapViewController StoreMapViewController { get; set; }

        private readonly FilterSidebarNavigationDelegate filterSidebarNavigationDelegate;
        private List<TagDto> tags;
        public List<TagDto> Tags
        {
            get
            {
                if (tags == null)
                {
                    tags = new List<TagDto>();
                }
                return tags;
            }
            set
            {
                tags = value;
                var selectedTags = "";
                if (tags.Count == 0)
                {
                    selectedTags = "Beliebig";
                }
                else
                {
                    foreach (var tag in tags)
                    {
                        if (string.IsNullOrWhiteSpace(selectedTags))
                        {
                            selectedTags += tag.Name;
                        }
                        else
                        {
                            selectedTags += $@", {tag.Name}";
                        }
                    }
                }
                TagsLabel.Text = selectedTags;
                StoreMapViewController.FilterAnnotations(tags, Double.Parse(DistanceSlider.Value.ToString("F")));
            }
        }
        public double DistanceSliderValue { get { return Double.Parse(DistanceSlider.Value.ToString()); } }

        public FilterMenuTableViewController(IntPtr handle) : base(handle)
        {
            if(FilterSidebarViewController.Instance.TryGetTarget(out var filterSidebar))
            filterSidebarNavigationDelegate = new FilterSidebarNavigationDelegate(filterSidebar);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var userDefaults = NSUserDefaults.StandardUserDefaults;
            var distanceSetting = userDefaults.StringForKey(new NSString("distanceSetting"));

            if(distanceSetting != null) {
                //DistanceSlider.Value = float.Parse(distanceSetting);
            }
            DistanceSlider.Value = 10.0f;

            DistanceSlider.ValueChanged += DistanceSlider_ValueChanged;
            DistanceLabel.Text = DistanceSlider.Value.Equals(10.0f) ? "unbegrenzt" : DistanceSlider.Value < 1 ? $@"{(int)(DistanceSlider.Value * 1000)} m" : $@"{DistanceSlider.Value.ToString("F")} km";


        }

        void DistanceSlider_ValueChanged(object sender, EventArgs e)
        {
            var value = Double.Parse((sender as UISlider).Value.ToString());
            value = (Math.Round(value / 0.5d, 0)) * 0.5d;
            (sender as UISlider).Value = float.Parse(value.ToString());
            if (value.Equals(10.0f))
            {
                DistanceLabel.Text = "Unbegrenzt";
            }
            else
            {
                DistanceLabel.Text = value < 1 ? $@"{(int)(value * 1000)} m" : $@"{value.ToString("F")} km";
            }

            var userDefaults = NSUserDefaults.StandardUserDefaults;
            userDefaults.SetString(value.ToString(), "distanceSetting");
            StoreMapViewController.FilterAnnotations(Tags, Double.Parse(value.ToString("F")));

        }

        partial void SelectTagsButton_TouchUpInside(UIButton sender)
        {
            var storyboard = UIStoryboard.FromName("Main", null);
            var tagSearch = storyboard.InstantiateViewController("TagSearchNavigationController") as TagSearchNavigationController;
            ((TagSearchTableViewController)tagSearch.ViewControllers[0]).SelectedTags = Tags;
            filterSidebarNavigationDelegate.PerformNavigation(tagSearch);
        }
    }
}