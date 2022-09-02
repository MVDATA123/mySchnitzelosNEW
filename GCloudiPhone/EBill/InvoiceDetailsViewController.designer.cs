// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace GCloudiPhone
{
    [Register ("InvoiceDetailsViewController")]
    partial class InvoiceDetailsViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel CompanyNameLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView ContentView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel DateTimeLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView headerSeparator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView infoSeparator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel InvoiceNoLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView paymentSeparator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView paymentTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint paymentTableViewHeightConstraint { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView productsSeparator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint ProductTableHeightConstraint { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView productTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel RegisterIdLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIBarButtonItem shareInvoiceButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView SignatureImage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView signatureSeparator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel StoreAddressLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel StoreNameLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView sumSeparator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView taxesTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel TaxNoLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView taxSeparator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint taxTableViewHeightConstraint { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel TurnoverLabel { get; set; }

        [Action ("ShareInvoiceButton_Activated:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void ShareInvoiceButton_Activated (UIKit.UIBarButtonItem sender);

        void ReleaseDesignerOutlets ()
        {
            if (CompanyNameLabel != null) {
                CompanyNameLabel.Dispose ();
                CompanyNameLabel = null;
            }

            if (ContentView != null) {
                ContentView.Dispose ();
                ContentView = null;
            }

            if (DateTimeLabel != null) {
                DateTimeLabel.Dispose ();
                DateTimeLabel = null;
            }

            if (headerSeparator != null) {
                headerSeparator.Dispose ();
                headerSeparator = null;
            }

            if (infoSeparator != null) {
                infoSeparator.Dispose ();
                infoSeparator = null;
            }

            if (InvoiceNoLabel != null) {
                InvoiceNoLabel.Dispose ();
                InvoiceNoLabel = null;
            }

            if (paymentSeparator != null) {
                paymentSeparator.Dispose ();
                paymentSeparator = null;
            }

            if (paymentTableView != null) {
                paymentTableView.Dispose ();
                paymentTableView = null;
            }

            if (paymentTableViewHeightConstraint != null) {
                paymentTableViewHeightConstraint.Dispose ();
                paymentTableViewHeightConstraint = null;
            }

            if (productsSeparator != null) {
                productsSeparator.Dispose ();
                productsSeparator = null;
            }

            if (ProductTableHeightConstraint != null) {
                ProductTableHeightConstraint.Dispose ();
                ProductTableHeightConstraint = null;
            }

            if (productTableView != null) {
                productTableView.Dispose ();
                productTableView = null;
            }

            if (RegisterIdLabel != null) {
                RegisterIdLabel.Dispose ();
                RegisterIdLabel = null;
            }

            if (shareInvoiceButton != null) {
                shareInvoiceButton.Dispose ();
                shareInvoiceButton = null;
            }

            if (SignatureImage != null) {
                SignatureImage.Dispose ();
                SignatureImage = null;
            }

            if (signatureSeparator != null) {
                signatureSeparator.Dispose ();
                signatureSeparator = null;
            }

            if (StoreAddressLabel != null) {
                StoreAddressLabel.Dispose ();
                StoreAddressLabel = null;
            }

            if (StoreNameLabel != null) {
                StoreNameLabel.Dispose ();
                StoreNameLabel = null;
            }

            if (sumSeparator != null) {
                sumSeparator.Dispose ();
                sumSeparator = null;
            }

            if (taxesTableView != null) {
                taxesTableView.Dispose ();
                taxesTableView = null;
            }

            if (TaxNoLabel != null) {
                TaxNoLabel.Dispose ();
                TaxNoLabel = null;
            }

            if (taxSeparator != null) {
                taxSeparator.Dispose ();
                taxSeparator = null;
            }

            if (taxTableViewHeightConstraint != null) {
                taxTableViewHeightConstraint.Dispose ();
                taxTableViewHeightConstraint = null;
            }

            if (TurnoverLabel != null) {
                TurnoverLabel.Dispose ();
                TurnoverLabel = null;
            }
        }
    }
}