using GradGateway.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GradGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlatformStatsController : ControllerBase
{
    private readonly IPlatformStatsService _service;

    public PlatformStatsController(IPlatformStatsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetPlatformStats()
    {
        try
        {
            var stats = await _service.GetPlatformStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to retrieve platform statistics", error = ex.Message });
        }
    }
}
