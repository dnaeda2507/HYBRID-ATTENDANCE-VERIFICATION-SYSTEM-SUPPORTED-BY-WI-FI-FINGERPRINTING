using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Application.DTOs.Attendances;
using CleanArchitecture.Application.Features.Attendances.Commands;
using CleanArchitecture.Application.Features.Attendances.Queries;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Filters;
using CleanArchitecture.Core.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/sessions")]
    public class SessionsController : BaseApiController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionsController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        private string UserId => _httpContextAccessor.HttpContext?.User?.Claims
            .FirstOrDefault(c => c.Type == "uid")?.Value;

        [HttpPost("create")]
        [Authorize(Roles = $"{nameof(Roles.ItStaff)}, {nameof(Roles.Teacher)}, {nameof(Roles.AcademicStaff)}")]
        public async Task<ActionResult<ApiResponse<CreateSessionResult>>> Create(
            [FromBody] CreateSessionCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(ApiResponse<CreateSessionResult>.Ok(result, "Session created successfully"));
        }

        [HttpPost("end")]
        [Authorize(Roles = $"{nameof(Roles.ItStaff)}, {nameof(Roles.Teacher)}, {nameof(Roles.AcademicStaff)}")]
        public async Task<ActionResult<ApiResponse<bool>>> End(int sessionId)
        {
            await Mediator.Send(new EndSessionCommand { SessionId = sessionId });
            return Ok(ApiResponse<bool>.Ok(true, "Session ended successfully"));
        }

        [HttpPost("attend")]
        [Authorize(Roles = nameof(Roles.Student))]
        public async Task<ActionResult<ApiResponse<bool>>> Attend([FromBody] MarkAttendanceCommand command)
        {
            await Mediator.Send(command);
            return Ok(ApiResponse<bool>.Ok(true, "Attendance marked successfully"));
        }

        [HttpGet("get-currentuser-attendances")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResponse<AttendanceListingDTO>>>> GetCurrentUserAttendances([FromQuery] RequestParameter requestParam)
        {
            var result = await Mediator.Send(new GetAllAttendancesQuery
            {
                PageNumber = requestParam.PageNumber,
                PageSize = requestParam.PageSize,
                Predicate = x => x.StudentId == UserId
            });
            return Ok(ApiResponse<PagedResponse<AttendanceListingDTO>>.Ok(result));
        }

        [HttpGet("attendance-report")]
        [Authorize(Roles = $"{nameof(Roles.ItStaff)}, {nameof(Roles.Teacher)}")]
        public async Task<ActionResult<ApiResponse<List<AttendedUserDTO>>>> GetAttendanceReport(int sessionId)
        {
            var result = await Mediator.Send(new GetAttendedStudentsQuery
            {
                Predicate = x => x.SessionId == sessionId
            });
            return Ok(ApiResponse<List<AttendedUserDTO>>.Ok(result));
        }
    }
}