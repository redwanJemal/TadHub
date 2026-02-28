using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Financial.Contracts.Settings;
using Tenancy.Contracts;

namespace TadHub.Api.Controllers;

[ApiController]
[Route("api/v1/admin/settings/financial")]
[Authorize(Roles = "platform-admin")]
public class AdminFinancialSettingsController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private static TenantFinancialSettings _platformDefaults = new();

    public AdminFinancialSettingsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    /// <summary>
    /// Gets the platform-wide default financial settings.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(TenantFinancialSettings), StatusCodes.Status200OK)]
    public IActionResult GetDefaults()
    {
        return Ok(_platformDefaults);
    }

    /// <summary>
    /// Updates the platform-wide default financial settings.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult UpdateDefaults([FromBody] TenantFinancialSettings settings)
    {
        _platformDefaults = settings;
        return NoContent();
    }
}
