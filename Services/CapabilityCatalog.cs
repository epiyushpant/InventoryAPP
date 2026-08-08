namespace Inventory.Services
{
    /// <summary>
    /// One catalog entry. <c>Parent</c> places the key in the admin tree (page/report to module).
    /// <c>Forms</c> lists the pages a field renders on — a field is live while any of them is live.
    /// <c>Requires</c> holds extra gates, used for cross-cutting features and the reports page.
    /// </summary>
    public record CapabilityDef(
        string Key,
        string Label,
        string Category,
        string? Parent = null,
        string[]? Forms = null,
        string[]? Requires = null);

    /// <summary>Static catalog + business presets. Never put stock/VAT/PAN locks here as disableable.</summary>
    public static class CapabilityCatalog
    {
        public const string CategoryModule = "module";
        public const string CategoryFeature = "feature";
        public const string CategoryForm = "form";
        public const string CategoryField = "field";
        public const string CategoryReport = "report";

        public const string PresetKirana = "Kirana";
        public const string PresetPharmacy = "Pharmacy";
        public const string PresetWholesale = "Wholesale";
        public const string PresetFull = "Full";
        public const string PresetCustom = "Custom";

        public static readonly IReadOnlyList<CapabilityDef> All = new List<CapabilityDef>
        {
            // Modules — each owns pages, reports and fields
            new("module.masters", "Master data", CategoryModule),
            new("module.purchasing", "Purchasing", CategoryModule),
            new("module.sales", "Sales", CategoryModule),
            new("module.stock", "Inventory & stock", CategoryModule),
            new("module.reports", "Reports", CategoryModule),
            new("module.administration", "Administration", CategoryModule),

            // Cross-cutting features — no screens of their own, used as extra gates
            new("feature.expiry", "Expiry / batch tracking", CategoryFeature),
            new("feature.payments", "Invoice payment status / AR", CategoryFeature),
            new("feature.dashboardKpis", "Dashboard Nepal KPIs", CategoryFeature),

            // Pages — master data
            new("form.categories", "Categories", CategoryForm, "module.masters"),
            new("form.locations", "Warehouses", CategoryForm, "module.masters"),
            new("form.suppliers", "Suppliers", CategoryForm, "module.masters"),
            new("form.customers", "Customers", CategoryForm, "module.masters"),
            new("form.products", "Products", CategoryForm, "module.masters"),
            new("form.unitConversions", "Unit conversions", CategoryForm, "module.masters"),

            // Pages — purchasing
            new("form.purchaseRequisitions", "Purchase requisitions", CategoryForm, "module.purchasing"),
            new("form.purchaseOrders", "Purchase orders", CategoryForm, "module.purchasing"),
            new("form.grns", "Goods received (GRN)", CategoryForm, "module.purchasing"),

            // Pages — sales
            new("form.sales", "Sales orders", CategoryForm, "module.sales"),
            new("form.deliveryNotes", "Delivery notes", CategoryForm, "module.sales"),
            new("form.salesInvoices", "Sales invoices", CategoryForm, "module.sales"),

            // Pages — inventory & stock
            new("form.inventories", "Stock on hand", CategoryForm, "module.stock"),
            new("form.stockMovements", "Stock movements", CategoryForm, "module.stock"),
            new("form.stockAdjustments", "Stock adjustments", CategoryForm, "module.stock"),
            new("form.stockTransfers", "Stock transfers", CategoryForm, "module.stock"),

            // Pages — reports & administration
            new("form.reports", "Reports page", CategoryForm, "module.reports"),
            new("form.users", "Users", CategoryForm, "module.administration"),

            // Fields — placed on the pages they render on
            new("field.product.expiryDate", "Expiry date", CategoryField,
                null, new[] { "form.grns", "form.inventories" }, new[] { "feature.expiry" }),
            new("field.grn.otherExpenses", "Landed cost / other expenses", CategoryField,
                null, new[] { "form.grns" }),
            new("field.invoice.paymentStatus", "Payment status", CategoryField,
                null, new[] { "form.salesInvoices" }, new[] { "feature.payments" }),

            // Reports — owned by the module that produces them, all gated by the reports page
            new("report.stock-summary", "Stock summary", CategoryReport,
                "module.stock", null, new[] { "form.reports" }),
            new("report.low-stock", "Low stock", CategoryReport,
                "module.stock", null, new[] { "form.reports" }),
            new("report.stock-ledger", "Stock ledger", CategoryReport,
                "module.stock", null, new[] { "form.reports" }),
            new("report.fiscal-year-stock", "Fiscal year stock", CategoryReport,
                "module.stock", null, new[] { "form.reports" }),
            new("report.expiry-soon", "Expiry soon", CategoryReport,
                "module.stock", null, new[] { "form.reports", "feature.expiry" }),
            new("report.purchase-history", "Purchase history", CategoryReport,
                "module.purchasing", null, new[] { "form.reports" }),
            new("report.vat-purchase-register", "VAT purchase register", CategoryReport,
                "module.purchasing", null, new[] { "form.reports" }),
            new("report.sales-history", "Sales history", CategoryReport,
                "module.sales", null, new[] { "form.reports" }),
            new("report.vat-sales-register", "VAT sales register", CategoryReport,
                "module.sales", null, new[] { "form.reports" }),
        };

        public static readonly IReadOnlyDictionary<string, CapabilityDef> ByKey =
            All.ToDictionary(c => c.Key, c => c, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Pre-2026-08 keys mapped onto the current catalog. A null value means the key was dropped.
        /// Several old modules collapse onto one new module; the merge is permissive (any on wins)
        /// because the per-page toggles underneath still decide what a shop can reach.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string?> LegacyKeys =
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["module.purchaseRequisition"] = "module.purchasing",
                ["module.purchaseOrder"] = "module.purchasing",
                ["module.grn"] = "module.purchasing",
                ["module.salesOrder"] = "module.sales",
                ["module.deliveryNote"] = "module.sales",
                ["module.salesInvoice"] = "module.sales",
                ["module.users"] = "module.administration",
                ["module.expiry"] = "feature.expiry",
                ["module.payments"] = "feature.payments",
                ["module.dashboardKpis"] = "feature.dashboardKpis",
                // form.unitConversions is the real switch, so the module wrapper is gone
                ["module.unitConversion"] = null,
            };

        /// <summary>Current key for a stored key, or null when it no longer exists.</summary>
        public static string? ResolveKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            if (ByKey.TryGetValue(key, out var def)) return def.Key;
            return LegacyKeys.TryGetValue(key, out var mapped) ? mapped : null;
        }

        /// <summary>
        /// Applies the cascade to raw stored ticks: a key is live only when its own tick is on and
        /// every gate above it is on. Stored values are never modified — switching a module off
        /// blocks its children rather than unticking them, so the change is reversible.
        /// </summary>
        public static Dictionary<string, bool> ComputeEffective(IReadOnlyDictionary<string, bool> stored)
        {
            var effective = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            var resolving = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            bool Resolve(string key)
            {
                if (effective.TryGetValue(key, out var cached)) return cached;

                if (!ByKey.TryGetValue(key, out var def))
                    return !stored.TryGetValue(key, out var unknown) || unknown;

                if (!resolving.Add(key)) return false;

                var on = stored.TryGetValue(def.Key, out var own) && own;

                if (on && def.Parent != null)
                    on = Resolve(def.Parent);

                if (on && def.Requires != null)
                    on = def.Requires.All(Resolve);

                // A field can live on several pages; it survives while any of them is live.
                if (on && def.Forms is { Length: > 0 })
                    on = def.Forms.Any(Resolve);

                resolving.Remove(key);
                effective[def.Key] = on;
                return on;
            }

            foreach (var def in All) Resolve(def.Key);
            return effective;
        }

        public static IReadOnlyList<string> KeysForPreset(string preset)
        {
            var excluded = preset switch
            {
                PresetKirana => new[]
                {
                    "form.purchaseRequisitions", "form.unitConversions",
                    "feature.expiry", "field.product.expiryDate", "report.expiry-soon"
                },
                PresetPharmacy => new[] { "form.unitConversions" },
                PresetWholesale => new[] { "feature.expiry", "field.product.expiryDate", "report.expiry-soon" },
                _ => Array.Empty<string>() // Full and Custom start from everything on
            };

            var skip = new HashSet<string>(excluded, StringComparer.OrdinalIgnoreCase);
            return All.Select(c => c.Key).Where(k => !skip.Contains(k)).ToList();
        }

        public static bool IsKnown(string key) => ByKey.ContainsKey(key);
    }
}
