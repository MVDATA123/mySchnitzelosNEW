using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using GCloud.Shared.Dto.Domain;
using GCloudiPhone.Extensions;
using GCloudiPhone.Shared;
using GCloudShared.Service;
using GCloudShared.Shared;
using Optional;
using Refit;
using UIKit;

namespace GCloudiPhone
{
    public partial class TagSearchTableViewController : UITableViewController, IUISearchResultsUpdating
    {
        private UISearchController SearchController { get; set; }
        private readonly IStoreService storeService;
        private LoadingOverlay loading;

        public List<TagDto> Tags { get; private set; }
        public WeakReference<FilterMenuTableViewController> FilterMenuController { get; set; }
        public List<TagDto> SelectedTags { get; set; }

        public TagSearchTableViewController(IntPtr handle) : base(handle)
        {
            storeService = RestService.For<IStoreService>(HttpClientContainer.Instance.HttpClient);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (FilterSidebarViewController.Instance.TryGetTarget(out var filterSidebarController))
            {
                FilterMenuController = filterSidebarController.FilterMenuControllerRef;
            }
            loading = new LoadingOverlay(View.Frame);
            SearchController = new ColoredUISearchController(searchResultsController: null)
            {
                DimsBackgroundDuringPresentation = false
            };
            SearchController.SearchResultsUpdater = this;
            SearchController.SearchBar.Placeholder = "Kategorien durchsuchen";

            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                NavigationItem.SearchController = SearchController;
                NavigationItem.HidesSearchBarWhenScrolling = true;
            }
            else
            {
                SearchController.SearchBar.SearchBarStyle = UISearchBarStyle.Default;
                SearchController.SearchBar.SizeToFit();
                TagsTableView.TableHeaderView = SearchController.SearchBar;
            }

            NavigationItem.LeftBarButtonItem = new UIBarButtonItem("Beliebig", UIBarButtonItemStyle.Plain, SetAllTags);
            NavigationItem.RightBarButtonItem = new UIBarButtonItem("Fertig", UIBarButtonItemStyle.Plain, SetTags);

            View.AddSubview(loading);

            Tags = new List<TagDto>();
            TagsTableView.Source = new TagTableSource(Tags, this);
            TagsTableView.ReloadData();

            LoadTags();
        }

        private async void LoadTags()
        {
            var stores = (await storeService.GetStores()).Select(s => new StoreLocationDto(s).SomeNotNull()).ToList();
            var tmpTags = new List<TagDto>();
            foreach (var store in stores)
            {
                store.MatchSome(s => tmpTags.AddRange(s.Tags));
            }

            Tags = tmpTags.GroupBy(t => t.Name).Select(t => t.First()).ToList();
            TagsTableView.Source = new TagTableSource(Tags, this);
            TagsTableView.ReloadData();
            SetSelected();

            loading.Hide();
        }

        void SetSelected()
        {
            foreach (var tag in Tags)
            {
                foreach (var selectedTag in SelectedTags)
                {
                    if (selectedTag.Id.Equals(tag.Id))
                    {
                        TagsTableView.Source.RowSelected(TagsTableView, NSIndexPath.FromRowSection(Tags.IndexOf(tag), 0));
                        //TagsTableView.SelectRow(NSIndexPath.FromRowSection(Tags.IndexOf(tag), 0), true, UITableViewScrollPosition.None);
                    }
                }
            }
        }

        void SetAllTags(object sender, EventArgs e)
        {
            if (FilterMenuController.TryGetTarget(out var ctl))
            {
                ctl.Tags = new List<TagDto>();
                DismissViewController(true, null);
            }
        }

        void SetTags(object sender, EventArgs e)
        {
            if (FilterMenuController.TryGetTarget(out var ctl))
            {
                ctl.Tags = SelectedTags;
                DismissViewController(true, null);
            }
        }

        public void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            var searchString = searchController.SearchBar.Text;
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var filteredTags = Tags.Where(t => t.Name.ToLower().Contains(searchString.ToLower())).ToList();
                TagsTableView.Source = new TagTableSource(filteredTags, this);
                TagsTableView.ReloadData();
            }
            else
            {
                TagsTableView.Source = new TagTableSource(Tags, this);
                TagsTableView.ReloadData();
            }
        }
    }
}