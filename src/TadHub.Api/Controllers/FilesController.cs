using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Storage;
using TadHub.SharedKernel.Api;

namespace TadHub.Api.Controllers;

/// <summary>
/// Generic tenant-scoped file upload endpoint.
/// Files are uploaded as pending and later attached to entities on form submission.
/// </summary>
[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/files")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class FilesController : ControllerBase
{
    private readonly ITenantFileService _tenantFileService;

    public FilesController(ITenantFileService tenantFileService)
    {
        _tenantFileService = tenantFileService;
    }

    /// <summary>
    /// Uploads a file and creates a pending TenantFile record.
    /// The file can later be attached to an entity (e.g., Candidate) on form submission.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="fileType">Type of file: photo, video, passport.</param>
    /// <param name="file">The file to upload.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost]
    [ProducesResponseType(typeof(TenantFileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(50 * 1024 * 1024 + 1024)]
    public async Task<IActionResult> Upload(
        Guid tenantId,
        [FromQuery] string fileType,
        IFormFile file,
        CancellationToken ct)
    {
        var path = HttpContext.Request.Path.Value;

        if (string.IsNullOrWhiteSpace(fileType))
            return Problem(ApiError.BadRequest("fileType query parameter is required (photo, video, passport)", path));

        var rules = TenantFileService.GetValidationRules(fileType);
        if (rules is null)
            return Problem(ApiError.BadRequest($"Unknown file type: {fileType}. Supported: photo, video, passport", path));

        if (file.Length == 0)
            return Problem(ApiError.BadRequest("File is empty", path));

        if (file.Length > rules.Value.MaxSize)
            return Problem(ApiError.BadRequest($"File exceeds maximum size of {rules.Value.MaxSize / (1024 * 1024)}MB", path));

        if (!rules.Value.AllowedTypes.Contains(file.ContentType.ToLower()))
            return Problem(ApiError.BadRequest($"Content type {file.ContentType} not allowed for {fileType}. Allowed: {string.Join(", ", rules.Value.AllowedTypes)}", path));

        await using var stream = file.OpenReadStream();
        var result = await _tenantFileService.UploadAsync(
            tenantId, file.FileName, stream, file.ContentType, file.Length, fileType, ct);

        return Ok(result);
    }

    private IActionResult Problem(ApiError error)
    {
        return new ObjectResult(error) { StatusCode = error.Status, ContentTypes = { "application/problem+json" } };
    }
}
