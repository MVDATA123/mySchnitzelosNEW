using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ZXing;
using ZXing.QrCode;

namespace mvdata.foodjet.Utils
{
    public static class QrCodeUtils
    {
        public static Bitmap GetQrCode(string qrCodeValue, int preferedHeight, int preferedWidth)
        {
            var hints = new Dictionary<EncodeHintType, object>
            {
                {EncodeHintType.CHARACTER_SET, "UTF-8"},
                {EncodeHintType.MARGIN, 2}
            };

            var writer = new QRCodeWriter();
            var bitMatrix = writer.encode(qrCodeValue, BarcodeFormat.QR_CODE, preferedWidth, preferedHeight, hints);

            var height = bitMatrix.Height;
            var width = bitMatrix.Width;

            var pixels = new int[width * height];
            for (var y = 0; y < height; y++)
            {
                int offset = y * width;
                for (var x = 0; x < width; x++)
                {
                    pixels[offset + x] = bitMatrix[x, y] ? Color.Black : Color.White;
                }
            }

            var bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
            bitmap.SetPixels(pixels, 0, width, 0, 0, width, height);

            return bitmap;
        }
    }
}