using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Constraints;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using GCloud.Shared.Dto.Api;
using GCloud.Shared.Dto.Domain;
using GCloudShared.Extensions;
using GCloudShared.Service;
using GCloudShared.Shared;
using Refit;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace mvdata.foodjet
{
    [Activity(Label = "ManagerEditStoreActivity", ParentActivity = typeof(ManagerStoresList), ScreenOrientation = ScreenOrientation.Portrait)]
    public class ManagerEditStoreActivity : BaseActivity
    {
        private ConstraintLayout _mainRoot;
        private ProgressBar _progressBar;
        private Toolbar _toolbar;
        private AppCompatEditText _txtStoreName, _txtStoreStreet, _txtStoreHouseNr, _txtStorePostCode, _txtStoreCity;
        private Button _btnApplyChanges;
        private StoreManagerEditModel _store;
        private IStoreService _storeService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.ManagerStoreEdit);

            _mainRoot = FindViewById<ConstraintLayout>(Resource.Id.mainRootEditStore);
            _toolbar = FindViewById<Toolbar>(Resource.Id.app_bar);
            _txtStoreName = FindViewById<AppCompatEditText>(Resource.Id.txtEditStoreName);
            _txtStoreStreet = FindViewById<AppCompatEditText>(Resource.Id.txtEditStoreStreet);
            _txtStoreHouseNr = FindViewById<AppCompatEditText>(Resource.Id.txtEditStoreHouseNr);
            _txtStorePostCode = FindViewById<AppCompatEditText>(Resource.Id.txtEditStorePostCode);
            _txtStoreCity = FindViewById<AppCompatEditText>(Resource.Id.txtEditStoreCity);
            _btnApplyChanges = FindViewById<Button>(Resource.Id.btnEditStoreSave);
            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBarStoreEdit);
            _storeService = RestService.For<IStoreService>(HttpClientContainer.Instance.HttpClient, HttpClientContainer.Instance.RefitSettings);

            if (Intent.GetStringExtra("store").TryDeserializeObject<StoreDto>(out var store))
            {
                _store = new StoreManagerEditModel()
                {
                    Name = store.Name,
                    Street = store.Street,
                    HouseNr = store.HouseNr,
                    PostCode = store.Plz,
                    City = store.City,
                    Id = store.Id
                };
            }
            else
            {
                Finish();
                return;
            }

            _txtStoreName.Text = _store.Name;
            _txtStoreStreet.Text = _store.Street;
            _txtStoreHouseNr.Text = _store.HouseNr;
            _txtStorePostCode.Text = _store.PostCode;
            _txtStoreCity.Text = _store.City;
            _btnApplyChanges.Click += delegate
            {
                PersistStore();
            };

            _progressBar.Visibility = ViewStates.Gone;
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetSupportActionBar(_toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetTitle(Resource.String.EditStoreActivityHeader);
        }

        private async void PersistStore()
        {
            if (_store != null)
            {
                RunOnUiThread(() => _progressBar.Visibility = ViewStates.Visible);
                try
                {
                    _store = new StoreManagerEditModel
                    {
                        City = _txtStoreCity.Text,
                        Id = _store.Id,
                        HouseNr = _txtStoreHouseNr.Text,
                        PostCode = _txtStorePostCode.Text,
                        Name = _txtStoreName.Text,
                        Street = _txtStoreStreet.Text
                    };
                    await _storeService.UpdateStore(_store);
                    Snackbar.Make(_mainRoot, Resource.String.changesSaved, Snackbar.LengthLong).Show();
                }
                catch (ApiException ex)
                {
                    ex.ShowApiErrorResultSnackbar(_mainRoot);
                }
                finally
                {
                    RunOnUiThread(() => _progressBar.Visibility = ViewStates.Gone);
                }
            }
        }
    }
}