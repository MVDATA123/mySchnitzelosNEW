using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using GCloudShared.Service.Dto;
using Refit;

namespace GCloudShared.Service
{
    public interface IAuthService
    {
        [Post("/api/HomeApi/Login")]
        IObservable<LoginResponseModel> Login([Body()] LoginRequestModel model);

        [Get("/api/HomeApi/Logout")]
        Task<HttpResponseMessage> Logout(String deviceId);

        [Get("/api/UsersApi/GetQrCode")]
        Task<Stream> LoadQrCode();

        [Post("/api/HomeApi/Register")]
        Task RegisterUser(RegisterRequestModel model);

        [Get("/api/HomeApi/Available/{username}")]
        Task<bool> IsUsernameAvailable([AliasAs("username")] string username);

        [Get("/api/HomeApi/IsEmailAvailable")]
        Task<bool> IsEmailAvailable([AliasAs("email")] string email);

        [Get("/api/HomeApi/IsInvitationCodeAvailable")]
        Task<bool> IsInvitationCodeAvailable([AliasAs("invitationCode")] string invitationCode);

        [Get("/api/HomeApi/InvitationCodeSenderId")]
        Task<string> InvitationCodeSenderId([AliasAs("invitationCode")] string invitationCode);

        [Get("/api/HomeApi/GetTotalPointsByUserID")]
        Task<string> GetTotalPointsByUserID([AliasAs("userId")] string userId);

        [Get("/api/HomeApi/ResendActivationEmail")]
        Task<bool> ResendActivationEmail(string username);

        [Post("/api/HomeApi/ChangePassword")]
        Task ChangePassword(ChangePasswordRequestModel model);

        [Post("/api/HomeApi/ResetPassword")]
        Task<bool> ForgotPassword(string usernameOrEmail);
    }
}
