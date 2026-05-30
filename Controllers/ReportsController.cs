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

        public ReportsController(ReportRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("stock-summary")]
        public async Task<IActionResult> GetStockSummary()
        {
            return Ok(await _repo.GetStockSummaryAsync());
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock()
        {
            return Ok(await _repo.GetLowStockReportAsync());
        }

        [HttpGet("sales-history")]
        public async Task<IActionResult> GetSalesHistory()
        {
            return Ok(await _repo.GetSalesHistoryAsync());
        }

        [HttpGet("purchase-history")]
        public async Task<IActionResult> GetPurchaseHistory()
        {
            return Ok(await _repo.GetPurchaseHistoryAsync());
        }

        [HttpGet("stock-ledger")]
        public async Task<IActionResult> GetStockLedger()
        {
            return Ok(await _repo.GetStockLedgerAsync());
        }

        [HttpGet("vat-sales-register")]
        public async Task<IActionResult> GetVatSalesRegister()
        {
            return Ok(await _repo.GetVatSalesRegisterAsync());
        }

        [HttpGet("vat-purchase-register")]
        public async Task<IActionResult> GetVatPurchaseRegister()
        {
            return Ok(await _repo.GetVatPurchaseRegisterAsync());
        }

        [HttpGet("fiscal-year-stock")]
        public async Task<IActionResult> GetFiscalYearStock()
        {
            return Ok(await _repo.GetFiscalYearStockReportAsync());
        }
    }
}
