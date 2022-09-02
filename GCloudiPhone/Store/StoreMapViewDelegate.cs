using System;
using MapKit;
using Foundation;
using UIKit;
using GCloudiPhone.Extensions;
using CoreGraphics;
using GCloudiPhone.Shared;

namespace GCloudiPhone
{
    public class StoreMapViewDelegate : MKMapViewDelegate
    {
        string pId = "StorePinAnnotation";
        private readonly WeakReference<StoreMapViewController> controller;

        public StoreMapViewDelegate(WeakReference<StoreMapViewController> controller)
        {
            this.controller = controller;
        }

        public override MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation)
        {
            if (IsCurrentLocation(mapView, annotation))
            {
                //var userLocationView = mapView.ViewForAnnotation(annotation);
                //userLocationView.CanShowCallout = false;
                return null;
            }

            MKAnnotationView pinView = null;
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                // create pin annotation view
                pinView = (MKMarkerAnnotationView)mapView.DequeueReusableAnnotation(pId);

                if (pinView == null)
                    pinView = new MKMarkerAnnotationView(annotation, pId);
            }
            else
            {
                pinView = (MKPinAnnotationView)mapView.DequeueReusableAnnotation(pId);

                if (pinView == null)
                    pinView = new MKPinAnnotationView(annotation, pId);
            }

            pinView.CanShowCallout = true;
            var detailBtn = UIButton.FromType(UIButtonType.DetailDisclosure);
            detailBtn.TintColor = UIColor.FromRGB(255, 87, 34);
            pinView.RightCalloutAccessoryView = detailBtn;

            var routeBtn = UIButton.FromType(UIButtonType.DetailDisclosure);
            routeBtn.SetImage(UIImage.FromBundle("NavigationIconFilled"), UIControlState.Normal);
            routeBtn.Frame = new CGRect(0, 0, 50, 50);
            routeBtn.TintColor = UIColor.White;
            routeBtn.BackgroundColor = UIColor.FromRGB(255, 87, 34);
            //routeBtn.Frame = new CGRect(0, 0, 50, 50);

            pinView.LeftCalloutAccessoryView = routeBtn;

            return pinView;
        }

        public override void CalloutAccessoryControlTapped(MKMapView mapView, MKAnnotationView view, UIControl control)
        {
            if (control == view.RightCalloutAccessoryView)
            {
                var store = ((StoreMKPointAnnotation)view.Annotation).Store;
                if(controller.TryGetTarget(out var ctl)) {
                    ctl.PerformSegue("PopUpSegue", NSObjectWrapper.Wrap(store));
                }
            }

            if(control == view.LeftCalloutAccessoryView) {
                var storeAddress = Uri.EscapeUriString(((StoreMKPointAnnotation)view.Annotation).Store.Address);
                storeAddress = "https://maps.apple.com/?daddr=" + storeAddress;
                if (UIApplication.SharedApplication.CanOpenUrl(new NSUrl(storeAddress)))
                {
                    UIApplication.SharedApplication.OpenUrl(new NSUrl(storeAddress));
                }
            }
        }

        private bool IsCurrentLocation(MKMapView mapView, IMKAnnotation annotation)
        {
            var userLocationAnnotation = ObjCRuntime.Runtime.GetNSObject(annotation.Handle) as MKUserLocation;
            if (userLocationAnnotation != null)
            {
                return userLocationAnnotation == mapView.UserLocation;
            }

            return false;
        }

        public override void DidSelectAnnotationView(MKMapView mapView, MKAnnotationView view)
        {
            if(IsCurrentLocation(mapView, view.Annotation)) {
                return;
            }

            if (controller.TryGetTarget(out var ctl))
            {
                ctl.StoreMapView_DidSelectAnnotationView(mapView, new MKAnnotationViewEventArgs(view));
            }
        }

        public override void DidDeselectAnnotationView(MKMapView mapView, MKAnnotationView view)
        {
            if (IsCurrentLocation(mapView, view.Annotation))
            {
                return;
            }

            if (controller.TryGetTarget(out var ctl))
            {
                ctl.CloseStoreInfo();
            }
        }
    }
}
