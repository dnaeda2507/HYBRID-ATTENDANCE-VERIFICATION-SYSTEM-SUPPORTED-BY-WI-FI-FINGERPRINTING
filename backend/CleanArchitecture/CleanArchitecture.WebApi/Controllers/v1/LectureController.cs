using System.Threading.Tasks;
using CleanArchitecture.Application.DTOs.Lectures;
using CleanArchitecture.Application.Features.Lectures.Commands.CreateLecture;
using CleanArchitecture.Application.Features.Lectures.Commands.DeleteLecture;
using CleanArchitecture.Application.Features.Lectures.Commands.UpdateLecture;
using CleanArchitecture.Application.Features.Lectures.Queries.GetAllLectures;
using CleanArchitecture.Application.Features.Lectures.Queries.GetLectureById;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize]
    public class LectureController : BaseApiController
    {
        [HttpPost("create")]
        [Authorize(Roles = $"{nameof(Roles.ItStaff)}, {nameof(Roles.Teacher)}")]
        public async Task<ActionResult<ApiResponse<int>>> CreateLecture([FromBody] CreateLectureCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(ApiResponse<int>.Ok(result, "Lecture created successfully"));
        }

        [HttpGet("get-all")]
        public async Task<ActionResult<ApiResponse<PagedResponse<LectureListingDTO>>>> GetAllLectures([FromQuery] GetAllLecturesQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(ApiResponse<PagedResponse<LectureListingDTO>>.Ok(result));
        }

        [HttpGet("get-by-id/{id}")]
        [Authorize(Roles = $"{nameof(Roles.ItStaff)}, {nameof(Roles.Teacher)}")]
        public async Task<ActionResult<ApiResponse<LectureDTO>>> GetLectureById(int id)
        {
            var result = await Mediator.Send(new GetLectureByIdQuery { Id = id });
            return Ok(ApiResponse<LectureDTO>.Ok(result));
        }

        [HttpPut("update")]
        [Authorize(Roles = $"{nameof(Roles.ItStaff)}, {nameof(Roles.Teacher)}")]
        public async Task<ActionResult<ApiResponse<int>>> UpdateLecture([FromBody] UpdateLectureCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(ApiResponse<int>.Ok(result, "Lecture updated successfully"));
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = $"{nameof(Roles.ItStaff)}, {nameof(Roles.Teacher)}")]
        public async Task<ActionResult<ApiResponse<int>>> DeleteLecture(int id)
        {
            var result = await Mediator.Send(new DeleteLectureCommand { Id = id });
            return Ok(ApiResponse<int>.Ok(result, "Lecture deleted successfully"));
        }
    }
}