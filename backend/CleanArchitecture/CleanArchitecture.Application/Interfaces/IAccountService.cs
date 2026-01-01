using System.Threading.Tasks;
using CleanArchitecture.Application.DTOs.Account;
using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.DTOs.Email;

namespace CleanArchitecture.Core.Interfaces
{
    public interface IAccountService
    {
        Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request, string ipAddress);
        Task<string> RegisterAsync(RegisterRequest request, string origin);
        Task<string> ConfirmEmailAsync(string userId, string code);
        Task<EmailRequest> ForgotPassword(ForgotPasswordRequest model, string origin);
        Task<string> ResetPassword(ResetPasswordRequest model);
        Task<string> ChangePassword(ChangePasswordRequest model);
    }
}
