using System.Threading.Tasks;
using CleanArchitecture.Application.DTOs.Account;
using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        [HttpPost("authenticate/web")]
        public async Task<ActionResult<ApiResponse<AuthenticationResponse>>> AuthenticateWebAsync(AuthenticationRequest request)
        {
            var result = await _accountService.AuthenticateAsync(request, GenerateIPAddress());
            if (result.Roles.Contains(Roles.Student.ToString()))
            {
                return Ok(ApiResponse<AuthenticationResponse>.Fail(
                    "Student cannot login to web application. Please use mobile application."));
            }
            return Ok(ApiResponse<AuthenticationResponse>.Ok(result));
        }
        [HttpPost("authenticate/mobile")]
        public async Task<ActionResult<AuthenticationResponse>> AuthenticateMobileAsync(AuthenticationRequest request)
        {
            var result = await _accountService.AuthenticateAsync(request, GenerateIPAddress());
            return Ok(ApiResponse<AuthenticationResponse>.Ok(result));
        }
        // [HttpPost("register")]
        // public async Task<IActionResult> RegisterAsync(RegisterRequest request)
        // {
        //     var origin = Request.Headers["origin"];
        //     return Ok(await _accountService.RegisterAsync(request, origin));
        // }
        // [HttpGet("confirm-email")]
        // public async Task<ActionResult<string>> ConfirmEmailAsync([FromQuery] string userId, [FromQuery] string code)
        // {
        //     var origin = Request.Headers["origin"].FirstOrDefault() ?? "https://localhost:9001";
        //     var result = await _accountService.ConfirmEmailAsync(userId, code);
        //     return Ok(result);
        // }
        // [HttpPost("forgot-password")]
        // public async Task<ActionResult<EmailRequest>> ForgotPassword(ForgotPasswordRequest model)
        // {
        //     var origin = Request.Headers["origin"].FirstOrDefault() ?? "https://localhost:9001";
        //     var result = await _accountService.ForgotPassword(model, origin);
        //     return Ok(result);
        // }
        // [HttpPost("reset-password")]
        // public async Task<ActionResult<string>> ResetPassword(ResetPasswordRequest model)
        // {
        //     var result = await _accountService.ResetPassword(model);
        //     return Ok(result);
        // }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> ChangePassword(ChangePasswordRequest model)
        {
            var result = await _accountService.ChangePassword(model);
            return Ok(ApiResponse.Ok(result));
        }

        private string GenerateIPAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}