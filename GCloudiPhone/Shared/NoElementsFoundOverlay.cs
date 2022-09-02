using System;
using CoreGraphics;
using UIKit;

namespace GCloudiPhone.Shared
{
    public class NoElementsFoundOverlay : UIView
    {
        UILabel loadingLabel;

        public NoElementsFoundOverlay(CGRect frame) : base(frame)
        {
            // configurable bits
            BackgroundColor = UIColor.White;
            Alpha = 0.75f;
            AutoresizingMask = UIViewAutoresizing.All;

            nfloat labelHeight = 22;
            nfloat labelWidth = Frame.Width - 20;

            // derive the center x and y
            nfloat centerX = Frame.Width / 2;
            nfloat posY = 70;

            // create and configure the "Loading Data" label
            loadingLabel = new UILabel(new CGRect(centerX - (labelWidth / 2), posY, labelWidth, labelHeight))
            {
                BackgroundColor = UIColor.White,
                TextColor = UIColor.Black,
                Text = "Keine Daten gefunden",
                TextAlignment = UITextAlignment.Center,
                AutoresizingMask = UIViewAutoresizing.All
            };
            AddSubview(loadingLabel);
        }

        /// <summary>
        /// Fades out the control and then removes it from the super view
        /// </summary>
        public void Hide()
        {
            Animate(
                0.5, // duration
                             () => { Alpha = 0; },
                RemoveFromSuperview
            );
        }
    }
}

