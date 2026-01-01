using System;
using System.Collections.Generic;
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
    public partial class UpdateUserCommand : IRequest<string>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string InformationMail { get; set; }
        public List<Roles> Roles { get; set; }
    }

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, string>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public UpdateUserCommandHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<string> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new ApiException($"User with email {request.Email} not found.");
            }

            _mapper.Map(request, user);

            var result = await _userManager.UpdateAsync(user);

            if (request.Roles != null && request.Roles.Any())
            {
                var roles = Enum.GetValues(typeof(Roles)).Cast<Roles>().Select(r => r.ToString()).ToList();
                var userRoles = await _userManager.GetRolesAsync(user);
                var rolesToAdd = request.Roles.Select(r => r.ToString()).Except(userRoles).ToList();
                var rolesToRemove = userRoles.Except(request.Roles.Select(r => r.ToString())).ToList();

                if (rolesToAdd.Any())
                {
                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                }

                if (rolesToRemove.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }
            }

            if (result.Succeeded)
            {
                return $"User {request.Email} updated successfully.";
            }

            throw new ApiException($"{result.Errors.FirstOrDefault()?.Description.ToString()}");
        }
    }



}