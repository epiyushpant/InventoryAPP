using Inventory.Models;
using Inventory.Services;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Data
{
    /// <summary>
    /// Per-role, per-shop grants over the capability catalog. Only leaf keys (pages, fields,
    /// reports) are stored — module rows in the admin tree are derived from their children, and
    /// features are a shop-level switch rather than something a role owns.
    /// </summary>
    public class RolePermissionService
    {
        public const string AdminRole = "Admin";

        private readonly ApplicationDbContext _context;
        private readonly TenantCapabilityService _caps;

        public RolePermissionService(ApplicationDbContext context, TenantCapabilityService caps)
        {
            _context = context;
            _caps = caps;
        }

        /// <summary>Roles the app ships with. Admin is always full access and never editable.</summary>
        public static readonly IReadOnlyList<string> KnownRoles = new[]
        {
            AdminRole, "Accountant", "Sales", "Inventory", "User"
        };

        /// <summary>
        /// Seed grants, ported from the hardcoded role arrays that used to live in the web layout.
        /// Used until an admin saves the role, and by "reset to default".
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string[]> RoleDefaults =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["Inventory"] = new[]
                {
                    "form.categories", "form.locations", "form.suppliers", "form.products",
                    "form.unitConversions",
                    "form.inventories", "form.stockMovements", "form.stockAdjustments", "form.stockTransfers",
                    "form.purchaseRequisitions", "form.purchaseOrders", "form.grns",
                    "form.deliveryNotes",
                    "form.reports",
                    "report.stock-summary", "report.low-stock", "report.stock-ledger",
                    "report.fiscal-year-stock", "report.expiry-soon",
                    "report.purchase-history", "report.vat-purchase-register",
                    "field.product.expiryDate", "field.grn.otherExpenses"
                },
                ["Sales"] = new[]
                {
                    "form.customers", "form.products",
                    "form.sales", "form.deliveryNotes", "form.salesInvoices",
                    "form.reports",
                    "report.sales-history", "report.vat-sales-register",
                    "field.invoice.paymentStatus"
                },
                ["Accountant"] = new[]
                {
                    "form.customers", "form.products",
                    "form.sales", "form.salesInvoices", "form.stockMovements",
                    "form.reports",
                    "report.stock-summary", "report.low-stock", "report.stock-ledger",
                    "report.fiscal-year-stock", "report.expiry-soon",
                    "report.purchase-history", "report.vat-purchase-register",
                    "report.sales-history", "report.vat-sales-register",
                    "field.invoice.paymentStatus", "field.product.expiryDate"
                },
                // Dashboard only, matching the old behaviour for a plain user
                ["User"] = Array.Empty<string>()
            };

        /// <summary>Leaf keys a role can be granted. Modules and features are shop-level.</summary>
        public static IEnumerable<CapabilityDef> GrantableKeys() =>
            CapabilityCatalog.All.Where(c =>
                c.Category is CapabilityCatalog.CategoryForm
                    or CapabilityCatalog.CategoryField
                    or CapabilityCatalog.CategoryReport);

        public static bool IsAdminRole(string role) =>
            string.Equals(role, AdminRole, StringComparison.OrdinalIgnoreCase);

        private static Dictionary<string, bool> DefaultsFor(string role)
        {
            var granted = RoleDefaults.TryGetValue(role, out var keys) ? keys : Array.Empty<string>();
            var set = new HashSet<string>(granted, StringComparer.OrdinalIgnoreCase);
            return GrantableKeys().ToDictionary(c => c.Key, c => set.Contains(c.Key), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Stored grants for a role, or the seed defaults when the shop has never saved it.</summary>
        public async Task<(Dictionary<string, bool> Permissions, bool IsDefault)> GetForRoleAsync(int tenantId, string role)
        {
            if (IsAdminRole(role))
                return (GrantableKeys().ToDictionary(c => c.Key, _ => true, StringComparer.OrdinalIgnoreCase), true);

            var rows = await _context.RolePermissions
                .Where(p => p.TenantId == tenantId && p.RoleName == role)
                .ToListAsync();

            if (rows.Count == 0)
                return (DefaultsFor(role), true);

            var stored = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows)
            {
                var key = CapabilityCatalog.ResolveKey(row.Key);
                if (key == null) continue;
                stored[key] = stored.TryGetValue(key, out var existing) ? existing || row.Allowed : row.Allowed;
            }

            // Keys added to the catalog after the shop last saved default to their seed value.
            var defaults = DefaultsFor(role);
            var map = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (var def in GrantableKeys())
                map[def.Key] = stored.TryGetValue(def.Key, out var allowed) ? allowed : defaults[def.Key];

            return (map, false);
        }

        public async Task ReplaceForRoleAsync(int tenantId, string role, IEnumerable<(string Key, bool Allowed)> items)
        {
            if (IsAdminRole(role))
                throw new InvalidOperationException("The Admin role always has full access and cannot be edited.");

            var grantable = GrantableKeys().Select(c => c.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var incoming = items
                .Select(i => (Key: CapabilityCatalog.ResolveKey(i.Key), i.Allowed))
                .Where(i => i.Key != null && grantable.Contains(i.Key!))
                .GroupBy(i => i.Key!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Last().Allowed, StringComparer.OrdinalIgnoreCase);

            var existing = await _context.RolePermissions
                .Where(p => p.TenantId == tenantId && p.RoleName == role)
                .ToListAsync();
            _context.RolePermissions.RemoveRange(existing);

            foreach (var key in grantable)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    TenantId = tenantId,
                    RoleName = role,
                    Key = key,
                    Allowed = incoming.TryGetValue(key, out var allowed) && allowed
                });
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>Drops stored rows so the role falls back to its seed defaults.</summary>
        public async Task ResetRoleAsync(int tenantId, string role)
        {
            if (IsAdminRole(role))
                throw new InvalidOperationException("The Admin role always has full access and cannot be edited.");

            var existing = await _context.RolePermissions
                .Where(p => p.TenantId == tenantId && p.RoleName == role)
                .ToListAsync();
            _context.RolePermissions.RemoveRange(existing);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// What the caller may use: the union of their roles, intersected with the shop's effective
        /// capabilities so a grant can never promise access the tenant has switched off. Modules and
        /// features stay true here — they are gated by capabilities, not by role.
        /// </summary>
        public async Task<Dictionary<string, bool>> GetAllowedMapAsync(int tenantId, IEnumerable<string> roles)
        {
            var roleList = roles.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
            var map = CapabilityCatalog.All.ToDictionary(
                c => c.Key,
                c => c.Category is CapabilityCatalog.CategoryModule or CapabilityCatalog.CategoryFeature,
                StringComparer.OrdinalIgnoreCase);

            if (roleList.Any(IsAdminRole))
            {
                foreach (var key in map.Keys.ToList()) map[key] = true;
                return map;
            }

            foreach (var role in roleList)
            {
                var (permissions, _) = await GetForRoleAsync(tenantId, role);
                foreach (var (key, allowed) in permissions)
                    if (allowed) map[key] = true;
            }

            var enabled = await _caps.GetEnabledMapAsync(tenantId);
            foreach (var key in map.Keys.ToList())
                if (enabled.TryGetValue(key, out var on) && !on)
                    map[key] = false;

            return map;
        }
    }
}
