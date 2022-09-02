
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Support.V7.Widget;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

using GCloud.Shared.Dto;
using GCloudShared.Shared;
using GCloudShared.Service;
using Refit;
using mvdata.foodjet.RecycleView.BillItems;
using mvdata.foodjet.RecycleView.BillPayments;
using mvdata.foodjet.RecycleView.BillTaxes;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using mvdata.foodjet.Utils;

namespace mvdata.foodjet
{
    [Activity(Label = "", ScreenOrientation = ScreenOrientation.Portrait)]
    public class BillDetailsActivity : BaseActivity
    {
        private TextView _storeName;
        private TextView _companyName;
        private TextView _storeAddress;
        private TextView _companyVatId;
        private TextView _terminalId;
        private TextView _InvoiceNo;
        private TextView _invoiceDate;
        private TextView _invoiceTime;
        private TextView _invoiceSumGross;

        private ProgressBar _progressBar;
        private ScrollView _scrollView;

        private ImageView _signature;

        private RecyclerView _billItemsRecylerView;
        private RecyclerView _billTaxesRecyclerView;
        private RecyclerView _billPaymentsRecyclerView;
        private RecyclerView.LayoutManager _layoutManagerBillItems;
        private RecyclerView.LayoutManager _layoutManagerTaxes;
        private RecyclerView.LayoutManager _layoutManagerPayments;
        private Toolbar _toolbar;


        private IBillService _billService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _billService = RestService.For<IBillService>(HttpClientContainer.Instance.HttpClient);
            var billGuid = Intent.GetStringExtra("billGuid");
            LoadInvoice(Guid.Parse(billGuid));

            SetContentView(Resource.Layout.BillDetails);

            _toolbar = FindViewById<Toolbar>(Resource.Id.app_bar);
            _storeName = FindViewById<TextView>(Resource.Id.billDetailsStoreName);
            _companyName = FindViewById<TextView>(Resource.Id.billDetailsCompanyName);
            _storeAddress = FindViewById<TextView>(Resource.Id.billDetailsStoreAddress);
            //_storePhone = FindViewById<TextView>(Resource.Id.billDetailsStorePhone);
            _companyVatId = FindViewById<TextView>(Resource.Id.billDetailsCompanyAtu);
            _terminalId = FindViewById<TextView>(Resource.Id.billDetailsTerminalIdTxt);
            _InvoiceNo = FindViewById<TextView>(Resource.Id.billDetailsInvoiceNoTxt);
            _invoiceDate = FindViewById<TextView>(Resource.Id.billDetailsDateTxt);
            _invoiceTime = FindViewById<TextView>(Resource.Id.billDetailsTimeTxt);
            _invoiceSumGross = FindViewById<TextView>(Resource.Id.billDetailsSum);

            _signature = FindViewById<ImageView>(Resource.Id.billDetailsQrCode);

            _billItemsRecylerView = FindViewById<RecyclerView>(Resource.Id.billItemsRecyclerView);
            _billPaymentsRecyclerView = FindViewById<RecyclerView>(Resource.Id.billPaymentsRecyclerView);
            _billTaxesRecyclerView = FindViewById<RecyclerView>(Resource.Id.billTaxesRecyclerView);

            _progressBar = FindViewById<ProgressBar>(Resource.Id.billDetailsProgressBar);

            _scrollView = FindViewById<ScrollView>(Resource.Id.billDetailsScrollView);

            _layoutManagerBillItems = new LinearLayoutManager(this);
            _layoutManagerPayments = new LinearLayoutManager(this);
            _layoutManagerTaxes = new LinearLayoutManager(this);

            SetSupportActionBar(_toolbar);
        }

        private async void LoadInvoice(Guid id)
        {
            Invoice invoice = null;
            try
            {
                invoice = (await _billService.GetById(id)).Invoice;
            }
            catch (Exception)
            {
                invoice = JsonConvert.DeserializeObject<Invoice>(@"{
  'InvoiceNumber': '6',
  'InvoiceDate': '2018-10-24T15:06:01',
  'Delivery': null,
  'Biller': {
    'VATIdentificationNumber': 'ATU46004700',
    'Address': {
      'Salutation': null,
      'Name': 'Zentrale',
      'Street': 'Steingasse 7',
      'Town': 'Wien',
      'ZIP': 1030,
      'Country': {
        'CountryCode': null,
        'Value': 'Austria'
      },
      'Email': null,
      'Contact': null
    },
    'InvoiceRecipientsBillerID': 0,
    'ComanyName': 'MV-Data Datenverarbeitung Ges.m.b.H'
  },
  'InvoiceRecipient': null,
  'Details': {
    'HeaderDescription': null,
    'ItemList': {
      'HeaderDescription': null,
      'ListLineItem': [
        {
          'Description': 'VOLLKORNLAIB 1 kg',
          'Quantity': {
            'Unit': null,
            'Value': 1
          },
          'UnitPrice': 4.52,
          'VATRate': 10,
          'InvoiceRecipientsOrderReference': null,
          'LineItemAmount': 4.52
        },
        {
          'Description': 'VOLLKORNKASTEN 1kg',
          'Quantity': {
            'Unit': null,
            'Value': 1
          },
          'UnitPrice': 4.54,
          'VATRate': 10,
          'InvoiceRecipientsOrderReference': null,
          'LineItemAmount': 4.54
        },
        {
          'Description': 'VOLLKORNLAIB 0,5 kg',
          'Quantity': {
            'Unit': null,
            'Value': 1
          },
          'UnitPrice': 2.31,
          'VATRate': 10,
          'InvoiceRecipientsOrderReference': null,
          'LineItemAmount': 2.31
        },
        {
          'Description': 'VOLLKORNLAIB AKTION',
          'Quantity': {
            'Unit': null,
            'Value': 1
          },
          'UnitPrice': 4.18,
          'VATRate': 10,
          'InvoiceRecipientsOrderReference': null,
          'LineItemAmount': 4.18
        }
      ],
      'FooterDescription': null
    },
    'FooterDescription': null
  },
  'Tax': {
    'VAT': [
      {
        'TaxedAmount': 0.0,
        'VATRate': 10.00,
        'Amount': 1.4136363636363636363636363636
      }
    ]
  },
  'TotalGrossAmount': 15.5500,
  'PayableAmount': 15.5500,
  'PaymentMethods': [
    {
      'Comment': 'BAR                 ',
      'Amount': 15.55,
      'UniversalBankTransaction': null
    }
  ],
  'PaymentConditions': null,
  'Comment': null,
  'Language': null,
  'InvoiceCurrency': '€',
  'DocumentType': null,
  'GeneratingSystem': null,
  'JwsSignature': 'eyJhbGciOiJFUzI1NiJ9.X1IxLUFUMl8xXzZfMjAxOC0xMC0yNFQxNTowNjowMF8wLDAwXzE1LDU1XzAsMDBfMCwwMF8wLDAwX3pPWERhdG89XzAxQzI3RUU2MjYxMDNDREJENkJBNDczNDU3Xy9VZGRuSW5QeDZnPQ.-wgyLowJar93f3a9LygaKhWFOiYUZHNpPXZco2ZKudcNWQWxFKMog5_P1ZbZ1EMQvdNT1qZOzIzOaj3b09x-kA'
}");
            }
            RunOnUiThread(() => SetData(invoice));
        }

        private void SetData(Invoice invoice)
        {
            _billItemsRecylerView.SetLayoutManager(_layoutManagerBillItems);
            var billItemsAdapter = new BillItemsAdapter(this, invoice.Details.ItemList.ListLineItem.ToList());
            _billItemsRecylerView.SetAdapter(billItemsAdapter);

            _billPaymentsRecyclerView.SetLayoutManager(_layoutManagerPayments);
            var billPaymentsAdapter = new BillPaymentsAdapter(this, invoice.PaymentMethods);
            _billPaymentsRecyclerView.SetAdapter(billPaymentsAdapter);

            _billTaxesRecyclerView.SetLayoutManager(_layoutManagerTaxes);
            var billTaxesAdapter = new BillTaxesAdapter(this, invoice.Tax.VAT.ToList());
            _billTaxesRecyclerView.SetAdapter(billTaxesAdapter);

            _storeName.Text = invoice.Biller.Address.Name.Trim();
            _companyName.Text = invoice.Biller.ComanyName.Trim();
            _storeAddress.Text = $"{invoice.Biller.Address.Street.Trim()}, {invoice.Biller.Address.ZIP} {invoice.Biller.Address.Town.Trim()}";
            _companyVatId.Text = invoice.Biller.VATIdentificationNumber.Trim();

            _terminalId.Text = invoice.Biller.InvoiceRecipientsBillerID.ToString();
            _InvoiceNo.Text = invoice.InvoiceNumber;
            _invoiceDate.Text = invoice.InvoiceDate.ToString("dd.MM.yyyy");
            _invoiceTime.Text = invoice.InvoiceDate.ToString("HH:mm");

            _invoiceSumGross.Text = invoice.TotalGrossAmount.ToString("0.##");

            if (!string.IsNullOrWhiteSpace(invoice.JwsSignature))
            {
                _signature.SetImageBitmap(QrCodeUtils.GetQrCode(invoice.JwsSignature, 500, 500));
            }

            _progressBar.Visibility = ViewStates.Gone;
            _scrollView.Visibility = ViewStates.Visible;
        }
    }
}
