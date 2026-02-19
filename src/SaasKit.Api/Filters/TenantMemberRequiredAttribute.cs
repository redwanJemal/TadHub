using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SaasKit.SharedKernel.Api;
using SaasKit.SharedKernel.Interfaces;
using Tenancy.Contracts;

namespace SaasKit.Api.Filters;

/// <summary>
/// Action filter that requires the current user to be a member of the specified tenant.
/// Must be used after [Authorize] and [TenantRequired].
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class TenantMemberRequiredAttribute : ActionFilterAttribute
{
    /// <summary>
    /// The route parameter name containing the tenant ID. Default: "tenantId" or "id".
    /// </summary>
    public string TenantIdParameter { get; set; } = "id";

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var tenantService = context.HttpContext.RequestServices.GetService<ITenantService>();
        var currentUser = context.HttpContext.RequestServices.GetService<ICurrentUser>();

        if (tenantService is null || currentUser is null || !currentUser.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Try to get tenant ID from route
        Guid tenantId = Guid.Empty;
        
        if (context.ActionArguments.TryGetValue(TenantIdParameter, out var idValue) && idValue is Guid id)
        {
            tenantId = id;
        }
        else if (context.ActionArguments.TryGetValue("tenantId", out var tenantIdValue) && tenantIdValue is Guid tid)
        {
            tenantId = tid;
        }

        if (tenantId == Guid.Empty)
        {
            // Try tenant context
            var tenantContext = context.HttpContext.RequestServices.GetService<ITenantContext>();
            if (tenantContext?.IsResolved == true)
            {
                tenantId = tenantContext.TenantId;
            }
        }

        if (tenantId == Guid.Empty)
        {
            context.Result = new BadRequestObjectResult(
                ApiError.BadRequest("Tenant ID is required", "TENANT_REQUIRED"));
            return;
        }

        // Check membership
        var isMember = await tenantService.IsMemberAsync(tenantId, currentUser.UserId, context.HttpContext.RequestAborted);
        
        if (!isMember)
        {
            context.Result = new ObjectResult(
                ApiError.Forbidden("You are not a member of this tenant"))
            {
                StatusCode = 403
            };
            return;
        }

        await next();
    }
}
