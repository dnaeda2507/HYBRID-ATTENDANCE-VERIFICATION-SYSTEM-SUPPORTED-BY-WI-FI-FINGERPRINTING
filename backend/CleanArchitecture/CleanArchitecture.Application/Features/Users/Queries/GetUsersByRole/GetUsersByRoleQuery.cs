using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.DTOs.Users;
using CleanArchitecture.Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.Users.Queries.GetUsersByRole
{
    public class GetUsersByRoleQuery : IRequest<List<UserListingDTO>>
    {
        public string Role { get; set; }
    }

    public class GetUsersByRoleQueryHandler : IRequestHandler<GetUsersByRoleQuery, List<UserListingDTO>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public GetUsersByRoleQueryHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<List<UserListingDTO>> Handle(GetUsersByRoleQuery request, CancellationToken cancellationToken)
        {
            var allUsers = await _userManager.Users.ToListAsync(cancellationToken);
            var usersInRole = new List<ApplicationUser>();
            foreach (var user in allUsers)
            {
                if (await _userManager.IsInRoleAsync(user, request.Role))
                {
                    usersInRole.Add(user);
                }
            }

            return usersInRole.Select(_mapper.Map<UserListingDTO>).ToList();
        }
    }

}