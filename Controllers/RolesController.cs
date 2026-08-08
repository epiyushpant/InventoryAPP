using Inventory.Data;
using Inventory.Models;
using Inventory.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Controllers
{
    [Route("api/roles")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly RolePermissionService _permissions;
        private readonly TenantCapabilityService _caps;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(
            RolePermissionService permissions,
            TenantCapabilityService caps,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _permissions = permissions;
            _caps = caps;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var tenant = await _caps.GetCurrentOrDefaultTenantAsync();

            var names = RolePermissionService.KnownRoles
                .Concat(await _roleManager.Roles.Select(r => r.Name!).ToListAsync())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var result = new List<object>();
            foreach (var name in names)
            {
                var users = await _userManager.GetUsersInRoleAsync(name);
                var (_, isDefault) = await _permissions.GetForRoleAsync(tenant.TenantId, name);
                result.Add(new
                {
                    name,
                    userCount = users.Count,
                    isAdmin = RolePermissionService.IsAdminRole(name),
                    isDefault
                });
            }

            return Ok(new { tenantId = tenant.TenantId, roles = result });
        }

        [HttpGet("{role}/permissions")]
        public async Task<IActionResult> GetRolePermissions(string role)
        {
            var tenant = await _caps.GetCurrentOrDefaultTenantAsync();
            var (permissions, isDefault) = await _permissions.GetForRoleAsync(tenant.TenantId, role);
            var enabled = await _caps.GetEnabledMapAsync(tenant.TenantId);

            return Ok(new
            {
                role,
                isAdmin = RolePermissionService.IsAdminRole(role),
                isDefault,
                permissions = permissions.Select(p => new { key = p.Key, allowed = p.Value }),
                // The shop's effective capabilities — a row off here cannot be granted to anyone
                enabled
            });
        }

        [HttpPut("{role}/permissions")]
        public async Task<IActionResult> PutRolePermissions(string role, [FromBody] List<RolePermissionDto> body)
        {
            try
            {
                var tenant = await _caps.GetCurrentOrDefaultTenantAsync();
                await _permissions.ReplaceForRoleAsync(
                    tenant.TenantId,
                    role,
                    (body ?? new()).Select(b => (b.Key, b.Allowed)));
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{role}/reset")]
        public async Task<IActionResult> ResetRole(string role)
        {
            try
            {
                var tenant = await _caps.GetCurrentOrDefaultTenantAsync();
                await _permissions.ResetRoleAsync(tenant.TenantId, role);
                return Ok(new { message = $"Reset {role} to default permissions." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class RolePermissionDto
    {
        public string Key { get; set; } = string.Empty;
        public bool Allowed { get; set; }
    }
}
