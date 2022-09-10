using System;
using System.Threading.Tasks;
using GCloudShared.Service.Dto;
using Refit;

namespace GCloudShared.Service.WebShopRegisterServices
{
    public interface IWebShopService
    {
        [Post("/CustomerApi/Register")]
        Task<RegisterToWebShopModel> Register([Body()] RegisterToWebShopModel model);

        [Post("/CustomerApi/CheckIfCustomerAlreadyRegistred")]
        Task<string> CheckIfUserIsAlreadyRegistredInWebShop([Body()] RegisterToWebShopModel model);

        [Post("/Customer/PasswordRecoveryFromGCloud")]
        Task<string> ResetPasswordInWebShopFromGcloud([Body()] RecoveryPasswordToWebShopModel model);

        [Post("/Customer/WelcomeMailFromGCloud")]
        Task<string> SetWelcomeEmailToWebShopFromGcloud([Body()] RecoveryPasswordToWebShopModel model);

    }
}
