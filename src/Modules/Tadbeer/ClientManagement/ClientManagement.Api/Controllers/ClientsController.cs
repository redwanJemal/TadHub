using ClientManagement.Contracts;
using ClientManagement.Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TadHub.SharedKernel.Api;

namespace ClientManagement.Api.Controllers;

/// <summary>
/// Client management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/clients")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    /// <summary>
    /// Registers a new client.
    /// </summary>
    /// <remarks>
    /// Auto-detects client category from Emirates ID.
    /// Use categoryOverride to manually set (requires clients.manage permission).
    /// </remarks>
    [HttpPost]
    [Authorize(Policy = "clients.create")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] CreateClientRequest request, CancellationToken ct)
    {
        var result = await _clientService.RegisterAsync(request, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode == "DUPLICATE_EMIRATES_ID"
                ? Conflict(new { error = result.Error, message = result.Error })
                : BadRequest(new { error = result.Error, message = result.Error });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Gets a client by ID.
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <param name="include">Relations to include: documents, discountCards</param>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "clients.view")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] string? include, CancellationToken ct)
    {
        var includes = IncludeResolver.Parse(include);
        var result = await _clientService.GetByIdAsync(id, includes, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error, message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Updates a client.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "clients.update")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClientRequest request, CancellationToken ct)
    {
        var result = await _clientService.UpdateAsync(id, request, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error, message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Verifies a client's documents.
    /// </summary>
    /// <remarks>
    /// Sets IsVerified=true and publishes ClientVerifiedEvent.
    /// Enables contract creation for this client.
    /// </remarks>
    [HttpPost("{id:guid}/verify")]
    [Authorize(Policy = "clients.manage")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Verify(Guid id, CancellationToken ct)
    {
        var result = await _clientService.VerifyAsync(id, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode == "ALREADY_VERIFIED"
                ? BadRequest(new { error = result.Error, message = result.Error })
                : NotFound(new { error = result.Error, message = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Blocks a client.
    /// </summary>
    /// <remarks>
    /// Sets SponsorFileStatus=Blocked and pauses all pending contracts.
    /// </remarks>
    [HttpPost("{id:guid}/block")]
    [Authorize(Policy = "clients.manage")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Block(Guid id, [FromBody] BlockClientRequest request, CancellationToken ct)
    {
        var result = await _clientService.BlockAsync(id, request.Reason, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode == "ALREADY_BLOCKED"
                ? BadRequest(new { error = result.Error, message = result.Error })
                : NotFound(new { error = result.Error, message = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Unblocks a previously blocked client.
    /// </summary>
    [HttpPost("{id:guid}/unblock")]
    [Authorize(Policy = "clients.manage")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unblock(Guid id, CancellationToken ct)
    {
        var result = await _clientService.UnblockAsync(id, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode == "NOT_BLOCKED"
                ? BadRequest(new { error = result.Error, message = result.Error })
                : NotFound(new { error = result.Error, message = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Lists clients with filtering, sorting, and pagination.
    /// </summary>
    /// <remarks>
    /// Filters:
    /// - filter[category]=local,expat (array)
    /// - filter[sponsorFileStatus]=active
    /// - filter[isVerified]=true
    /// - filter[nationality]=UAE
    /// - filter[emirate]=dubai
    /// - filter[createdAt][gte]=2026-01-01
    /// 
    /// Sorting:
    /// - sort=createdAt:desc
    /// - sort=fullNameEn:asc
    /// 
    /// Includes:
    /// - include=documents,discountCards
    /// </remarks>
    [HttpGet]
    [Authorize(Policy = "clients.view")]
    [ProducesResponseType(typeof(PagedList<ClientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] QueryParameters query, CancellationToken ct)
    {
        var result = await _clientService.ListAsync(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Searches clients by name or Emirates ID.
    /// </summary>
    [HttpGet("search")]
    [Authorize(Policy = "clients.view")]
    [ProducesResponseType(typeof(PagedList<ClientRefDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] QueryParameters query, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return BadRequest(new { error = "INVALID_QUERY", message = "Search query must be at least 2 characters" });

        var result = await _clientService.SearchAsync(q, query, ct);
        return Ok(result);
    }

    #region Documents

    /// <summary>
    /// Gets all documents for a client.
    /// </summary>
    [HttpGet("{id:guid}/documents")]
    [Authorize(Policy = "clients.view")]
    [ProducesResponseType(typeof(List<ClientDocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocuments(Guid id, CancellationToken ct)
    {
        var result = await _clientService.GetDocumentsAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error, message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Adds a document to a client.
    /// </summary>
    [HttpPost("{id:guid}/documents")]
    [Authorize(Policy = "clients.update")]
    [ProducesResponseType(typeof(ClientDocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddDocument(Guid id, [FromBody] AddDocumentRequest request, CancellationToken ct)
    {
        var result = await _clientService.AddDocumentAsync(id, request, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error, message = result.Error });

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    /// <summary>
    /// Verifies a document.
    /// </summary>
    [HttpPost("{id:guid}/documents/{documentId:guid}/verify")]
    [Authorize(Policy = "clients.manage")]
    [ProducesResponseType(typeof(ClientDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyDocument(Guid id, Guid documentId, CancellationToken ct)
    {
        var result = await _clientService.VerifyDocumentAsync(id, documentId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error, message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes a document.
    /// </summary>
    [HttpDelete("{id:guid}/documents/{documentId:guid}")]
    [Authorize(Policy = "clients.manage")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(Guid id, Guid documentId, CancellationToken ct)
    {
        var result = await _clientService.DeleteDocumentAsync(id, documentId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error, message = result.Error });

        return NoContent();
    }

    #endregion

    #region Communication Logs

    /// <summary>
    /// Gets communication logs for a client.
    /// </summary>
    [HttpGet("{id:guid}/communications")]
    [Authorize(Policy = "clients.view")]
    [ProducesResponseType(typeof(PagedList<CommunicationLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCommunications(Guid id, [FromQuery] QueryParameters query, CancellationToken ct)
    {
        var result = await _clientService.GetCommunicationsAsync(id, query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Adds a communication log entry.
    /// </summary>
    [HttpPost("{id:guid}/communications")]
    [Authorize(Policy = "clients.update")]
    [ProducesResponseType(typeof(CommunicationLogDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddCommunication(Guid id, [FromBody] AddCommunicationRequest request, CancellationToken ct)
    {
        var result = await _clientService.AddCommunicationAsync(id, request, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error, message = result.Error });

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    #endregion

    #region Discount Cards

    /// <summary>
    /// Gets discount cards for a client.
    /// </summary>
    [HttpGet("{id:guid}/discount-cards")]
    [Authorize(Policy = "clients.view")]
    [ProducesResponseType(typeof(List<DiscountCardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDiscountCards(Guid id, CancellationToken ct)
    {
        var result = await _clientService.GetDiscountCardsAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error, message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Adds a discount card to a client.
    /// </summary>
    [HttpPost("{id:guid}/discount-cards")]
    [Authorize(Policy = "clients.update")]
    [ProducesResponseType(typeof(DiscountCardDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddDiscountCard(Guid id, [FromBody] AddDiscountCardRequest request, CancellationToken ct)
    {
        var result = await _clientService.AddDiscountCardAsync(id, request, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error, message = result.Error });

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    #endregion
}
