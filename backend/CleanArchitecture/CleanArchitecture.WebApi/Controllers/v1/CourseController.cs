using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Application.DTOs.Courses;
using CleanArchitecture.Application.Features.Courses.Commands.CreateCourse;
using CleanArchitecture.Application.Features.Courses.Commands.DeleteCourse;
using CleanArchitecture.Application.Features.Courses.Commands.UpdateCourse;
using CleanArchitecture.Application.Features.Courses.Queries.GetAllCourses;
using CleanArchitecture.Application.Features.Courses.Queries.GetCourse;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize]
    public class CourseController : BaseApiController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CourseController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        private string UserId => _httpContextAccessor.HttpContext?.User?.Claims
            .FirstOrDefault(c => c.Type == "uid")?.Value;

        [HttpPost("create")]
        [Authorize(Roles = $"{nameof(Roles.ItStaff)}, {nameof(Roles.Teacher)}")]
        public async Task<ActionResult<ApiResponse<int>>> CreateCourse([FromBody] CreateCourseCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(ApiResponse<int>.Ok(result, "Course created successfully"));
        }

        [HttpPut("update")]
        [Authorize(Roles = $"{nameof(Roles.ItStaff)}, {nameof(Roles.Teacher)}")]
        public async Task<ActionResult<ApiResponse<int>>> UpdateCourse([FromBody] UpdateCourseCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(ApiResponse<int>.Ok(result, "Course updated successfully"));
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = $"{nameof(Roles.ItStaff)}, {nameof(Roles.Teacher)}")]
        public async Task<ActionResult<ApiResponse<int>>> DeleteCourse(int id)
        {
            var result = await Mediator.Send(new DeleteCourseCommand { Id = id });
            return Ok(ApiResponse<int>.Ok(result, "Course deleted successfully"));
        }

        [HttpGet("get-all")]
        public async Task<ActionResult<ApiResponse<PagedResponse<CourseListingDTO>>>> GetAllCourses([FromQuery] GetAllCoursesQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(ApiResponse<PagedResponse<CourseListingDTO>>.Ok(result));
        }

        [HttpGet("get-by-id/{id}")]
        public async Task<ActionResult<ApiResponse<CourseDetailDTO>>> GetCourseById(int id)
        {
            var result = await Mediator.Send(new GetCourseQuery
            {
                Predicate = c => c.Id == id,
            });
            return Ok(ApiResponse<CourseDetailDTO>.Ok(result));
        }

        [HttpGet("get-by-user/{userId}")]
        public async Task<ActionResult<ApiResponse<CourseDetailDTO>>> GetCourseByUserId(string userId)
        {
            var result = await Mediator.Send(new GetCourseQuery
            {
                Predicate = c => c.CourseStudents.Any(cs => cs.Student.Id == userId),
            });
            return Ok(ApiResponse<CourseDetailDTO>.Ok(result));
        }

        [HttpGet("get-for-current-user")]
        public async Task<ActionResult<ApiResponse<PagedResponse<CourseListingDTO>>>> GetCourseForCurrentUser([FromQuery] GetAllCoursesQuery query)
        {
            var result = await Mediator.Send(new GetAllCoursesQuery
            {
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                Predicate = c => c.CourseStudents.Any(cs => cs.Student.Id == UserId),
            });
            return Ok(ApiResponse<PagedResponse<CourseListingDTO>>.Ok(result));
        }

    }
}