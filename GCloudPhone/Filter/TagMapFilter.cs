using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using mvdata.foodjet.Domain;
using mvdata.foodjet.Extensions;
using Optional;

namespace mvdata.foodjet.Filter
{
    public class TagMapFilter : AbstractMapFilter
    {
        private readonly IEnumerable<string> _tagsToApply;

        public TagMapFilter(IEnumerable<string> tagsToApply)
        {
            _tagsToApply = tagsToApply;
        }

        public override bool IsVisible(Marker marker)
        {
            if (_tagsToApply.Any())
            {
                var store = marker.GetStore();

                return store.Map(s => s.Tags.Aggregate(false, (x, y) => x || _tagsToApply.Contains(y.Name))).ValueOr(true);
            }

            return true;
        }

        public override bool IsVisible(StoreLocationDto store)
        {
            if (_tagsToApply.Any())
            {
                return store.SomeNotNull().Map(s => s.Tags.Aggregate(false, (x, y) => x || _tagsToApply.Contains(y.Name))).ValueOr(true);
            }

            return true;
        }
    }
}