using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using Financial.Contracts;
using Financial.Contracts.DTOs;
using Financial.Contracts.Settings;
using Worker.Contracts;
using Client.Contracts;
using Contract.Contracts;
using Tenancy.Contracts;
using TadHub.Api.Documents;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.Infrastructure.Storage;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/invoices")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ITenantService _tenantService;
    private readonly IClientService _clientService;
    private readonly IWorkerService _workerService;
    private readonly IContractService _contractService;
    private readonly IFileStorageService _fileStorageService;

    public InvoicesController(
        IInvoiceService invoiceService,
        ITenantService tenantService,
        IClientService clientService,
        IWorkerService workerService,
        IContractService contractService,
        IFileStorageService fileStorageService)
    {
        _invoiceService = invoiceService;
        _tenantService = tenantService;
        _clientService = clientService;
        _workerService = workerService;
        _contractService = contractService;
        _fileStorageService = fileStorageService;
    }

    [HttpGet]
    [HasPermission("invoices.view")]
    [ProducesResponseType(typeof(PagedList<InvoiceListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _invoiceService.ListAsync(tenantId, qp, ct);
        result = await EnrichList(tenantId, result, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("invoices.view")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid tenantId,
        Guid id,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _invoiceService.GetByIdAsync(tenantId, id, qp, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = await EnrichSingle(tenantId, result.Value!, ct);
        return Ok(dto);
    }

    [HttpPost]
    [HasPermission("invoices.create")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        Guid tenantId,
        [FromBody] CreateInvoiceRequest request,
        CancellationToken ct)
    {
        var result = await _invoiceService.CreateAsync(tenantId, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;
        return CreatedAtAction(nameof(GetById), new { tenantId, id = dto.Id }, dto);
    }

    [HttpPost("generate")]
    [HasPermission("invoices.create")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Generate(
        Guid tenantId,
        [FromBody] GenerateInvoiceRequest request,
        CancellationToken ct)
    {
        var result = await _invoiceService.GenerateForContractAsync(tenantId, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;
        return CreatedAtAction(nameof(GetById), new { tenantId, id = dto.Id }, dto);
    }

    [HttpPatch("{id:guid}")]
    [HasPermission("invoices.edit")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid tenantId,
        Guid id,
        [FromBody] UpdateInvoiceRequest request,
        CancellationToken ct)
    {
        var result = await _invoiceService.UpdateAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/status")]
    [HasPermission("invoices.manage_status")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransitionStatus(
        Guid tenantId,
        Guid id,
        [FromBody] TransitionInvoiceStatusRequest request,
        CancellationToken ct)
    {
        var result = await _invoiceService.TransitionStatusAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/credit-note")]
    [HasPermission("invoices.create")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCreditNote(
        Guid tenantId,
        Guid id,
        [FromBody] CreateCreditNoteRequest request,
        CancellationToken ct)
    {
        var result = await _invoiceService.CreateCreditNoteAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;
        return CreatedAtAction(nameof(GetById), new { tenantId, id = dto.Id }, dto);
    }

    [HttpPost("{id:guid}/discount")]
    [HasPermission("invoices.edit")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApplyDiscount(
        Guid tenantId,
        Guid id,
        [FromBody] ApplyDiscountRequest request,
        CancellationToken ct)
    {
        var result = await _invoiceService.ApplyDiscountAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/pdf")]
    [HasPermission("invoices.view")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadPdf(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        // 1. Fetch invoice with includes
        var invoiceResult = await _invoiceService.GetByIdAsync(tenantId, id,
            new QueryParameters { Include = "lineItems,payments" }, ct);
        if (!invoiceResult.IsSuccess)
            return MapResultError(invoiceResult);

        var invoice = invoiceResult.Value!;

        // 2. Fetch tenant info
        var tenantResult = await _tenantService.GetByIdAsync(tenantId, ct);
        var tenantName = tenantResult.IsSuccess ? tenantResult.Value!.Name : "TadHub";
        var tenantNameAr = tenantResult.IsSuccess ? tenantResult.Value!.NameAr : null;
        var tenantWebsite = tenantResult.IsSuccess ? tenantResult.Value!.Website : null;

        // 3. Fetch financial settings (template config, footer, terms)
        var template = new InvoiceTemplateSettings();
        string? footerText = null, footerTextAr = null, terms = null, termsAr = null;

        var settingsResult = await _tenantService.GetSettingsJsonAsync(tenantId, ct);
        if (settingsResult.IsSuccess && !string.IsNullOrWhiteSpace(settingsResult.Value))
        {
            var root = JsonNode.Parse(settingsResult.Value);
            var financialNode = root?["financial"];
            if (financialNode is not null)
            {
                var fs = financialNode.Deserialize<TenantFinancialSettings>();
                if (fs is not null)
                {
                    template = fs.InvoiceTemplate;
                    footerText = fs.InvoiceFooterText;
                    footerTextAr = fs.InvoiceFooterTextAr;
                    terms = fs.InvoiceTerms;
                    termsAr = fs.InvoiceTermsAr;
                }
            }
        }

        // 4. Download tenant logo from MinIO
        var logoKey = tenantResult.IsSuccess ? tenantResult.Value!.LogoUrl : null;
        byte[]? tenantLogo = null;
        if (template.ShowLogo && !string.IsNullOrEmpty(logoKey))
        {
            try { tenantLogo = await _fileStorageService.DownloadAsync(logoKey, ct); }
            catch { /* leave null */ }
        }

        // 5. Enrich client name
        string? clientName = null, clientNameAr = null;
        var clientResult = await _clientService.GetByIdAsync(tenantId, invoice.ClientId, ct);
        if (clientResult.IsSuccess)
        {
            clientName = clientResult.Value!.NameEn;
            clientNameAr = clientResult.Value!.NameAr;
        }

        // 6. Enrich worker name (if applicable)
        string? workerName = null, workerNameAr = null, workerCode = null;
        if (invoice.WorkerId.HasValue)
        {
            var workerResult = await _workerService.GetByIdAsync(tenantId, invoice.WorkerId.Value, ct: ct);
            if (workerResult.IsSuccess)
            {
                workerName = workerResult.Value!.FullNameEn;
                workerNameAr = workerResult.Value!.FullNameAr;
                workerCode = workerResult.Value!.WorkerCode;
            }
        }

        // 7. Build PDF
        var pdfData = new InvoicePdfData(
            invoice, tenantName, tenantNameAr, tenantWebsite, tenantLogo,
            template, footerText, footerTextAr, terms, termsAr,
            clientName, clientNameAr, workerName, workerNameAr, workerCode);

        var document = new InvoiceDocument(pdfData);
        var pdfBytes = document.GeneratePdf();

        // 8. Return
        var fileName = $"Invoice-{invoice.InvoiceNumber}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("invoices.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _invoiceService.DeleteAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return NoContent();
    }

    [HttpGet("summary")]
    [HasPermission("invoices.view")]
    [ProducesResponseType(typeof(InvoiceSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(
        Guid tenantId,
        CancellationToken ct)
    {
        var result = await _invoiceService.GetSummaryAsync(tenantId, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    #region BFF Enrichment

    private async Task<PagedList<InvoiceListDto>> EnrichList(
        Guid tenantId, PagedList<InvoiceListDto> pagedList, CancellationToken ct)
    {
        var clientIds = pagedList.Items.Select(i => i.ClientId).Distinct().ToList();
        var workerIds = pagedList.Items.Where(i => i.WorkerId.HasValue).Select(i => i.WorkerId!.Value).Distinct().ToList();
        var contractIds = pagedList.Items.Select(i => i.ContractId).Distinct().ToList();

        var clientMap = await BuildClientMap(tenantId, clientIds, ct);
        var workerMap = await BuildWorkerMap(tenantId, workerIds, ct);
        var contractMap = await BuildContractMap(tenantId, contractIds, ct);

        var enriched = pagedList.Items.Select(i => i with
        {
            Client = clientMap.GetValueOrDefault(i.ClientId),
            Worker = i.WorkerId.HasValue ? workerMap.GetValueOrDefault(i.WorkerId.Value) : null,
            Contract = contractMap.GetValueOrDefault(i.ContractId),
        }).ToList();

        return new PagedList<InvoiceListDto>(enriched, pagedList.TotalCount, pagedList.Page, pagedList.PageSize);
    }

    private async Task<InvoiceDto> EnrichSingle(Guid tenantId, InvoiceDto dto, CancellationToken ct)
    {
        InvoiceClientRef? clientRef = null;
        var clientResult = await _clientService.GetByIdAsync(tenantId, dto.ClientId, ct);
        if (clientResult.IsSuccess)
            clientRef = new InvoiceClientRef { Id = clientResult.Value!.Id, NameEn = clientResult.Value.NameEn, NameAr = clientResult.Value.NameAr };

        InvoiceWorkerRef? workerRef = null;
        if (dto.WorkerId.HasValue)
        {
            var workerResult = await _workerService.GetByIdAsync(tenantId, dto.WorkerId.Value, ct: ct);
            if (workerResult.IsSuccess)
                workerRef = new InvoiceWorkerRef { Id = workerResult.Value!.Id, FullNameEn = workerResult.Value.FullNameEn, FullNameAr = workerResult.Value.FullNameAr, WorkerCode = workerResult.Value.WorkerCode };
        }

        InvoiceContractRef? contractRef = null;
        var contractResult = await _contractService.GetByIdAsync(tenantId, dto.ContractId, ct: ct);
        if (contractResult.IsSuccess)
            contractRef = new InvoiceContractRef { Id = contractResult.Value!.Id, ContractCode = contractResult.Value.ContractCode };

        return dto with { Client = clientRef, Worker = workerRef, Contract = contractRef };
    }

    private async Task<Dictionary<Guid, InvoiceClientRef>> BuildClientMap(Guid tenantId, List<Guid> ids, CancellationToken ct)
    {
        var map = new Dictionary<Guid, InvoiceClientRef>();
        if (ids.Count == 0) return map;
        var result = await _clientService.ListAsync(tenantId, new QueryParameters { PageSize = ids.Count }, ct);
        foreach (var c in result.Items.Where(c => ids.Contains(c.Id)))
            map[c.Id] = new InvoiceClientRef { Id = c.Id, NameEn = c.NameEn, NameAr = c.NameAr };
        return map;
    }

    private async Task<Dictionary<Guid, InvoiceWorkerRef>> BuildWorkerMap(Guid tenantId, List<Guid> ids, CancellationToken ct)
    {
        var map = new Dictionary<Guid, InvoiceWorkerRef>();
        if (ids.Count == 0) return map;
        var result = await _workerService.ListAsync(tenantId, new QueryParameters { PageSize = ids.Count }, ct);
        foreach (var w in result.Items.Where(w => ids.Contains(w.Id)))
            map[w.Id] = new InvoiceWorkerRef { Id = w.Id, FullNameEn = w.FullNameEn, FullNameAr = w.FullNameAr, WorkerCode = w.WorkerCode };
        return map;
    }

    private async Task<Dictionary<Guid, InvoiceContractRef>> BuildContractMap(Guid tenantId, List<Guid> ids, CancellationToken ct)
    {
        var map = new Dictionary<Guid, InvoiceContractRef>();
        if (ids.Count == 0) return map;
        var result = await _contractService.ListAsync(tenantId, new QueryParameters { PageSize = ids.Count }, ct);
        foreach (var c in result.Items.Where(c => ids.Contains(c.Id)))
            map[c.Id] = new InvoiceContractRef { Id = c.Id, ContractCode = c.ContractCode };
        return map;
    }

    #endregion

    #region Error Helpers

    private IActionResult MapResultError<T>(Result<T> result)
        => MapError(result.Error!, result.ErrorCode);

    private IActionResult MapResultError(Result result)
        => MapError(result.Error!, result.ErrorCode);

    private IActionResult MapError(string error, string? errorCode)
    {
        var path = HttpContext.Request.Path.Value;
        var (status, apiError) = errorCode switch
        {
            "NOT_FOUND" => (404, ApiError.NotFound(error, path)),
            "CONFLICT" => (409, ApiError.Conflict(error, path)),
            "FORBIDDEN" => (403, ApiError.Forbidden(error)),
            _ => (400, ApiError.BadRequest(error, path))
        };
        return new ObjectResult(apiError) { StatusCode = status, ContentTypes = { "application/problem+json" } };
    }

    #endregion
}
