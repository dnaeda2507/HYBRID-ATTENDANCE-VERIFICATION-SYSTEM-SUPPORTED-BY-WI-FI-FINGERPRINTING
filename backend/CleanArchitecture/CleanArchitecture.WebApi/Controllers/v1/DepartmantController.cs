using System.Collections.Generic;
using System.Threading.Tasks;
using CleanArchitecture.Application.DTOs.Departmant;
using CleanArchitecture.Application.Features.Departmants.Queries;
using CleanArchitecture.Core.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize]
    public class DepartmantController : BaseApiController
    {
        [HttpGet("get-all")]
        public async Task<ActionResult<ApiResponse<List<DepartmantLookupDTO>>>> GetAllDepartmantsLookUp()
        {
            var result = await Mediator.Send(new GetAllDepartmantsQuery());
            return Ok(ApiResponse<List<DepartmantLookupDTO>>.Ok(result));
        }
    }
}