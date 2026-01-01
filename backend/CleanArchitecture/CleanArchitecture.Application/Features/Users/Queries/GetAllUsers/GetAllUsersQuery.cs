using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.DTOs.Users;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.Users.Queries
{
    public class GetAllUsersQuery : IRequest<PagedResponse<UserListingDTO>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResponse<UserListingDTO>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public GetAllUsersQueryHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<PagedResponse<UserListingDTO>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = _userManager.Users.AsNoTracking().AsQueryable();
            var totalUsers = await users.CountAsync(cancellationToken);

            var pagedUsers = await users
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(user => _mapper.Map<UserListingDTO>(user))
                .ToListAsync();

            return new PagedResponse<UserListingDTO>(pagedUsers, request.PageNumber, request.PageSize, totalUsers);
        }
    }
}