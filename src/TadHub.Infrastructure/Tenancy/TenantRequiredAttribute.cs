using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Interfaces;

namespace TadHub.Infrastructure.Tenancy;

/// <summary>
/// Action filter that requires a tenant to be resolved for the request.
/// Returns 400 Bad Request if no tenant context is available.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class TenantRequiredAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var tenantContext = context.HttpContext.RequestServices.GetService(typeof(ITenantContext)) as ITenantContext;

        if (tenantContext is null || !tenantContext.IsResolved)
        {
            var error = ApiError.BadRequest(
                "Tenant context is required for this operation. " +
                "Provide tenant via X-Tenant-Id header, JWT claim, or subdomain.",
                "TENANT_REQUIRED");

            context.Result = new BadRequestObjectResult(error);
            return;
        }

        base.OnActionExecuting(context);
    }
}
