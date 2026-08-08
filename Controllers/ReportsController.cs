using Inventory.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly ReportRepository _repo;
        private readonly TenantCapabilityService _caps;

        private static readonly Dictionary<string, string> ReportCapabilities = new(StringComparer.OrdinalIgnoreCase)
        {
            ["stock-summary"] = "report.stock-summary",
            ["low-stock"] = "report.low-stock",
            ["stock-ledger"] = "report.stock-ledger",
            ["purchase-history"] = "report.purchase-history",
            ["sales-history"] = "report.sales-history",
            ["vat-sales-register"] = "report.vat-sales-register",
            ["vat-purchase-register"] = "report.vat-purchase-register",
            ["fiscal-year-stock"] = "report.fiscal-year-stock",
            ["expiry-soon"] = "report.expiry-soon",
        };

        public ReportsController(ReportRepository repo, TenantCapabilityService caps)
        {
            _repo = repo;
            _caps = caps;
        }

        private async Task<IActionResult?> GuardAsync(string reportId)
        {
            if (!ReportCapabilities.TryGetValue(reportId, out var key))
                return null;
            if (!await _caps.IsEnabledAsync(key))
                return BadRequest(new { message = $"Report '{reportId}' is disabled for your business profile." });
            return null;
        }

        [HttpGet("catalog")]
        public async Task<IActionResult> GetCatalog()
        {
            var map = await _caps.GetEnabledMapAsync();
            var items = ReportCapabilities
                .Where(kv => map.TryGetValue(kv.Value, out var on) && on)
                .Select(kv => kv.Key)
                .ToList();
            return Ok(items);
        }

        [HttpGet("stock-summary")]
        public async Task<IActionResult> GetStockSummary()
        {
            if (await GuardAsync("stock-summary") is { } denied) return denied;
            return Ok(await _repo.GetStockSummaryAsync());
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock()
        {
            if (await GuardAsync("low-stock") is { } denied) return denied;
            return Ok(await _repo.GetLowStockReportAsync());
        }

        [HttpGet("sales-history")]
        public async Task<IActionResult> GetSalesHistory()
        {
            if (await GuardAsync("sales-history") is { } denied) return denied;
            return Ok(await _repo.GetSalesHistoryAsync());
        }

        [HttpGet("purchase-history")]
        public async Task<IActionResult> GetPurchaseHistory()
        {
            if (await GuardAsync("purchase-history") is { } denied) return denied;
            return Ok(await _repo.GetPurchaseHistoryAsync());
        }

        [HttpGet("stock-ledger")]
        public async Task<IActionResult> GetStockLedger()
        {
            if (await GuardAsync("stock-ledger") is { } denied) return denied;
            return Ok(await _repo.GetStockLedgerAsync());
        }

        [HttpGet("vat-sales-register")]
        public async Task<IActionResult> GetVatSalesRegister()
        {
            if (await GuardAsync("vat-sales-register") is { } denied) return denied;
            return Ok(await _repo.GetVatSalesRegisterAsync());
        }

        [HttpGet("vat-purchase-register")]
        public async Task<IActionResult> GetVatPurchaseRegister()
        {
            if (await GuardAsync("vat-purchase-register") is { } denied) return denied;
            return Ok(await _repo.GetVatPurchaseRegisterAsync());
        }

        [HttpGet("fiscal-year-stock")]
        public async Task<IActionResult> GetFiscalYearStock()
        {
            if (await GuardAsync("fiscal-year-stock") is { } denied) return denied;
            return Ok(await _repo.GetFiscalYearStockReportAsync());
        }

        [HttpGet("expiry-soon")]
        public async Task<IActionResult> GetExpirySoon()
        {
            if (await GuardAsync("expiry-soon") is { } denied) return denied;
            return Ok(await _repo.GetExpirySoonReportAsync());
        }
    }
}
