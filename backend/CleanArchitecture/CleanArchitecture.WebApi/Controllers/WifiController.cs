using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.DTOs.Wifi;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WifiController : ControllerBase
    {
        private readonly IStudentWifiScanService _scanService;
        private readonly IWifiTrainingSampleService _sampleService;
        private readonly ICurrentUserService _currentUser;

        public WifiController(IStudentWifiScanService scanService, IWifiTrainingSampleService sampleService, ICurrentUserService currentUser)
        {
            _scanService = scanService;
            _sampleService = sampleService;
            _currentUser = currentUser;
        }

        [HttpPost("scans")]
        [Authorize]
        public async Task<IActionResult> CreateScan([FromBody] StudentWifiScanCreateDto dto)
        {
            if (dto == null) return BadRequest();
            if (dto.AccessPoints == null || dto.AccessPoints.Count == 0) return BadRequest("no access points");
            if (string.IsNullOrWhiteSpace(dto.StudentId)) dto.StudentId = _currentUser.UserId;

            var id = await _scanService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetScan), new { id }, null);
        }

        [HttpGet("scans/{id}")]
        [Authorize]
        public async Task<IActionResult> GetScan(int id)
        {
            var scan = await _scanService.GetByIdAsync(id);
            if (scan == null) return NotFound();
            return Ok(scan);
        }

        [HttpPost("training-samples")]
        [Authorize]
        public async Task<IActionResult> CreateTrainingSample([FromBody] StudentWifiScanCreateDto dto)
        {
            if (dto == null) return BadRequest();
            if (dto.AccessPoints == null || dto.AccessPoints.Count == 0) return BadRequest("no access points");
            if (string.IsNullOrWhiteSpace(dto.StudentId)) dto.StudentId = _currentUser.UserId;

            var id = await _sampleService.CreateAsync(dto);
            return Created($"/api/wifi/training-samples/{id}", null);
        }
    }
}