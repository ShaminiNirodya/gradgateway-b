using GradGateway.Business.DTOs;
using GradGateway.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GradGateway.Api.Controllers;

[ApiController]
[Route("api/supportinquiries")]
public class SupportInquiriesController : ControllerBase
{
    private readonly ISupportInquiryService _service;

    public SupportInquiriesController(ISupportInquiryService service)
    {
        _service = service;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Submit([FromBody] SubmitSupportInquiryDto dto)
    {
        try
        {
            var row = await _service.SubmitAsync(dto);
            return Ok(new { message = "Your message has been received. We will get back to you soon.", inquiry = row });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
