using System.Security.Claims;
using Inventory.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Inventory.Filters
{
    /// <summary>
    /// Rejects the action when the shop does not have the capability enabled, or when the caller's
    /// roles are not granted it. Admin bypasses the role half.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireCapabilityAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _key;

        public RequireCapabilityAttribute(string key)
        {
            _key = key;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Only gate mutating verbs
            var method = context.HttpContext.Request.Method;
            if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method))
            {
                await next();
                return;
            }

            var services = context.HttpContext.RequestServices;

            if (services.GetService(typeof(TenantCapabilityService)) is not TenantCapabilityService caps)
            {
                await next();
                return;
            }

            if (!await caps.IsEnabledAsync(_key))
            {
                context.Result = new BadRequestObjectResult(new
                {
                    message = $"This feature is disabled for your business profile (capability '{_key}'). Ask an Admin to enable it or change the preset."
                });
                return;
            }

            var roles = context.HttpContext.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            if (!roles.Any(RolePermissionService.IsAdminRole)
                && services.GetService(typeof(RolePermissionService)) is RolePermissionService permissions)
            {
                var tenant = await caps.GetCurrentOrDefaultTenantAsync();
                var allowed = await permissions.GetAllowedMapAsync(tenant.TenantId, roles);
                if (allowed.TryGetValue(_key, out var granted) && !granted)
                {
                    context.Result = new ObjectResult(new
                    {
                        message = $"Your role does not have access to this screen ('{_key}'). Ask an Admin to grant it."
                    })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                    return;
                }
            }

            await next();
        }
    }
}
