using Inventory.Models;
using Inventory.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Data
{
    public class TenantCapabilityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantContext _tenantContext;

        public TenantCapabilityService(ApplicationDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<Tenant> GetOrCreateDefaultTenantAsync()
        {
            var tenant = await _context.Tenants.Include(t => t.Capabilities).FirstOrDefaultAsync(t => t.IsActive);
            if (tenant != null) return tenant;

            tenant = new Tenant
            {
                Name = "Default Shop",
                Preset = CapabilityCatalog.PresetFull,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            await ApplyPresetAsync(tenant.TenantId, CapabilityCatalog.PresetFull);
            return await _context.Tenants.Include(t => t.Capabilities).FirstAsync(t => t.TenantId == tenant.TenantId);
        }

        public async Task<Tenant> GetCurrentOrDefaultTenantAsync()
        {
            if (_tenantContext.HasTenant)
            {
                var current = await _context.Tenants.Include(t => t.Capabilities)
                    .FirstOrDefaultAsync(t => t.TenantId == _tenantContext.TenantId);
                if (current != null) return current;
            }

            return await GetOrCreateDefaultTenantAsync();
        }

        /// <summary>
        /// Raw tick state as an admin sees it in the tree — no cascade applied. Stored rows are
        /// mapped through <see cref="CapabilityCatalog.LegacyKeys"/>, and catalog keys the tenant
        /// has never stored fall back to their preset membership.
        /// </summary>
        public async Task<Dictionary<string, bool>> GetStoredMapAsync(int? tenantId = null)
        {
            var tenant = tenantId.HasValue
                ? await _context.Tenants.Include(t => t.Capabilities).FirstOrDefaultAsync(t => t.TenantId == tenantId)
                : await GetCurrentOrDefaultTenantAsync();

            if (tenant == null)
                return CapabilityCatalog.All.ToDictionary(c => c.Key, _ => true, StringComparer.OrdinalIgnoreCase);

            var map = CapabilityCatalog.All.ToDictionary(c => c.Key, _ => false, StringComparer.OrdinalIgnoreCase);

            if (tenant.Capabilities.Count == 0)
            {
                foreach (var key in map.Keys.ToList())
                    map[key] = true;
                return map;
            }

            var stored = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in tenant.Capabilities)
            {
                var key = CapabilityCatalog.ResolveKey(row.Key);
                if (key == null) continue;
                // Several legacy modules can collapse onto one key — any of them being on wins.
                stored[key] = stored.TryGetValue(key, out var existing) ? existing || row.Enabled : row.Enabled;
            }

            var presetName = string.Equals(tenant.Preset, CapabilityCatalog.PresetCustom, StringComparison.OrdinalIgnoreCase)
                ? CapabilityCatalog.PresetFull
                : tenant.Preset;
            var presetEnabled = new HashSet<string>(
                CapabilityCatalog.KeysForPreset(presetName),
                StringComparer.OrdinalIgnoreCase);

            foreach (var key in map.Keys.ToList())
            {
                map[key] = stored.TryGetValue(key, out var enabled)
                    ? enabled
                    : presetEnabled.Contains(key);
            }

            return map;
        }

        /// <summary>Stored ticks with the module/feature cascade applied — what the app should obey.</summary>
        public async Task<Dictionary<string, bool>> GetEnabledMapAsync(int? tenantId = null)
        {
            var stored = await GetStoredMapAsync(tenantId);
            return CapabilityCatalog.ComputeEffective(stored);
        }

        public async Task<bool> IsEnabledAsync(string key)
        {
            var map = await GetEnabledMapAsync();
            if (!map.ContainsKey(key)) return true;
            return map[key];
        }

        public async Task ApplyPresetAsync(int tenantId, string preset)
        {
            var tenant = await _context.Tenants.Include(t => t.Capabilities)
                .FirstOrDefaultAsync(t => t.TenantId == tenantId)
                ?? throw new InvalidOperationException("Tenant not found.");

            var enabledKeys = new HashSet<string>(CapabilityCatalog.KeysForPreset(preset), StringComparer.OrdinalIgnoreCase);
            _context.TenantCapabilities.RemoveRange(tenant.Capabilities);

            foreach (var def in CapabilityCatalog.All)
            {
                _context.TenantCapabilities.Add(new TenantCapability
                {
                    TenantId = tenantId,
                    Key = def.Key,
                    Enabled = enabledKeys.Contains(def.Key)
                });
            }

            tenant.Preset = preset;
            await _context.SaveChangesAsync();
        }

        public async Task ReplaceCapabilitiesAsync(int tenantId, IEnumerable<(string Key, bool Enabled)> items)
        {
            var tenant = await _context.Tenants.Include(t => t.Capabilities)
                .FirstOrDefaultAsync(t => t.TenantId == tenantId)
                ?? throw new InvalidOperationException("Tenant not found.");

            var incoming = items
                .Where(i => CapabilityCatalog.IsKnown(i.Key))
                .GroupBy(i => i.Key)
                .ToDictionary(g => g.Key, g => g.Last().Enabled, StringComparer.OrdinalIgnoreCase);

            _context.TenantCapabilities.RemoveRange(tenant.Capabilities);

            foreach (var def in CapabilityCatalog.All)
            {
                var enabled = incoming.TryGetValue(def.Key, out var e) && e;
                _context.TenantCapabilities.Add(new TenantCapability
                {
                    TenantId = tenantId,
                    Key = def.Key,
                    Enabled = enabled
                });
            }

            tenant.Preset = CapabilityCatalog.PresetCustom;
            await _context.SaveChangesAsync();
        }

        public async Task<Tenant> ProvisionTenantAsync(string name, string preset, string adminUsername, string adminEmail, string adminPassword, string? adminFullName, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var tenant = new Tenant
            {
                Name = string.IsNullOrWhiteSpace(name) ? "New Shop" : name.Trim(),
                Preset = string.IsNullOrWhiteSpace(preset) ? CapabilityCatalog.PresetFull : preset.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            await ApplyPresetAsync(tenant.TenantId, tenant.Preset);

            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            // Clear request tenant so SaveChanges does not overwrite the new shop's TenantId
            var previousTenantId = _tenantContext.HasTenant ? _tenantContext.TenantId : (int?)null;
            _tenantContext.Clear();

            try
            {
                var admin = new ApplicationUser
                {
                    UserName = adminUsername.Trim(),
                    Email = string.IsNullOrWhiteSpace(adminEmail) ? $"{adminUsername.Trim()}@local" : adminEmail.Trim(),
                    FullName = adminFullName,
                    Role = "Admin",
                    TenantId = tenant.TenantId
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (!result.Succeeded)
                {
                    _context.Tenants.Remove(tenant);
                    await _context.SaveChangesAsync();
                    throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
                }

                await userManager.AddToRoleAsync(admin, "Admin");
                return tenant;
            }
            finally
            {
                if (previousTenantId.HasValue)
                    _tenantContext.SetTenant(previousTenantId.Value);
            }
        }
    }
}
