using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventory.Models;
using Inventory.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalProducts = await _context.Products.CountAsync();
            var totalSales = await _context.Sales.CountAsync();
            var totalRevenue = await _context.SaleDetails.SumAsync(d => (decimal)d.OrderedQuantity * d.UnitPrice);
            
            var lowStockCount = await (from p in _context.Products
                                       join i in _context.Inventories on p.ProductID equals i.ProductID
                                       where i.QuantityOnHand <= p.ReorderLevel && i.QuantityOnHand > 0
                                       select p).CountAsync();

            var outOfStockCount = await _context.Inventories.CountAsync(i => i.QuantityOnHand <= 0);

            var totalOutstanding = await _context.SalesInvoices
                .Where(i => i.Status != "Paid")
                .SumAsync(i => i.GrandTotal);

            var topProducts = await (from sd in _context.SaleDetails
                                     join p in _context.Products on sd.ProductID equals p.ProductID
                                     group sd by new { p.ProductName, p.SKU } into g
                                     orderby g.Sum(x => x.OrderedQuantity) descending
                                     select new {
                                         Name = g.Key.ProductName,
                                         SKU = g.Key.SKU,
                                         TotalSold = g.Sum(x => x.OrderedQuantity)
                                     }).Take(5).ToListAsync();

            // VAT Reconciliation Logic (Nepal 13%)
            var salesVat = await _context.SalesInvoices.SumAsync(i => i.TaxAmount);
            var purchaseVat = await (from g in _context.GRNs
                                    join p in _context.Products on g.ProductID equals p.ProductID
                                    select (decimal)(g.ReceivedQuantity - g.DamagedQuantity) * p.CostPrice * 0.13m).SumAsync();

            var recentSales = await _context.Sales
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .Select(s => new {
                    s.SaleID,
                    s.SaleDate,
                    s.Status,
                    CustomerName = _context.Customers.Where(c => c.CustomerID == s.CustomerID).Select(c => c.FullName).FirstOrDefault() ?? "Walk-in"
                })
                .ToListAsync();

            // Sales per day for chart (last 7 days)
            var last7Days = Enumerable.Range(0, 7).Select(i => DateTime.UtcNow.Date.AddDays(-i)).Reverse();
            var salesHistory = new List<object>();
            foreach (var date in last7Days)
            {
                var count = await _context.Sales.CountAsync(s => s.SaleDate.Date == date);
                salesHistory.Add(new { date = date.ToString("MMM dd"), count });
            }

            return Ok(new
            {
                TotalProducts = totalProducts,
                TotalSales = totalSales,
                TotalRevenue = totalRevenue,
                LowStockCount = lowStockCount,
                OutOfStockCount = outOfStockCount,
                TotalOutstanding = totalOutstanding,
                TopProducts = topProducts,
                VatSummary = new {
                    SalesVat = salesVat,
                    PurchaseVat = purchaseVat,
                    NetPayable = salesVat - purchaseVat
                },
                RecentSales = recentSales,
                SalesHistory = salesHistory
            });
        }

        [HttpPost("generate-reorders")]
        public async Task<IActionResult> GenerateReorders()
        {
            var lowStockProducts = await (from p in _context.Products
                                          join i in _context.Inventories on p.ProductID equals i.ProductID
                                          where i.QuantityOnHand <= p.ReorderLevel
                                          select p).ToListAsync();

            int count = 0;
            // Group by Supplier to create one PO per supplier
            var groupedBySupplier = lowStockProducts.Where(p => p.SupplierID.HasValue).GroupBy(p => p.SupplierID);

            foreach (var group in groupedBySupplier)
            {
                var po = new PurchaseOrder
                {
                    SupplierID = group.Key ?? 0,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    PurchaseOrderDetails = group.Select(p => new PurchaseOrderDetail
                    {
                        ProductID = p.ProductID,
                        OrderedQuantity = (p.ReorderLevel ?? 0) > 0 ? (p.ReorderLevel ?? 0) * 2 : 10, // Simple heuristic: order twice the reorder level
                        UnitPrice = p.CostPrice // Use current cost price as default
                    }).ToList()
                };
                _context.PurchaseOrders.Add(po);
                count++;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Successfully generated {count} draft purchase orders." });
        }
    }
}
