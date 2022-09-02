using CoreAnimation;
using Foundation;
using GCloud.Shared.Dto;
using GCloudiPhone.Helpers;
using System;
using UIKit;
using CoreGraphics;
using GCloudiPhone.Extensions;

namespace GCloudiPhone
{
    public partial class InvoiceDetailsViewController : UIViewController
    {
        public Invoice Invoice
        {
            get => invoice;
            set
            {
                invoice = value;
                UpdateView();
            }
        }

        private Invoice invoice;
        private ProductTableDataSource productDataSource;
        private PaymentTableDataSource paymentDataSource;
        private TaxTableDataSource taxDataSource;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ContentView.Layer.ShadowRadius = 10;
            ContentView.Layer.ShadowOffset = new CGSize(0, 0);
            ContentView.Layer.ShouldRasterize = false;
            ContentView.Layer.ShadowOpacity = 0.5f;
            ContentView.Layer.MasksToBounds = false;

            var linePattern = new NSNumber[] { 9, 3 };
            var lineWidth = 1;
            var strokeColor = UIColor.Black.CGColor;

            var headerSeparatorLine = new CAShapeLayer
            {
                StrokeColor = strokeColor,
                LineWidth = lineWidth,
                LineDashPattern = linePattern
            };


            var path = new CGPath();
            path.AddLines(new CGPoint[] { new CGPoint(0, 3), new CGPoint(headerSeparator.Frame.Width, 3) });


            headerSeparatorLine.Path = path;
            headerSeparator.Layer.AddSublayer(headerSeparatorLine);

            var infoSeparatorLine = new CAShapeLayer
            {
                StrokeColor = strokeColor,
                LineWidth = lineWidth,
                LineDashPattern = linePattern
            };


            infoSeparatorLine.Path = path;
            infoSeparator.Layer.AddSublayer(infoSeparatorLine);

            var productsSeparatorLine = new CAShapeLayer
            {
                StrokeColor = strokeColor,
                LineWidth = lineWidth,
                LineDashPattern = linePattern
            };


            productsSeparatorLine.Path = path;
            productsSeparator.Layer.AddSublayer(productsSeparatorLine);

            var sumSeparatorLine = new CAShapeLayer
            {
                StrokeColor = strokeColor,
                LineWidth = lineWidth,
                LineDashPattern = linePattern
            };


            sumSeparatorLine.Path = path;
            sumSeparator.Layer.AddSublayer(sumSeparatorLine);

            var paymentSeparatorLine = new CAShapeLayer
            {
                StrokeColor = strokeColor,
                LineWidth = lineWidth,
                LineDashPattern = linePattern
            };

            var paymentPath = new CGPath();
            paymentPath.AddLines(new CGPoint[] { new CGPoint(0, 1), new CGPoint(headerSeparator.Frame.Width, 1) });
            paymentPath.AddLines(new CGPoint[] { new CGPoint(0, 4), new CGPoint(headerSeparator.Frame.Width, 4) });
            paymentSeparatorLine.Path = paymentPath;
            paymentSeparator.Layer.AddSublayer(paymentSeparatorLine);

            var taxSeparatorLine = new CAShapeLayer
            {
                StrokeColor = strokeColor,
                LineWidth = lineWidth,
                LineDashPattern = linePattern
            };


            taxSeparatorLine.Path = path;
            taxSeparator.Layer.AddSublayer(taxSeparatorLine);

            var signatureSeparatorLine = new CAShapeLayer
            {
                StrokeColor = strokeColor,
                LineWidth = lineWidth,
                LineDashPattern = linePattern
            };


            signatureSeparatorLine.Path = path;
            signatureSeparator.Layer.AddSublayer(signatureSeparatorLine);

            productTableView.TableHeaderView = new UIView(new CGRect(0, 0, 0, 0));
            productTableView.TableFooterView = new UIView(new CGRect(0, 0, 0, 0));

            paymentTableView.TableHeaderView = new UIView(new CGRect(0, 0, 0, 0));
            paymentTableView.TableFooterView = new UIView(new CGRect(0, 0, 0, 0));

            taxesTableView.TableHeaderView = new UIView(new CGRect(0, 0, 0, 0));
            taxesTableView.TableFooterView = new UIView(new CGRect(0, 0, 0, 0));
        }

        private void UpdateView()
        {
            LoadViewIfNeeded();
            StoreNameLabel.Text = Invoice.Biller.Address.Name;
            CompanyNameLabel.Text = Invoice.Biller.ComanyName;
            StoreAddressLabel.Text = $"{Invoice.Biller.Address.Street}, {Invoice.Biller.Address.ZIP} {Invoice.Biller.Address.Town}";
            TaxNoLabel.Text = Invoice.Biller.VATIdentificationNumber;

            RegisterIdLabel.Text = "1";
            InvoiceNoLabel.Text = Invoice.InvoiceNumber;
            DateTimeLabel.Text = Invoice.InvoiceDate.ToString("dd.MM.yyyy HH:mm");

            if (productDataSource == null)
            {
                productDataSource = new ProductTableDataSource();
                productTableView.WeakDataSource = productDataSource;
            }
            productDataSource.UpdateTable(productTableView, Invoice.Details.ItemList.ListLineItem);

            ProductTableHeightConstraint.Constant = Invoice.Details.ItemList.ListLineItem.Length * productTableView.RowHeight;

            if (paymentDataSource == null)
            {
                paymentDataSource = new PaymentTableDataSource();
                paymentTableView.WeakDataSource = paymentDataSource;
            }
            paymentDataSource.UpdateTable(paymentTableView, Invoice.PaymentMethods);

            paymentTableViewHeightConstraint.Constant = Invoice.PaymentMethods.Count * paymentTableView.RowHeight;

            if (taxDataSource == null)
            {
                taxDataSource = new TaxTableDataSource();
                taxesTableView.WeakDataSource = taxDataSource;
            }
            taxDataSource.UpdateTable(taxesTableView, Invoice.Tax.VAT);

            taxTableViewHeightConstraint.Constant = Invoice.Tax.VAT.Length * taxesTableView.RowHeight;

            TurnoverLabel.Text = $"{Invoice.InvoiceCurrency} {Invoice.TotalGrossAmount.ToString("F")}";

            if (invoice.JwsSignature != null)
            {
                SignatureImage.Image = invoice.JwsSignature.GenerateQrCode();
            }

            ContentView.LayoutIfNeeded();
            ContentView.Layer.ShadowPath = UIBezierPath.FromRect(ContentView.Bounds).CGPath;
        }

        public InvoiceDetailsViewController(IntPtr handle) : base(handle)
        { }

        partial void ShareInvoiceButton_Activated(UIBarButtonItem sender)
        {
            using (var alertController = UIAlertController.Create("Rechnung exportieren", "Wie möchtest du die Rechnung exportieren?", UIAlertControllerStyle.ActionSheet))
            using (var saveAsImageAction = UIAlertAction.Create("Als Bild speichern", UIAlertActionStyle.Default, ExportImageAction))
            using (var saveAsPdfAction = UIAlertAction.Create("Als PDF speichern", UIAlertActionStyle.Default, ExportPdfAction))
            using (var cancelAction = UIAlertAction.Create("Abbrechen", UIAlertActionStyle.Cancel, null))
            {
                alertController.AddAction(saveAsImageAction);
                alertController.AddAction(saveAsPdfAction);
                alertController.AddAction(cancelAction);

                using (var popoverController = alertController.PopoverPresentationController)
                {
                    if (popoverController != null)
                    {
                        popoverController.BarButtonItem = sender;
                    }
                }

                this.PresentViewController(alertController, true, null);
            }
        }

        void ExportImageAction(UIAlertAction obj)
        {
            ShareData(ContentView.AsImage());
        }

        void ExportPdfAction(UIAlertAction obj)
        {
            ShareData(ContentView.AsPdf());
        }

        private void ShareData(NSObject data)
        {
            var objectsToShare = new[] { data };
            var shareVC = new UIActivityViewController(objectsToShare, null);
            using (var popoverController = shareVC.PopoverPresentationController)
            {
                if (popoverController != null)
                {
                    popoverController.SourceView = this.View;
                }
            }

            this.PresentViewController(shareVC, true, null);
        }

    }
}