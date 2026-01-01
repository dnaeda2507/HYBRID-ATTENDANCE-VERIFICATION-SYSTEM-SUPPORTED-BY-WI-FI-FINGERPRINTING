using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Application.Features.Users.Commands.DeleteUser
{
    public partial class DeleteUserCommand : IRequest<string>
    {
        public string Id { get; set; }
    }

    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, string>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteUserCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null)
            {
                throw new ApiException($"User with ID {request.Id} not found.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new ApiException($"Failed to delete user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return $"User with ID {request.Id} deleted successfully.";
        }
    }
}