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

namespace mvdata.foodjet.Filter
{
    public class TagViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationDateTime { get; set; }
        public bool IsChecked { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}