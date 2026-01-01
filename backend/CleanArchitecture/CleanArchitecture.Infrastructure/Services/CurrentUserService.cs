using System.Security.Claims;
using CleanArchitecture.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
            => _httpContextAccessor = httpContextAccessor;

        public string? UserId
            => User?.FindFirstValue("uid");

        public bool IsAuthenticated
            => User?.Identity?.IsAuthenticated == true;

        public bool IsInRole(string role)
            => User?.IsInRole(role) == true;
    }
}