using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Constraints;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using GCloudShared.Extensions;
using GCloudShared.Service;
using GCloudShared.Shared;
using mvdata.foodjet.Adapter;
using mvdata.foodjet.Control;
using mvdata.foodjet.Domain;
using mvdata.foodjet.Settings;
using Optional;
using Refit;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace mvdata.foodjet
{
    [Activity(Label = "ManagerReportSelectionActivity", ParentActivity = typeof(Dashboard))]
    public class ManagerReportSelectionActivity : AppCompatActivity
    {
        private ConstraintLayout _mainRoot;

        private Toolbar _toolbar;
        private ProgressBar _progressBar;
        private Spinner _spinnerReports, _spinnerStores;
        private DictionarySpinnerAdapter _spinnerReportsAdapter;
        private StoreSpinnerAdapter _spinnerStoresAdapter;
        private Button _btnManagerReportSelectionConfirm;
        private TextView _txtManagerReportSelectionDateFrom, _txtManagerReportSelectionDateTo;
        private readonly List<StoreLocationDto> _stores = new List<StoreLocationDto>();
        private readonly Dictionary<string, string> _reports = new Dictionary<string, string>{{ "CouponUsages", "Gutschein-Verbrauch" }, { "CouponUsagePerTime", "Gutschein-Verbrauch nach Zeit" }, { "CouponUserUsages", "Gutschein-Verbrauch nach Benutzer" } };

        private bool _datePickershown;
        private object _datePickerSync = new object();
        private IUserStoreService _userStoreService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_manager_report_selection);

            _mainRoot = FindViewById<ConstraintLayout>(Resource.Id.managerReportSelectionRootView);
            _toolbar = FindViewById<Toolbar>(Resource.Id.app_bar);
            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBarManagerReportSelection);

            _txtManagerReportSelectionDateFrom = FindViewById<TextView>(Resource.Id.txtManagerReportSelectionDateFrom);
            _txtManagerReportSelectionDateFrom.Touch += ShowDatePickerDialog;
            _txtManagerReportSelectionDateFrom.Enabled = false;

            _txtManagerReportSelectionDateTo = FindViewById<TextView>(Resource.Id.txtManagerReportSelectionDateTo);
            _txtManagerReportSelectionDateTo.Touch += ShowDatePickerDialog;
            _txtManagerReportSelectionDateTo.Enabled = false;

            _spinnerReports = FindViewById<Spinner>(Resource.Id.spinnerManagerReportSelectionReportName);
            _spinnerReportsAdapter = new DictionarySpinnerAdapter(this, _reports, _spinnerReports);

            _spinnerStores = FindViewById<Spinner>(Resource.Id.spinnerManagerReportSelectionStore);
            _spinnerStoresAdapter = new StoreSpinnerAdapter(this, _stores, _spinnerStores);

            _btnManagerReportSelectionConfirm = FindViewById<Button>(Resource.Id.btnManagerReportSelectionConfirm);
            _btnManagerReportSelectionConfirm.Click += delegate { LoadReport(); };
            _btnManagerReportSelectionConfirm.Enabled = false;

            _userStoreService = RestService.For<IUserStoreService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);

            Task.Run(async () =>
            {
                try
                {
                    var stores = (await _userStoreService.GetManagerStores()).Select(s => new StoreLocationDto(s))
                        .ToList();
                    RunOnUiThread(() =>
                    {
                        _stores.AddRange(stores);
                        _spinnerStoresAdapter.NotifyDataSetChanged();
                        _btnManagerReportSelectionConfirm.Enabled = true;
                        _progressBar.Visibility = ViewStates.Gone;
                        _txtManagerReportSelectionDateFrom.Enabled = true;
                        _txtManagerReportSelectionDateTo.Enabled = true;
                    });
                }
                catch (ApiException apiException)
                {
                    apiException.ShowApiErrorResultSnackbar(_mainRoot);
                }
            });
        }

        private void ShowDatePickerDialog(object sender, EventArgs e)
        {
            if (!_datePickershown && sender is TextView textView)
            {
                _datePickershown = true;
                lock (_datePickerSync)
                {
                    var frag = DatePickerFragment.NewInstance(date =>
                    {
                        textView.Text = date.ToString("dd.MM.yyyy");
                        _datePickershown = false;
                    });
                    frag.Dismissed += delegate { _datePickershown = false; };
                    frag.Canceled += delegate { _datePickershown = false; };
                    frag.Show(SupportFragmentManager, DatePickerFragment.TAG);
                }
            }
        }

        private void LoadReport()
        {
            var dateFromString = _txtManagerReportSelectionDateFrom.Text;
            var dateToString = _txtManagerReportSelectionDateTo.Text;

            if (DateTime.TryParse(dateFromString, out _) && DateTime.TryParse(dateToString, out _) && _spinnerStoresAdapter.SelectedValue.HasValue && _spinnerReportsAdapter.SelectedValue.HasValue)
            {
                var intent = new Intent(this, typeof(WebviewActivity));
                intent.PutExtra("DataString", "ManagerReports");
                intent.PutExtra("dateFrom", dateFromString);
                intent.PutExtra("dateTo", dateToString);
                _spinnerStoresAdapter.SelectedValue.MatchSome(store => intent.PutExtra("storeGuid", store.Id.ToString()));
                _spinnerReportsAdapter.SelectedKey.MatchSome(selectedKey => intent.PutExtra("HomeUrlPart", selectedKey));
                StartActivity(intent);
            }
            else
            {
                Snackbar.Make(_mainRoot, Resource.String.generalErrorMessage, Snackbar.LengthLong).Show();
            }
            
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetSupportActionBar(_toolbar);
            SupportActionBar.SetTitle(Resource.String.managerReportSelectionActivityHeader);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
        }
    }
}