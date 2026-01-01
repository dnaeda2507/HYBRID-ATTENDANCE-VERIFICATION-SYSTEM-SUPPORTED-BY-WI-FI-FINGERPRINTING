using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.DTOs.Users;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQuery : IRequest<UserDetailDTO>
    {
        public string Id { get; set; }
    }

    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDetailDTO>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public GetUserByIdQueryHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<UserDetailDTO> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (user == null)
            {
                throw new Exception(nameof(ApplicationUser) + " not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userDTO = _mapper.Map<UserDetailDTO>(user);
            userDTO.Roles = new List<Roles>();
            foreach (var role in roles)
            {
                if (Enum.TryParse(typeof(Roles), role, out var parsedRole))
                {
                    userDTO.Roles.Add((Roles)parsedRole);
                }
            }
            return userDTO;
        }
    }
}