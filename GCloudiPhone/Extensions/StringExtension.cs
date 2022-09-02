using CoreGraphics;
using CoreImage;
using Foundation;
using UIKit;

namespace GCloudiPhone.Extensions
{
    public static class StringExtension
    {
        public static UIImage GenerateQrCode(this string data)
        {
            using (var filter = CIFilter.FromName("CIQRCodeGenerator"))
            {
                filter.SetValueForKey(NSData.FromString(data), new NSString("inputMessage"));
                var transform = CGAffineTransform.MakeScale(10, 10);
                using (var output = filter.OutputImage?.ImageByApplyingTransform(transform))
                {
                    return UIImage.FromImage(output);
                }
            }
        }

        public static UIImage GetImageFromBase64(this string base64String)
        {
            return UIImage.LoadFromData(new NSData(base64String, NSDataBase64DecodingOptions.None));
        }
    }
}
