using System.Threading.Tasks;
using CleanArchitecture.Application.DTOs.Wifi;
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

        public WifiController(
            IStudentWifiScanService scanService,
            IWifiTrainingSampleService sampleService)
        {
            _scanService = scanService;
            _sampleService = sampleService;
        }

        [HttpPost("scans")]
        [Authorize]
        public async Task<IActionResult> CreateScan([FromBody] StudentWifiScanCreateDto dto)
        {
            var result = await _scanService.CreateAsync(dto);
            return Ok(result);
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
            var id = await _sampleService.CreateAsync(dto);
            return Created($"/api/wifi/training-samples/{id}", null);
        }
    }
}
