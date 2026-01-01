using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Application.DTOs.Users;
using CleanArchitecture.Application.Features.Users.Commands.CreateUser;
using CleanArchitecture.Application.Features.Users.Commands.DeleteUser;
using CleanArchitecture.Application.Features.Users.Commands.UpdateUser;
using CleanArchitecture.Application.Features.Users.Queries;
using CleanArchitecture.Application.Features.Users.Queries.GetUserById;
using CleanArchitecture.Application.Features.Users.Queries.GetUsersByRole;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize]
    public class UserController : BaseApiController
    {
        public IHttpContextAccessor _httpContextAccessor;

        public UserController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private string UserId => _httpContextAccessor.HttpContext?.User?.Claims
            .FirstOrDefault(c => c.Type == "uid")?.Value;

        [HttpPost("create")]
        [Authorize(Roles = nameof(Roles.ItStaff))]
        public async Task<ActionResult<ApiResponse>> CreateUser([FromBody] CreateUserCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(ApiResponse.Ok(result));
        }

        [HttpPut("update")]
        [Authorize(Roles = nameof(Roles.ItStaff))]
        public async Task<ActionResult<ApiResponse>> UpdateUser([FromBody] UpdateUserCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(ApiResponse.Ok(result));
        }

        [HttpPut("update-profile")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> UpdateProfile([FromBody] UpdateProfileCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(ApiResponse.Ok(result));
        }

        [HttpGet("get-all")]
        [Authorize(Roles = nameof(Roles.ItStaff))]
        public async Task<ActionResult<ApiResponse<PagedResponse<UserListingDTO>>>> GetAllUsers([FromQuery] GetAllUsersQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(ApiResponse<PagedResponse<UserListingDTO>>.Ok(result));
        }

        [HttpGet("get-by-id/{id}")]
        [Authorize(Roles = nameof(Roles.ItStaff))]
        public async Task<ActionResult<ApiResponse<UserDetailDTO>>> GetUserById(string id)
        {
            var result = await Mediator.Send(new GetUserByIdQuery { Id = id });
            return Ok(ApiResponse<UserDetailDTO>.Ok(result));
        }

        [HttpGet("get-current-user")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserDetailDTO>>> GetCurrentUser()
        {
            var result = await Mediator.Send(new GetUserByIdQuery { Id = UserId });
            return Ok(ApiResponse<UserDetailDTO>.Ok(result));
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = nameof(Roles.ItStaff))]
        public async Task<ActionResult<ApiResponse>> DeleteUser(string id)
        {
            var result = await Mediator.Send(new DeleteUserCommand { Id = id });
            return Ok(ApiResponse.Ok(result));
        }

        [HttpGet("get-students")]
        [Authorize(Roles = $"{nameof(Roles.ItStaff)},{nameof(Roles.Teacher)}")]
        public async Task<ActionResult<ApiResponse<List<UserListingDTO>>>> GetStudents()
        {
            var result = await Mediator.Send(new GetUsersByRoleQuery { Role = Roles.Student.ToString() });
            return Ok(ApiResponse<List<UserListingDTO>>.Ok(result));
        }

        [HttpGet("get-teachers")]
        [Authorize(Roles = $"{nameof(Roles.ItStaff)},{nameof(Roles.Teacher)}")]
        public async Task<ActionResult<ApiResponse<List<UserListingDTO>>>> GetTeachers()
        {
            var result = await Mediator.Send(new GetUsersByRoleQuery { Role = Roles.Teacher.ToString() });
            return Ok(ApiResponse<List<UserListingDTO>>.Ok(result));
        }

        [HttpGet("get-it-staff")]
        [Authorize(Roles = $"{nameof(Roles.ItStaff)},{nameof(Roles.Teacher)}")]
        public async Task<ActionResult<ApiResponse<List<UserListingDTO>>>> GetItStaff()
        {
            var result = await Mediator.Send(new GetUsersByRoleQuery { Role = Roles.ItStaff.ToString() });
            return Ok(ApiResponse<List<UserListingDTO>>.Ok(result));
        }

        [HttpGet("get-academic-staff")]
        [Authorize(Roles = $"{nameof(Roles.ItStaff)},{nameof(Roles.Teacher)}")]
        public async Task<ActionResult<ApiResponse<List<UserListingDTO>>>> GetAcademicStaff()
        {
            var result = await Mediator.Send(new GetUsersByRoleQuery { Role = Roles.AcademicStaff.ToString() });
            return Ok(ApiResponse<List<UserListingDTO>>.Ok(result));
        }
    }
}