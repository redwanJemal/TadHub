using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Audit.Contracts;
using Audit.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Audit.Core.Services;

public class WebhookService : IWebhookService
{
    private readonly AppDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(AppDbContext db, IHttpClientFactory httpClientFactory, ILogger<WebhookService> logger)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<PagedList<WebhookDto>> GetWebhooksAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var webhooks = await _db.Set<Webhook>().AsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.CreatedAt).ToPagedListAsync(qp, ct);
        var dtos = webhooks.Items.Select(x => new WebhookDto(x.Id, x.Url, x.Events != null ? JsonSerializer.Deserialize<List<string>>(x.Events) : null, x.IsActive, x.LastTriggeredAt, x.FailureCount, x.CreatedAt)).ToList();
        return new PagedList<WebhookDto>(dtos, webhooks.TotalCount, webhooks.Page, webhooks.PageSize);
    }

    public async Task<Result<WebhookDto>> CreateWebhookAsync(Guid tenantId, CreateWebhookRequest request, CancellationToken ct = default)
    {
        var webhook = new Webhook
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Url = request.Url,
            Events = request.Events != null ? JsonSerializer.Serialize(request.Events) : null,
            Secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
            IsActive = true
        };
        _db.Set<Webhook>().Add(webhook);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Created webhook {WebhookId} for tenant {TenantId}", webhook.Id, tenantId);
        return Result<WebhookDto>.Success(new WebhookDto(webhook.Id, webhook.Url, request.Events, webhook.IsActive, null, 0, webhook.CreatedAt));
    }

    public async Task<Result<bool>> DeleteWebhookAsync(Guid tenantId, Guid webhookId, CancellationToken ct = default)
    {
        var webhook = await _db.Set<Webhook>().FirstOrDefaultAsync(x => x.Id == webhookId && x.TenantId == tenantId, ct);
        if (webhook is null) return Result<bool>.NotFound("Webhook not found");
        _db.Set<Webhook>().Remove(webhook);
        await _db.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    public async Task TriggerWebhooksAsync(Guid tenantId, string eventName, object payload, CancellationToken ct = default)
    {
        var webhooks = await _db.Set<Webhook>().Where(x => x.TenantId == tenantId && x.IsActive).ToListAsync(ct);
        var client = _httpClientFactory.CreateClient();

        foreach (var webhook in webhooks)
        {
            if (webhook.Events != null)
            {
                var events = JsonSerializer.Deserialize<List<string>>(webhook.Events);
                if (events != null && !events.Contains(eventName) && !events.Contains("*")) continue;
            }

            var delivery = new WebhookDelivery { Id = Guid.NewGuid(), TenantId = tenantId, WebhookId = webhook.Id, EventName = eventName, Payload = JsonSerializer.Serialize(payload), Status = "pending" };
            _db.Set<WebhookDelivery>().Add(delivery);

            try
            {
                var response = await client.PostAsJsonAsync(webhook.Url, new { @event = eventName, payload, timestamp = DateTimeOffset.UtcNow }, ct);
                delivery.StatusCode = (int)response.StatusCode;
                delivery.Status = response.IsSuccessStatusCode ? "success" : "failed";
                delivery.DeliveredAt = DateTimeOffset.UtcNow;
                delivery.Attempts = 1;
                webhook.LastTriggeredAt = DateTimeOffset.UtcNow;
                if (!response.IsSuccessStatusCode) webhook.FailureCount++;
            }
            catch (Exception ex)
            {
                delivery.Status = "failed";
                delivery.ErrorMessage = ex.Message;
                delivery.Attempts = 1;
                webhook.FailureCount++;
                _logger.LogWarning(ex, "Failed to deliver webhook {WebhookId}", webhook.Id);
            }
        }
        await _db.SaveChangesAsync(ct);
    }
}
