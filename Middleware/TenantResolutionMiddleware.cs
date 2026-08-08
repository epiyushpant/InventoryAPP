using Inventory.Services;

namespace Inventory.Middleware
{
    /// <summary>Reads JWT claim tenant_id into ITenantContext for EF filters and writes.</summary>
    public class TenantResolutionMiddleware
    {
        public const string TenantClaimType = "tenant_id";

        private readonly RequestDelegate _next;

        public TenantResolutionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext http, ITenantContext tenantContext)
        {
            var claim = http.User?.FindFirst(TenantClaimType)?.Value
                ?? http.User?.FindFirst("tenantId")?.Value;

            if (int.TryParse(claim, out var tenantId) && tenantId > 0)
                tenantContext.SetTenant(tenantId);

            await _next(http);
        }
    }
}
