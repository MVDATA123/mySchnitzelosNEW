using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.Results;
using System.Web.Mvc;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GCloud.App_Start;
using GCloud.Controllers.ViewModels.Home;
using GCloud.Extensions;
using GCloud.Helper;
using GCloud.Models.Domain;
using GCloud.Service;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using GCloud.Shared.Dto.Api;
using GCloud.Shared.Dto.Domain;
using GCloud.Shared.Exceptions.Home;
using GCloud.Shared.Exceptions.User;
using Microsoft.Ajax.Utilities;
using WebGrease.Css.Extensions;
using ModelStateDictionary = System.Web.Http.ModelBinding.ModelStateDictionary;

namespace GCloud.Controllers.api
{
    [System.Web.Http.RoutePrefix("api/HomeApi")]
    public class HomeApiController : ApiController
    {
        private readonly IMobilePhoneService _mobilePhoneService;
        private readonly IUserService _userService;
        private readonly IStoreService _storesService;
        private readonly ICouponService _couponService;
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        public HomeApiController(IMobilePhoneService mobilePhoneService, IUserService userService, ICouponService couponService, IStoreService storeService)
        {
            _mobilePhoneService = mobilePhoneService;
            _userService = userService;
            _storesService = storeService;
            _couponService = couponService;
        }

        private IAuthenticationManager Authentication => Request.GetOwinContext().Authentication;

        [System.Web.Http.HttpPost]
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.Route("Login")]
        public async Task<LoginResponseModel> Login(LoginBindingModel model)
        {
            var user = _userService.FindbyUsername(model.Username);

            if (user == null)
            {
                throw new CredentialsWrongException();
            }

            if (!user.IsActive)
            {
                throw new UserDisabledException(user.Id);
            }
            if ((user.CreatedById == null || !UserManager.IsInRole(user.CreatedById, "Administrators")) && !user.EmailConfirmed)
            {
                throw new EmailNotConfirmedException(user.Id);
            }
            if (UserManager.CheckPassword(user, model.Password))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                var roles = await UserManager.GetRolesAsync(user.Id);

                claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x.ToString())));

                var id = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

                Authentication.SignIn(id);
            }
            else
            {
                throw new CredentialsWrongException();
            }

            var userRoles = UserManager.GetRolesAsync(user.Id);

            MobilePhone usersMobilePhone = null;

            if (model.DeviceId.HasValue)
            {
                usersMobilePhone = _mobilePhoneService.FindById(model.DeviceId.Value) ?? _mobilePhoneService.CreateNewDevice(user.Id, model.FirebaseInstanceId);
            }
            else
            {
                usersMobilePhone = _mobilePhoneService.CreateNewDevice(user.Id, model.FirebaseInstanceId);
            }

            return new LoginResponseModel
            {
                UserId = user.Id,
                Username = user.UserName,
                Role = (await userRoles).FirstOrDefault(),
                MobilePhoneGuid = usersMobilePhone.Id,
                Email = user.Email,
                InvitationCode = user.InvitationCode,
                TotalPoints = user.TotalPoints
            };
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                BadRequest(ModelState);
            }

            if (_userService.FindbyUsername(model.Username) != null)
            {
                throw new UsernameAlreadyTakenException(model.Username);
            }

            var user = new User()
            {
                UserName = model.Username,
                Email = model.Email,
                IsActive = true,
                Birthday = model.Birthday,
                FirstName = model.FirstName,
                LastName = model.LastName,
                InvitationCode = model.InvitationCode,
                InvitationCodeSender = model.InvitationCodeSender
            };

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                throw new RegistrationException();
            }

            //result = await UserManager.AddToRoleAsync(user.Id, "Customers");

            await UsersController.SendMail(UserManager, UsersController.GetBaseUrl(HttpContext.Current), user);

            //if (!result.Succeeded)
            //{
            //    throw new RegistrationException();
            //}

            return Ok();
        }


        [System.Web.Http.HttpGet]
        public void LogOff(Guid? deviceId)
        {
            var userId = User.Identity.GetUserId();
            Authentication.SignOut();
            if (deviceId != null)
            {
                _mobilePhoneService.RemoveDevice(userId, deviceId.Value);
            }
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.AllowAnonymous]
        public async Task<bool> ResendActivationEmail(string username)
        {
            var user = _userService.FindbyUsername(username);
            if (!user.EmailConfirmed)
            {
                await UsersController.SendMail(UserManager, UsersController.GetBaseUrl(HttpContext.Current), user);
                return true;
            }

            return false;
        }
        [System.Web.Http.Route("Available/{username}")]
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        public bool IsUserNameAvailable(string username)
        {
            if (username.IsNullOrWhiteSpace())
            {
                return false;
            }
            var user = UserManager.FindByName(username.Trim());
            return user == null;
        }

        [System.Web.Http.Route("IsEmailAvailable")]
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        public bool IsEmailAvailable(string email)
        {
            if (email.IsNullOrWhiteSpace())
            {
                return false;
            }
            var user = UserManager.FindByEmail(email.Trim());
            return user == null;
        }

        protected override InvalidModelStateResult BadRequest(ModelStateDictionary modelState)
        {
            var responseMessage = string.Join("\n",
                modelState.Values.SelectMany(m => m.Errors).Select(e => e.ErrorMessage).ToList());
            throw new HttpResponseException(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(responseMessage, Encoding.UTF8)
            });
        }

        [System.Web.Http.Route("IsInvitationCodeAvailable")]
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        public bool IsInvitationCodeAvailable(string invitationCode)
        {
            if (invitationCode.IsNullOrWhiteSpace())
            {
                return false;
            }

            var user = _userService.FindBy(x => x.InvitationCode == invitationCode).FirstOrDefault();
            if (user == null)
                return true;
            else
                return false;
        }

        [System.Web.Http.Route("GetTotalPointsByUserID")]
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        public string GetTotalPointsByUserID(string userId)
        {
            if (userId.IsNullOrWhiteSpace())
            {
                return "Invalid result";
            }

            var user = _userService.FindBy(x => x.Id == userId).FirstOrDefault();
            return user.TotalPoints;
        }

        [System.Web.Http.Route("InvitationCodeSenderId")]
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        public string InvitationCodeSenderId(string invitationCode)
        {
            if (invitationCode.IsNullOrWhiteSpace())
            {
                return null;
            }

            var user = _userService.FindBy(x => x.InvitationCode == invitationCode).FirstOrDefault();
            return user.Id;
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Authorize]
        public HttpResponseMessage ChangePassword(ChangePasswordBindingModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = _userService.FindById(User.Identity.GetUserId());

                var isOldPasswordValid = UserManager.CheckPassword(currentUser, model.OldPassword);

                if (isOldPasswordValid)
                {
                    var token = UserManager.GeneratePasswordResetToken(currentUser.Id);
                    UserManager.ResetPassword(currentUser.Id, token, model.NewPassword);
                    return Request.CreateResponse<object>(HttpStatusCode.OK, null);
                }
                throw new OldPasswordInvalidException();
            }

            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("LoadInitialData")]
        public LoadInitialDataResponseModel LoadInitialData(bool includeImage = false, bool includeBanner = false, bool includeCompanyLogo = false)
        {
            var stores = _storesService.FindAll().Include(s => s.Coupons).ToList().Select(x => Mapper.Map<StoreDto>(x, opts =>
            {
                opts.Items.Add(AutomapperConfig.UserId, User.Identity.GetUserId());
                opts.Items.Add(AutomapperConfig.IncludeBanner, includeBanner);
                opts.Items.Add(AutomapperConfig.IncludeCompanyLogo, includeCompanyLogo);
            })).ToList();
            var coupons = _storesService.FindAll().SelectMany(s => s.Coupons).ToList().DistinctBy(c => c.Id).Select(s => Mapper.Map<CouponDto>(s, opts => opts.Items.Add(AutomapperConfig.IncludeImage, includeImage))).ToList();
            //var coupons = stores.SelectMany(s => s.Coupons).ToList();

            var responseModel = new LoadInitialDataResponseModel
            {
                Stores = stores,
                Coupons = coupons
            };

            return responseModel;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetBackGroundImages")]
        public IList<ImageViewModel> GetBackGroundImages(string alreadyDownloaded)
        {
            var result = new List<ImageViewModel>();
            DirectoryInfo d = new DirectoryInfo(HostingEnvironment.MapPath("~/UploadedFiles/DashboardBackgrounds/"));//Assuming Test is your Folder
            FileInfo[] files = d.GetFiles(); //Getting Text files

            if (alreadyDownloaded != null)
            {
                result = alreadyDownloaded.Split(',').Select(x => new ImageViewModel()
                {
                    Name = x,
                    StateEnum = ImageViewModelState.Deleted
                }).ToList();
            }
            foreach (FileInfo file in files)
            {
                var item = result.FirstOrDefault(x => x.Name == file.Name);
                if (item != null)
                {
                    item.StateEnum = ImageViewModelState.UpToDate;
                    continue;
                }
                using (Image image = Image.FromFile(file.FullName))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        byte[] imageBytes = m.ToArray();

                        result.Add(new ImageViewModel
                        {
                            Name = file.Name,
                            Image = imageBytes,
                            StateEnum = ImageViewModelState.New
                        });
                    }
                }
            }
            return result;
        }
    }
}