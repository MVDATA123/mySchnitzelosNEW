using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using mvdata.foodjet.Domain;
using Optional;

namespace mvdata.foodjet.Adapter
{
    public class StoreSpinnerAdapter : AbstractSpinnerAdapter<StoreLocationDto>
    {
        public StoreSpinnerAdapter(Activity context, IList<StoreLocationDto> stores, Spinner spinner) : base(context, stores, spinner, store => store.Name)
        {
        }
    }
}