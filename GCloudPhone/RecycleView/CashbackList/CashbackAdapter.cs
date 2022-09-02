using System;
using System.Collections.Generic;
using System.Globalization;

using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using GCloud.Shared.Dto.Domain;
using mvdata.foodjet.RecycleView.CashbackList;

namespace mvdata.foodjet.Adapter
{
    public class CashbackAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;

        private readonly List<CashbackDto> _cashbacks;
        public override int ItemCount => _cashbacks.Count;

        public CashbackAdapter(List<CashbackDto> cashback)
        {
            _cashbacks = cashback;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is CashbackViewHolder vh)
            {

                var item = _cashbacks[position];
                var currencyText = item.CreditChange >= 0 ? "+ " : "";
                currencyText += item.CreditChange.ToString("C", CultureInfo.CreateSpecificCulture("de-DE"));
                vh.CashbackIcon.RequestLayout();
                if (item.CreditChange >= 0)
                {
//                    vh.Change.SetTextColor(Color.LightGreen);
                    vh.CashbackIcon.SetImageResource(Resource.Drawable.ic_in);

                }
                else
                {
                    vh.Change.SetTextColor(Color.Red);
                    vh.CashbackIcon.SetImageResource(Resource.Drawable.ic_out);
                }
                vh.CashbackIcon.LayoutParameters.Width = 100;
                vh.CashbackIcon.SetScaleType(ImageView.ScaleType.FitCenter);
                vh.Change.Text = currencyText;
                vh.Date.Text = item.CreditDateTime.ToString("D");
                vh.Time.Text = item.CreditDateTime.ToString("HH:mm");
            }

        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.CashbackRecycleViewItem, parent, false);

            var vh = new CashbackViewHolder(itemView, OnClick);

            return vh;
        }

        void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }
    }
}