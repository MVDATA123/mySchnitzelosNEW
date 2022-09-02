using AutoMapper.QueryableExtensions;
using GCloud.Models.Domain;
using GCloud.Service;
using GCloud.Shared.Dto.Api;
using GCloud.Shared.Dto.Domain;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using GCloud.Shared.Dto;
using GCloud.Shared.Exceptions.Store;


namespace GCloud.Controllers.api
{

    [RoutePrefix("api/BillApi")]
    public class BillApiController : ApiController
    {
        private readonly IBillService _billService;
        private readonly IMobilePhoneService _mobilePhoneService;
        private readonly IFirebaseNotificationService _firebaseNotificationService;
        private readonly IStoreService _storeService;

        public BillApiController(IBillService billService,
            IMobilePhoneService mobilePhoneService,
            IFirebaseNotificationService firebaseNotificationService,
            IStoreService storeService)
        {
            _billService = billService;
            _mobilePhoneService = mobilePhoneService;
            _firebaseNotificationService = firebaseNotificationService;
            _storeService = storeService;
        }

        // to call from end user to get last bills
        [Authorize]
        [HttpGet, HttpPost]
        [Route("Get")]
        public GetBillsResponseModel Get(List<Guid> alreadyGot)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                if (alreadyGot == null || alreadyGot.Count < 1)
                    return new GetBillsResponseModel(Mapper.Map<List<Bill_Out_Dto>>(_billService.FindBy(e => e.UserId == userId).ToList()));
                else
                    return new GetBillsResponseModel(Mapper.Map<List<Bill_Out_Dto>>(_billService.FindBy(e => alreadyGot.Contains(e.GetId())).ToList()));
            }
            catch(Exception ex)
            {
                throw new HttpResponseException(
                    new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        ReasonPhrase = ex.Message
                    });
            }
        }

        // to call from end user to get last bills
        [Authorize]
        [HttpGet]
        [Route("GetById")]
        public Bill_Out_Dto GetById(Guid id)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var f = _billService.FindBy(e => e.UserId == userId && e.Id == id).ToList().FirstOrDefault();
                return Mapper.Map<Bill_Out_Dto>(f);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(
                    new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        ReasonPhrase = ex.Message
                    });
            }
        }

        // to call from kassa
        [HttpPost]
        [Route("Add")]
        public async Task<HttpResponseMessage> Add(BillAddRequestModel model)
        {
            try
            {
                var store = _storeService.FindByApiToken(model.StoreApiToken);

                if (store == null)
                {
                    throw new ApiTokenInvalidException(model.StoreApiToken);
                }

                var response = new HttpResponseMessage(HttpStatusCode.OK);

                foreach (var invoice in model.Invoices)
                {
                    invoice.Biller = new InvoiceBiller
                    {
                        ComanyName = store.Company.Name,
                        VATIdentificationNumber = store.Company.TaxNumber,
                        Address = new InvoiceBillerAddress
                        {
                            Street = $"{store.Street} {store.HouseNr}",
                            ZIP = (ushort) (ushort.TryParse(store.Plz, out var plsValue) ? plsValue : 0),
                            Town = store.City,
                            Name = store.Name,
                            Country = new InvoiceBillerAddressCountry
                            {
                                Value = store.Country.Name
                            }
                        }
                    };
                }
                if (ValidateBill(model) == false)
                    throw new HttpResponseException(
                        new HttpResponseMessage(HttpStatusCode.NotAcceptable)
                        {
                            ReasonPhrase = "Bill not valid"
                        });

                foreach (var invoice in model.Invoices)
                {
                    var bill = new Bill
                    {
                        Amount = invoice.TotalGrossAmount,
                        Company = store.Company.Name,
                        ImportedAt = DateTime.Now,
                        Invoice = invoice,
                        InvoiceNumber = invoice.InvoiceNumber,
                        InvoiceDate = invoice.InvoiceDate,
                        UserId = model.UserId
                    };
                    _billService.Add(bill);

                    var phones = _mobilePhoneService.FindBy(p => p.UserId == model.UserId).ToList();
                    foreach (var p in phones)
                    {
                        var companyName = bill.Company.Length > 10 ? $"{bill.Company.Substring(0, 10)}..." : bill.Company;
                        var n = new FirebaseNotification
                        {
                            Body = $"Rechnung für {companyName} um {bill.Amount.ToString($"0.00 {invoice.InvoiceCurrency}")} vom {bill.Invoice.InvoiceDate:dd.MM.yyyy}",
                            CreatedOn = DateTime.UtcNow,
                            LastAttemptOn = DateTime.Now,
                            DeviceId = p.Id,
                            Title = "Neue Rechnung!",
                            Type = "bill",
                            BillId = bill.Id
                        };
                        _firebaseNotificationService.Add(n);

                        // todo send notification to phone
                        var res = await _firebaseNotificationService.Send(n);
                    }
                }

                return response;
            }
            catch (HttpResponseException he)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(
                    new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        ReasonPhrase = ex.Message
                    });
            }
        }

        private bool ValidateBill(BillAddRequestModel model)
        {
            if (model?.Invoices == null || model.Invoices.Any(i => string.IsNullOrEmpty(i.InvoiceNumber)))
                return false;

            var invoiceNumbers = model.Invoices.Select(i => i.InvoiceNumber).ToList();
            return !_billService.Any(local => invoiceNumbers.Contains(local.InvoiceNumber));
        }
    }
}