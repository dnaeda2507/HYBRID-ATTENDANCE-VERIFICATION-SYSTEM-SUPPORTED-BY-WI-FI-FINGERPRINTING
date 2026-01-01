using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Application.Features.Users.Commands.UpdateUser
{
    public partial class UpdateProfileCommand : IRequest<string>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string InformationMail { get; set; }
    }

    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, string>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public UpdateProfileCommandHandler(
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<string> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new ApiException($"User with email {request.Email} not found.");

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains(nameof(Roles.Student)))
            {
                user.PhoneNumber = request.PhoneNumber;
                user.InformationMail = request.InformationMail;
            }
            else
            {
                _mapper.Map(request, user);
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new ApiException(result.Errors.First().Description);

            return $"User {request.Email} updated successfully.";
        }
    }

}