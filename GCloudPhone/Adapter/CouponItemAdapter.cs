using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using GCloud.Shared.Dto.Domain;
using GCloudShared.Service;
using GCloudShared.Shared;
using Refit;

namespace mvdata.foodjet.Adapter
{
    class CouponItemAdapter : BaseAdapter<CouponDto>
    {
        private List<CouponDto> items;
        private Activity context;
        private IUserCouponService _userCouponService;


        public CouponItemAdapter(Activity context, List<CouponDto> items)
        {
            this.items = items;
            this.context = context;
            _userCouponService = RestService.For<IUserCouponService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            var view = convertView ?? context.LayoutInflater.Inflate(Resource.Layout.CouponListViewItem, null);

            using (var stream = _userCouponService.GetCouponImage(item.Id.ToString()))
            {
                var image = BitmapFactory.DecodeStream(stream.Result);
                view.FindViewById<TextView>(Resource.Id.txtCouponListViewItem).Text = item.Name;
                view.FindViewById<TextView>(Resource.Id.txtCouponListViewDescription).Text = item.ShortDescription;
                var imageView = view.FindViewById<ImageView>(Resource.Id.imageViewCouponListViewItem);
                if (image != null)
                {
                    imageView.SetImageBitmap(image);
                }
                else
                {
                    imageView.SetImageResource(Resource.Drawable.No_image_available);
                }
                imageView.LayoutParameters.Height = 175;
                imageView.LayoutParameters.Width = 175;
            }
            return view;
        }

        public override int Count => items.Count;

        public override CouponDto this[int position] => items[position];
    }
}