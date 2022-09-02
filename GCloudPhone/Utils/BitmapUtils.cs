using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Renderscripts;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace mvdata.foodjet.Utils
{
    public static class BitmapUtils
    {
        private const float BitmapScale = 0.4f;
        private const float BlurRadius = 2f;

        public static Bitmap Blur(Context context, Bitmap bitmap)
        {
            var width = (int)Math.Round(bitmap.Width * BitmapScale);
            var height = (int)Math.Round(bitmap.Height*  BitmapScale);

            var inputBitmap = Bitmap.CreateScaledBitmap(bitmap, width, height, false);
            var outputBitmap = Bitmap.CreateBitmap(inputBitmap);

            var rs = RenderScript.Create(context);
            var theIntrinsic = ScriptIntrinsicBlur.Create(rs, Element.U8_4(rs));
            var tmpIn = Allocation.CreateFromBitmap(rs, inputBitmap);
            var tmpOut = Allocation.CreateFromBitmap(rs, outputBitmap);
            theIntrinsic.SetRadius(BlurRadius);
            theIntrinsic.SetInput(tmpIn);
            theIntrinsic.ForEach(tmpOut);
            tmpOut.CopyTo(outputBitmap);

            return outputBitmap;
        }
    }
}