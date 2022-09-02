using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V4.Content.Res;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Util;
using Firebase.Messaging;
using GCloud.Shared.Dto.Domain;
using mvdata.foodjet.Adapter;
using GCloudShared.Domain;
using GCloudShared.Extensions;
using GCloudShared.Repository;
using GCloudShared.Service;
using GCloudShared.Shared;
using Java.Lang;
using mvdata.foodjet.Caching;
using mvdata.foodjet.RecycleView;
using mvdata.foodjet.RecycleView.BillsList;
using Optional;
using Optional.Collections;
using Refit;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Exception = Java.Lang.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using SearchView = Android.Support.V7.Widget.SearchView;
using Thread = System.Threading.Thread;
using System.IO;
using Newtonsoft.Json;

namespace mvdata.foodjet
{
    [Activity(Label = "Rechnungen", LaunchMode = LaunchMode.SingleTop, ScreenOrientation = ScreenOrientation.Portrait)]
    [MetaData("android.app.searchable", Resource = "@xml/bill_searchable")]
    [IntentFilter(new[] { Intent.ActionSearch })]
    public class BillsListActivity : BaseActivity, SearchView.IOnQueryTextListener,
        AppBarLayout.IOnOffsetChangedListener
    {
        private ListView _listViewBillsList;
        private RecyclerView _recyclerView;
        private Toolbar _toolbar;
        private CoordinatorLayout _mainRoot;
        private SwipeRefreshLayout _swipeRefreshLayout;
        private ImageView _imageViewIconSmall;
        private Button _btnBillsListEnableNotifications;
        private AppBarLayout _appBarLayout;
        private CollapsingToolbarLayout _collapsingToolbarLayout;
        private TextView _txtBillsListHeader;
        private BillsAdapter _billsAdapter;
        private RecyclerView.LayoutManager _layoutManager;
        private FloatingActionButton _fab;

        private IBillService _billService;

        private List<Bill_Out_Dto> _bills = new List<Bill_Out_Dto>();
        private string _invoiceNumber;
        private bool _toolbarIsShow = true;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.BillsList);

            _mainRoot = FindViewById<CoordinatorLayout>(Resource.Id.bills_list_main_root);
            _toolbar = FindViewById<Toolbar>(Resource.Id.bills_list_toolbar);

            _invoiceNumber = Intent.GetStringExtra("invoiceNumber");

            _appBarLayout = FindViewById<AppBarLayout>(Resource.Id.bills_list_app_bar_layout);
            _collapsingToolbarLayout = FindViewById<CollapsingToolbarLayout>(Resource.Id.bills_list_collapsing_toolbar_layout);
            _appBarLayout.AddOnOffsetChangedListener(this);

            _swipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.bills_list_swipe_refresh_layout);
            _txtBillsListHeader = FindViewById<TextView>(Resource.Id.bills_list_header);

            _billService = RestService.For<IBillService>(HttpClientContainer.Instance.HttpClient);
            _swipeRefreshLayout.Refresh += RefreshBills;

            _fab = FindViewById<FloatingActionButton>(Resource.Id.floating_action_button);

            _recyclerView = FindViewById<RecyclerView>(Resource.Id.bills_list_recycler_view);
            _layoutManager = new LinearLayoutManager(this);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.AddItemDecoration(new DividerItemDecoration(_recyclerView.Context, (int)Orientation.Vertical));
            var sizeProvider = new FixedPreloadSizeProvider(48, 48);

            RefreshBills(null, null);
        }

        private async void RefreshBills(object sender, EventArgs e)
        {
            await LoadBills();
            CachingHolder.Instance.SetBills(_bills);
        }


        public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset)
        {
            int scrollRange = -1;
            if (scrollRange == -1)
            {
                scrollRange = appBarLayout.TotalScrollRange;
            }

            if (scrollRange + verticalOffset == 0)
            {
                _collapsingToolbarLayout.Title = $"{_invoiceNumber}";
                _toolbarIsShow = true;
                _txtBillsListHeader.Visibility = ViewStates.Invisible;
            }
            else if (_toolbarIsShow)
            {
                _collapsingToolbarLayout.Title =
                    " "; //carefull there should a space between double quote otherwise it wont work 
                _toolbarIsShow = false;
                _txtBillsListHeader.Visibility = ViewStates.Visible;
            }
        }

        public override void OnShowNoInternetMessageSuccess()
        {
            base.OnShowNoInternetMessageSuccess();
            Finish();
        }

        protected override void OnResume()
        {
            SetSupportActionBar(_toolbar);
            if (_invoiceNumber != null)
            {
                _txtBillsListHeader.Text = $"{_invoiceNumber}";
                SupportActionBar.Title = $"{_invoiceNumber}";
            }
            else
            {
                _txtBillsListHeader.Text = $" ";
                SupportActionBar.Title = $" ";
            }

            base.OnResume();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.BillsListMenu, menu);

            // Associate searchable configuration with the SearchView
            var searchManager =
                (SearchManager)GetSystemService(Context.SearchService);
            var searchView =
                (SearchView)menu.FindItem(Resource.Id.BillsListMenuSearch).ActionView;
            searchView.SetSearchableInfo(searchManager.GetSearchableInfo(ComponentName));
            searchView.SetOnQueryTextListener(this);
            menu.FindItem(Resource.Id.BillsListMenuSearch).SetVisible(true);


            return true;
        }

        private void ListViewBillsListOnItemClick(object sender, Bill_Out_Dto bill)
        {
            // todo export here
            var intent = new Intent(this, typeof(BillDetailsActivity));
            intent.PutExtra("billGuid", bill.Id.ToString());
            StartActivity(intent);

            //ExportInvoice(bill);
        }

        private async Task LoadBills()
        {
            await Task.Run(async () =>
            {
                RunOnUiThread(() => _swipeRefreshLayout.Refreshing = true);
                List<Bill_Out_Dto> newBills = new List<Bill_Out_Dto>();
                try
                {
                    newBills = await _billService.Get(null);
                }
                catch (ApiException e)
                {
                    if (e.StatusCode == 0)
                    {
                        return;
                    }
                }

                LoadItemsInAdapter(newBills);
                RunOnUiThread(() => _swipeRefreshLayout.Refreshing = false);

            });
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            return true;
        }

        private void LoadItemsInAdapter(List<Bill_Out_Dto> bills)
        {
            var newBills = new List<Bill_Out_Dto>(bills);
            _bills.Clear();
            _bills.AddRange(newBills);
            _billsAdapter = new BillsAdapter(this, _bills);
            _billsAdapter.ItemClick += ListViewBillsListOnItemClick;
            RunOnUiThread(() =>
            {
                _recyclerView.SetAdapter(_billsAdapter);
            });
        }

        private void SearchBill(string query)
        {
            new Thread(new ThreadStart(delegate
            {
                RunOnUiThread(async () =>
                {
                    if (_bills == null)
                    {
                        _bills = await _billService.Get(null);
                    }

                    var searchResult = _bills.Where(c =>
                        c.Invoice.Biller.ComanyName.ToUpper().Contains(query.ToUpper())
                        || c.Invoice.InvoiceNumber.ToUpper().Contains(query.ToUpper())
                        || c.Invoice.InvoiceDate.ToLocalTime().ToString().ToUpper().Contains(query.ToUpper())).ToList();
                    LoadItemsInAdapter(searchResult);
                });
            })).Start();
        }

        public bool OnQueryTextChange(string newText)
        {
            SearchBill(newText);
            return true;
        }

        public bool OnQueryTextSubmit(string query)
        {
            SearchBill(query);
            return true;
        }


        private void ExportInvoice(Bill_Out_Dto bill)
        {
            var file = SaveToFile(bill.Invoice.InvoiceNumber, bill.Invoice.ToJson());

            Share(bill.Invoice.InvoiceNumber, file);
        }


        private void Share(string invoiceNumber, string filepath)
        {
            var intent = new Intent(Intent.ActionSend);
            intent.SetType("text/json");

            intent.PutExtra(Intent.ExtraStream, Android.Net.Uri.Parse("file://" + filepath));
            intent.PutExtra(Intent.ExtraText, string.Empty);
            intent.PutExtra(Intent.ExtraSubject, invoiceNumber ?? string.Empty);

            var chooserIntent = Intent.CreateChooser(intent, "Share eBill" ?? string.Empty);
            chooserIntent.SetFlags(ActivityFlags.ClearTop);
            chooserIntent.SetFlags(ActivityFlags.NewTask);
            Android.App.Application.Context.StartActivity(chooserIntent);
        }


        private string SaveToFile(string invoiceNumber, string invoiceXml)
        {
            var fileName = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), $"{invoiceNumber}.xml");
            File.WriteAllText(fileName, invoiceXml);
            return fileName;
        }
    }
}