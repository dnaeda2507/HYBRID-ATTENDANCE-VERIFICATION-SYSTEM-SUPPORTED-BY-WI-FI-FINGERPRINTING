using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Application.Features.Users.Commands.CreateUser
{
    public partial class CreateUserCommand : IRequest<string>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string InformationMail { get; set; }
        public List<Roles> Roles { get; set; }
        public string Password { get; set; }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, string>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateUserCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    InformationMail = request.InformationMail,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };
                var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
                if (userWithSameEmail == null)
                {
                    var result = await _userManager.CreateAsync(user, request.Password);
                    if (result.Succeeded)
                    {
                        foreach (var role in request.Roles)
                        {
                            await _userManager.AddToRoleAsync(user, role.ToString());
                        }
                        return $"User Registered.";
                    }
                    else
                    {
                        throw new ApiException($"{result.Errors.FirstOrDefault()?.ToString()}");
                    }
                }
                else
                {
                    throw new ApiException($"Email {request.Email} is already registered.");
                }
            }
            catch (ApiException ex)
            {
                throw new ApiException($"An error occurred while creating the user: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while creating the user: {ex.Message}");
            }
        }
    }


}