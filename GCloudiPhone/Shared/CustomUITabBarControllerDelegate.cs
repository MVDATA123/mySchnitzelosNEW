using System;
using UIKit;

namespace GCloudiPhone
{
    public class CustomUITabBarControllerDelegate : UITabBarControllerDelegate
    {
        public override bool ShouldSelectViewController(UITabBarController tabBarController, UIViewController viewController)
        {
            var fromView = tabBarController.SelectedViewController.View;
            var toView = viewController.View;
            if(toView == null) {
                return false;
            }

            //Zakomentarisano jer se pojavljivala greska kada se klikne na "More" tabu kada se klikne na njega
            //if(fromView != toView) {
            //    UIView.Transition(fromView, toView, 0.3, UIViewAnimationOptions.TransitionCrossDissolve, null);
            //}

            return true;
        }
    }
}

