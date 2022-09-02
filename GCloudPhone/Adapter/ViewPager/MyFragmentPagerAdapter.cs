using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Java.Lang;
using mvdata.foodjet.Filter;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using IFilterable = mvdata.foodjet.Filter.IFilterable;

namespace mvdata.foodjet.Adapter.ViewPager
{
    public class MyFragmentPagerAdapter : FragmentPagerAdapter
    {
        private readonly List<Fragment> _fragments = new List<Fragment>();
        private readonly List<string> _fragmentTitles = new List<string>();

        public MyFragmentPagerAdapter(FragmentManager supportFragmentManager) : base(supportFragmentManager)
        {
        }

        public void AddFragment(Fragment fragment, string fragmentTitle)
        {
            _fragments.Add(fragment);
            _fragmentTitles.Add(fragmentTitle);
        }

        public void AddFilter(AbstractMapFilter filter, bool autoApply = true)
        {
            foreach (var filterable in _fragments.OfType<IFilterable>())
            {
                filterable.AddFilter(filter, autoApply);
            }
        }

        public void ClearFilters()
        {
            foreach (var filterable in _fragments.OfType<IFilterable>())
            {
                filterable.ClearFilter();
            }
        }

        public override int Count => _fragments.Count;
        public override Fragment GetItem(int position)
        {
            return _fragments[position];
        }

        public override ICharSequence GetPageTitleFormatted(int position)
        {

            return new Java.Lang.String(_fragmentTitles[position].ToLower());// display the title
            //return null;// display only the icon
        }
    }
}