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
using Optional;

namespace mvdata.foodjet.Adapter
{
    /// <summary>
    /// In dieser Klasse werden die Keys im Spinner angezeigt, und die Values werden als SelectedValue zurückgegeben
    /// </summary>
    public class DictionarySpinnerAdapter : BaseAdapter<string>
    {
        private readonly Dictionary<string, string> _dictionary;
        private readonly Activity _context;
        private Option<int> _seletedPosition = 0.None();
        private Spinner _spinner;

        public Option<string> SelectedValue => _seletedPosition.Map(selectedPosition => _dictionary[_dictionary.Keys.ElementAt(selectedPosition)]);
        public Option<string> SelectedKey => _seletedPosition.Map(selectedPosition => _dictionary.Keys.ElementAt(selectedPosition));

        public DictionarySpinnerAdapter(Activity context, Dictionary<string, string> dictionary, Spinner spinner)
        {
            _context = context;
            _dictionary = dictionary;
            _spinner = spinner;
            _spinner.Adapter = this;
            _spinner.ItemSelected += (sender, args) => _seletedPosition = args.Position.Some();
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ?? _context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleSpinnerItem, parent, false);
            var text = view.FindViewById<TextView>(Android.Resource.Id.Text1);
            text.Text = _dictionary[_dictionary.Keys.ElementAt(position)];

            return view;
        }

        public override int Count => _dictionary.Count;

        /// <summary>
        /// Gibt den Key an der angegebenen Position zurück
        /// </summary>
        /// <param name="position">Die Position</param>
        /// <returns></returns>
        public override string this[int position] => _dictionary.Keys.ElementAt(position);

        public override View GetDropDownView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ?? _context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleSpinnerDropDownItem, parent, false);
            var text = view.FindViewById<TextView>(Android.Resource.Id.Text1);
            text.Text = _dictionary[_dictionary.Keys.ElementAt(position)];

            return view;
        }
    }
}