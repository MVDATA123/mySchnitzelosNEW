using System;
using UIKit;
using Foundation;
using CoreGraphics;

namespace GCloudiPhone.Shared
{
    public class ColoredUISearchController : UISearchController
    {
        public ColoredUISearchController(UIViewController searchResultsController) : base(searchResultsController)
        {
            SearchBar.TintColor = UIColor.White;
            //SearchBar.SetValueForKey(new NSString("Abbrechen"), new NSString("_cancelButtonText"));

            //if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            //{
            //    UITextField textField = SearchBar.ValueForKey(new NSString("_searchField")) as UITextField;
            //    textField.BackgroundColor = UIColor.White;
            //    textField.Opaque = false;
            //}
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();


        }
    }
}
