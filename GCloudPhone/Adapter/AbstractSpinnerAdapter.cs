using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public abstract class AbstractSpinnerAdapter<TType> : BaseAdapter<TType> where TType : class   {
        private readonly IList<TType> _entities;
        private readonly Activity _context;
        private Option<int> _seletedPosition = Option.None<int>();
        private Func<TType, string> _textExpression;
        private Spinner _spinner;

        public Option<TType> SelectedValue => _seletedPosition.Map(selectedPosition => _entities[selectedPosition]);

        protected AbstractSpinnerAdapter(Activity context, IList<TType> entities, Spinner spinner, Func<TType, string> textProperty)
        {
            _context = context;
            _entities = entities;
            _spinner = spinner;
            _spinner.Adapter = this;
            _spinner.ItemSelected += (sender, args) => _seletedPosition = args.Position.Some();
            _textExpression = textProperty;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = _entities[position];
            var view = convertView ?? _context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleSpinnerItem, parent, false);
            var text = view.FindViewById<TextView>(Android.Resource.Id.Text1);
            text.Text = _textExpression(item);

            _seletedPosition = position.Some();

            return view;
        }

        public override int Count => _entities.Count;

        public override TType this[int position] => _entities[position];

        public override View GetDropDownView(int position, View convertView, ViewGroup parent)
        {
            var item = _entities[position];
            var view = convertView ?? _context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleSpinnerDropDownItem, parent, false);
            var text = view.FindViewById<TextView>(Android.Resource.Id.Text1);
            text.Text = _textExpression(item);

            return view;
        }
    }
}