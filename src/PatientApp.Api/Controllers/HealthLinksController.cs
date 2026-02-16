using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PatientApp.Application.DTOs;
using PatientApp.Application.Interfaces;

namespace PatientApp.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HealthLinksController : ControllerBase
{
    private readonly IHealthLinkService _healthLinkService;

    public HealthLinksController(IHealthLinkService healthLinkService)
    {
        _healthLinkService = healthLinkService;
    }

    [HttpPost]
    [Consumes("application/json", "application/fhir+json")]
    public async Task<ActionResult<SmartHealthLinkDto>> ProcessBundle()
    {
        using var reader = new StreamReader(Request.Body);
        var bundleJson = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(bundleJson))
            return BadRequest("Request body must contain a FHIR Bundle JSON.");

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var result = await _healthLinkService.ProcessBundleAsync(bundleJson, baseUrl);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Retrieve(
        string id,
        [FromQuery] string? recipient)
    {
        if (string.IsNullOrWhiteSpace(recipient))
            return BadRequest("The 'recipient' query parameter is required.");

        var result = await _healthLinkService.RetrieveAsync(id, recipient);

        if (!result.Found)
            return NotFound();

        if (result.Expired)
            return StatusCode(410);

        return Content(result.JweCompactSerialization!, "application/jose");
    }
}
