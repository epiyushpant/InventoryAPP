using Inventory.Data;
using Inventory.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Inventory.Models;

namespace Inventory.Controllers
{
    [Route("api/tenants")]
    [ApiController]
    [Authorize]
    public class TenantsController : ControllerBase
    {
        private readonly TenantCapabilityService _caps;
        private readonly RolePermissionService _permissions;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TenantsController(
            TenantCapabilityService caps,
            RolePermissionService permissions,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _caps = caps;
            _permissions = permissions;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentTenant()
        {
            var tenant = await _caps.GetCurrentOrDefaultTenantAsync();
            return Ok(new
            {
                tenantId = tenant.TenantId,
                name = tenant.Name,
                preset = tenant.Preset,
                isActive = tenant.IsActive
            });
        }

        [HttpGet("current/capabilities")]
        public async Task<IActionResult> GetCapabilities()
        {
            var tenant = await _caps.GetCurrentOrDefaultTenantAsync();
            var stored = await _caps.GetStoredMapAsync(tenant.TenantId);
            var effective = CapabilityCatalog.ComputeEffective(stored);
            var allowed = await _permissions.GetAllowedMapAsync(tenant.TenantId, CurrentRoles());

            // capabilities[] carries the raw tick an admin sees in the tree; `enabled` is the
            // cascaded result the rest of the app obeys.
            var catalog = CapabilityCatalog.All.Select(c => new
            {
                c.Key,
                c.Label,
                c.Category,
                c.Parent,
                c.Forms,
                c.Requires,
                enabled = stored.TryGetValue(c.Key, out var raw) && raw,
                effective = effective.TryGetValue(c.Key, out var eff) && eff
            });

            return Ok(new
            {
                tenantId = tenant.TenantId,
                name = tenant.Name,
                preset = tenant.Preset,
                capabilities = catalog,
                enabled = effective,
                allowed
            });
        }

        private List<string> CurrentRoles() =>
            User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();

        [HttpGet("catalog")]
        public IActionResult GetCatalog()
        {
            return Ok(new
            {
                presets = new[]
                {
                    CapabilityCatalog.PresetKirana,
                    CapabilityCatalog.PresetPharmacy,
                    CapabilityCatalog.PresetWholesale,
                    CapabilityCatalog.PresetFull
                },
                items = CapabilityCatalog.All
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("current/capabilities")]
        public async Task<IActionResult> PutCapabilities([FromBody] List<CapabilityToggleDto> body)
        {
            try
            {
                var tenant = await _caps.GetCurrentOrDefaultTenantAsync();
                await _caps.ReplaceCapabilitiesAsync(
                    tenant.TenantId,
                    (body ?? new()).Select(b => (b.Key, b.Enabled)));
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("current/apply-preset")]
        public async Task<IActionResult> ApplyPreset([FromBody] ApplyPresetDto body)
        {
            try
            {
                var preset = body?.Preset?.Trim() ?? CapabilityCatalog.PresetFull;
                var tenant = await _caps.GetCurrentOrDefaultTenantAsync();
                await _caps.ApplyPresetAsync(tenant.TenantId, preset);
                return Ok(new { message = $"Applied preset {preset}", preset });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Create a new shop + Admin user (Wave F provisioning).</summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("provision")]
        public async Task<IActionResult> Provision([FromBody] ProvisionTenantDto body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.AdminUsername) || string.IsNullOrWhiteSpace(body.AdminPassword))
                return BadRequest(new { message = "Admin username and password are required." });

            try
            {
                var tenant = await _caps.ProvisionTenantAsync(
                    body.Name ?? "New Shop",
                    body.Preset ?? CapabilityCatalog.PresetFull,
                    body.AdminUsername,
                    body.AdminEmail ?? "",
                    body.AdminPassword,
                    body.AdminFullName,
                    _userManager,
                    _roleManager);

                return Ok(new
                {
                    tenantId = tenant.TenantId,
                    name = tenant.Name,
                    preset = tenant.Preset,
                    adminUsername = body.AdminUsername.Trim(),
                    message = "Shop provisioned. Log in as the new admin to use an empty tenant."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class CapabilityToggleDto
    {
        public string Key { get; set; } = string.Empty;
        public bool Enabled { get; set; }
    }

    public class ApplyPresetDto
    {
        public string Preset { get; set; } = "Full";
    }

    public class ProvisionTenantDto
    {
        public string? Name { get; set; }
        public string? Preset { get; set; }
        public string AdminUsername { get; set; } = string.Empty;
        public string AdminPassword { get; set; } = string.Empty;
        public string? AdminEmail { get; set; }
        public string? AdminFullName { get; set; }
    }
}
