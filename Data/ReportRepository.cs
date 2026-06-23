using Inventory.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Data
{
    public class ReportRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetStockSummaryAsync()
        {
            var inventories = await _context.Inventories.AsNoTracking().ToListAsync();
            var products = await _context.Products.AsNoTracking().ToListAsync();
            var locations = await _context.Locations.AsNoTracking().ToListAsync();

            return inventories.Select(i => new
            {
                Product = products.FirstOrDefault(p => p.ProductID == i.ProductID)?.ProductName ?? "Unknown",
                SKU = products.FirstOrDefault(p => p.ProductID == i.ProductID)?.SKU ?? "",
                Warehouse = locations.FirstOrDefault(l => l.LocationID == i.LocationID)?.WarehouseName ?? "Unknown",
                City = locations.FirstOrDefault(l => l.LocationID == i.LocationID)?.City ?? "",
                Quantity = i.QuantityOnHand,
                Available = i.AvailableQuantity,
                LastUpdated = i.LastUpdated
            }).ToList();
        }

        public async Task<object> GetLowStockReportAsync()
        {
            var inventories = await _context.Inventories.AsNoTracking().ToListAsync();
            var products = await _context.Products.AsNoTracking().ToListAsync();
            var locations = await _context.Locations.AsNoTracking().ToListAsync();

            return inventories
                .Select(i => new
                {
                    inv = i,
                    product = products.FirstOrDefault(p => p.ProductID == i.ProductID),
                    location = locations.FirstOrDefault(l => l.LocationID == i.LocationID)
                })
                .Where(x => x.inv.AvailableQuantity <= (x.product?.ReorderLevel ?? 5))
                .Select(x => new
                {
                    Product = x.product?.ProductName ?? "Unknown",
                    SKU = x.product?.SKU ?? "",
                    Warehouse = x.location?.WarehouseName ?? "Unknown",
                    Available = x.inv.AvailableQuantity,
                    ReorderLevel = x.product?.ReorderLevel ?? 5,
                    Status = "⚠️ Low Stock"
                }).ToList();
        }

        public async Task<object> GetSalesHistoryAsync()
        {
            var invoices = await _context.SalesInvoices.AsNoTracking().ToListAsync();
            var sales = await _context.Sales.AsNoTracking().ToListAsync();
            var customers = await _context.Customers.AsNoTracking().ToListAsync();

            return invoices
                .OrderByDescending(i => i.InvoiceDate)
                .Select(i =>
                {
                    var sale = sales.FirstOrDefault(s => s.SaleID == i.SaleID);
                    var customer = customers.FirstOrDefault(c => c.CustomerID == sale?.CustomerID);
                    return new
                    {
                        InvoiceID = $"INV-{i.InvoiceID}",
                        Date = i.InvoiceDate,
                        SaleRef = $"SO-{i.SaleID}",
                        Customer = customer?.FullName ?? $"Customer #{sale?.CustomerID}",
                        Tax = i.TaxAmount,
                        Total = i.GrandTotal,
                        Status = i.Status
                    };
                }).ToList();
        }

        public async Task<object> GetPurchaseHistoryAsync()
        {
            var grns = await _context.GRNs.AsNoTracking().ToListAsync();
            var pos = await _context.PurchaseOrders.AsNoTracking().ToListAsync();
            var suppliers = await _context.Suppliers.AsNoTracking().ToListAsync();
            var products = await _context.Products.AsNoTracking().ToListAsync();
            var locations = await _context.Locations.AsNoTracking().ToListAsync();

            return grns
                .OrderByDescending(g => g.ReceivedDate)
                .Select(g =>
                {
                    var po = pos.FirstOrDefault(p => p.PurchaseOrderID == g.PurchaseOrderID);
                    var supplier = suppliers.FirstOrDefault(s => s.SupplierID == po?.SupplierID);
                    return new
                    {
                        GRNID = $"GRN-{g.GRNID}",
                        Date = g.ReceivedDate,
                        PORef = $"PO-{g.PurchaseOrderID}",
                        Supplier = supplier?.SupplierName ?? $"Supplier #{po?.SupplierID}",
                        Product = products.FirstOrDefault(p => p.ProductID == g.ProductID)?.ProductName ?? $"Product #{g.ProductID}",
                        Qty = g.ReceivedQuantity,
                        Damaged = g.DamagedQuantity,
                        Warehouse = locations.FirstOrDefault(l => l.LocationID == g.LocationID)?.WarehouseName ?? "Unknown"
                    };
                }).ToList();
        }

        public async Task<object> GetStockLedgerAsync()
        {
            var movements = await _context.StockMovements.AsNoTracking()
                .OrderByDescending(m => m.MovementDate)
                .ToListAsync();
            var products = await _context.Products.AsNoTracking().ToListAsync();

            return movements.Select(m => new
            {
                ID = m.MovementID,
                Date = m.MovementDate,
                Product = products.FirstOrDefault(p => p.ProductID == m.ProductID)?.ProductName ?? $"Product #{m.ProductID}",
                Type = m.MovementType,
                Change = m.QuantityChange > 0 ? $"+{m.QuantityChange}" : $"{m.QuantityChange}",
                Ref = m.Reference ?? "-"
            }).ToList();
        }

        public async Task<object> GetVatSalesRegisterAsync()
        {
            var invoices = await _context.SalesInvoices.AsNoTracking().ToListAsync();
            var sales = await _context.Sales.AsNoTracking().ToListAsync();
            var customers = await _context.Customers.AsNoTracking().ToListAsync();

            return invoices.Select(i => {
                var sale = sales.FirstOrDefault(s => s.SaleID == i.SaleID);
                var customer = customers.FirstOrDefault(c => c.CustomerID == sale?.CustomerID);
                return new {
                    Date = i.InvoiceDate,
                    InvoiceNo = $"INV-{i.InvoiceID}",
                    CustomerName = customer?.FullName ?? "Cash Sale",
                    PAN = customer?.PAN ?? "-",
                    TaxableAmount = i.TaxableAmount,
                    NonTaxableAmount = i.NonTaxableAmount,
                    VAT = i.TaxAmount,
                    Total = i.GrandTotal
                };
            }).ToList();
        }

        public async Task<object> GetVatPurchaseRegisterAsync()
        {
            var grns = await _context.GRNs.AsNoTracking().ToListAsync();
            var pos = await _context.PurchaseOrders.AsNoTracking().ToListAsync();
            var suppliers = await _context.Suppliers.AsNoTracking().ToListAsync();
            var products = await _context.Products.AsNoTracking().ToListAsync();

            // Aggregating by GRN as we don't have a PurchaseInvoice model yet
            return grns.Select(g => {
                var po = pos.FirstOrDefault(p => p.PurchaseOrderID == g.PurchaseOrderID);
                var supplier = suppliers.FirstOrDefault(s => s.SupplierID == po?.SupplierID);
                var product = products.FirstOrDefault(p => p.ProductID == g.ProductID);
                
                decimal cost = product?.CostPrice ?? 0;
                decimal taxable = (g.ReceivedQuantity - g.DamagedQuantity) * cost;
                
                return new {
                    Date = g.ReceivedDate,
                    ReferenceNo = $"GRN-{g.GRNID}",
                    SupplierName = supplier?.SupplierName ?? "Unknown",
                    PAN = supplier?.TaxVatNumber ?? "-",
                    TaxableAmount = taxable,
                    VAT = taxable * 0.13m, // Assuming 13% for the report
                    Total = taxable * 1.13m
                };
            }).ToList();
        }

        public async Task<object> GetFiscalYearStockReportAsync()
        {
            var products = await _context.Products.AsNoTracking().ToListAsync();
            var inventories = await _context.Inventories.AsNoTracking().ToListAsync();
            
            return products.Select(p => {
                var inv = inventories.Where(i => i.ProductID == p.ProductID).ToList();
                var stock = inv.Sum(i => i.QuantityOnHand);
                return new {
                    Code = p.SKU,
                    Name = p.ProductName,
                    Unit = p.UnitOfMeasure,
                    ClosingStock = stock,
                    Rate = p.CostPrice,
                    TotalValue = stock * p.CostPrice
                };
            }).ToList();
        }
    }
}
