using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Candidate.Contracts;
using Worker.Contracts;
using Contract.Contracts;
using Client.Contracts;
using Document.Contracts;
using Audit.Contracts;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

// ──── DTOs ────

public sealed record DashboardSummaryDto
{
    public DashboardKpisDto Kpis { get; init; } = new();
    public DashboardComplianceDto Compliance { get; init; } = new();
    public List<DashboardActivityItemDto> RecentActivity { get; init; } = [];
    public DateTimeOffset GeneratedAt { get; init; }
}

public sealed record DashboardKpisDto
{
    public int ActiveWorkers { get; init; }
    public int TotalWorkers { get; init; }
    public int ActiveContracts { get; init; }
    public int TotalContracts { get; init; }
    public int PendingCandidates { get; init; }
    public int TotalCandidates { get; init; }
    public int ActiveClients { get; init; }
    public int TotalClients { get; init; }
}

public sealed record DashboardComplianceDto
{
    public int TotalDocuments { get; init; }
    public int Valid { get; init; }
    public int ExpiringSoon { get; init; }
    public int Expired { get; init; }
    public int Pending { get; init; }
    public double ComplianceRate { get; init; }
}

public sealed record DashboardActivityItemDto
{
    public Guid Id { get; init; }
    public string EventName { get; init; } = string.Empty;
    public string? EntityName { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

// ──── Controller ────

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/dashboard")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
[HasPermission("dashboard.view")]
public class DashboardController : ControllerBase
{
    private readonly ICandidateService _candidateService;
    private readonly IWorkerService _workerService;
    private readonly IContractService _contractService;
    private readonly IClientService _clientService;
    private readonly IDocumentService _documentService;
    private readonly IAuditService _auditService;

    private static readonly HashSet<string> ActiveWorkerStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Active", "Hired", "OnProbation", "Renewed", "Booked",
    };

    private static readonly HashSet<string> ActiveContractStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Active", "OnProbation", "Confirmed",
    };

    private static readonly HashSet<string> PendingCandidateStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Received", "UnderReview",
    };

    public DashboardController(
        ICandidateService candidateService,
        IWorkerService workerService,
        IContractService contractService,
        IClientService clientService,
        IDocumentService documentService,
        IAuditService auditService)
    {
        _candidateService = candidateService;
        _workerService = workerService;
        _contractService = contractService;
        _clientService = clientService;
        _documentService = documentService;
        _auditService = auditService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(Guid tenantId, CancellationToken ct)
    {
        // Run sequentially — all services share the same scoped DbContext which is not thread-safe
        var candidateCounts = await _candidateService.GetCountsByStatusAsync(tenantId, ct);
        var workerCounts = await _workerService.GetCountsByStatusAsync(tenantId, ct);
        var contractCounts = await _contractService.GetCountsByStatusAsync(tenantId, ct);
        var clientCounts = await _clientService.GetClientCountsAsync(tenantId, ct);
        var compliance = await _documentService.GetComplianceSummaryAsync(tenantId, ct);
        var auditEvents = await _auditService.GetEventsAsync(tenantId, new QueryParameters { PageSize = 10, Sort = "-createdAt" }, ct);

        // Compute KPIs
        var activeWorkers = workerCounts
            .Where(kv => ActiveWorkerStatuses.Contains(kv.Key))
            .Sum(kv => kv.Value);
        var totalWorkers = workerCounts.Values.Sum();

        var activeContracts = contractCounts
            .Where(kv => ActiveContractStatuses.Contains(kv.Key))
            .Sum(kv => kv.Value);
        var totalContracts = contractCounts.Values.Sum();

        var pendingCandidates = candidateCounts
            .Where(kv => PendingCandidateStatuses.Contains(kv.Key))
            .Sum(kv => kv.Value);
        var totalCandidates = candidateCounts.Values.Sum();

        // Compute compliance rate
        var complianceRate = compliance.TotalDocuments > 0
            ? Math.Round((double)compliance.Valid / compliance.TotalDocuments * 100, 1)
            : 0;

        // Map recent activity
        var recentActivity = auditEvents.Items.Select(e => new DashboardActivityItemDto
        {
            Id = e.Id,
            EventName = e.EventName,
            EntityName = ExtractEntityName(e.Payload),
            CreatedAt = e.CreatedAt,
        }).ToList();

        var summary = new DashboardSummaryDto
        {
            Kpis = new DashboardKpisDto
            {
                ActiveWorkers = activeWorkers,
                TotalWorkers = totalWorkers,
                ActiveContracts = activeContracts,
                TotalContracts = totalContracts,
                PendingCandidates = pendingCandidates,
                TotalCandidates = totalCandidates,
                ActiveClients = clientCounts.Active,
                TotalClients = clientCounts.Total,
            },
            Compliance = new DashboardComplianceDto
            {
                TotalDocuments = compliance.TotalDocuments,
                Valid = compliance.Valid,
                ExpiringSoon = compliance.ExpiringSoon,
                Expired = compliance.Expired,
                Pending = compliance.Pending,
                ComplianceRate = complianceRate,
            },
            RecentActivity = recentActivity,
            GeneratedAt = DateTimeOffset.UtcNow,
        };

        return Ok(summary);
    }

    private static string? ExtractEntityName(string? payload)
    {
        if (string.IsNullOrEmpty(payload)) return null;

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("FullNameEn", out var name))
                return name.GetString();
            if (doc.RootElement.TryGetProperty("NameEn", out var nameEn))
                return nameEn.GetString();
            if (doc.RootElement.TryGetProperty("ContractCode", out var code))
                return code.GetString();
        }
        catch
        {
            // Not valid JSON, ignore
        }

        return null;
    }
}
