using Inventory.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Data
{
    public class SaleRepository
    {
        private readonly ApplicationDbContext _context;

        public SaleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Sale>> GetAllAsync()
        {
            var sales = await _context.Sales.AsNoTracking()
                .Include(s => s.SaleDetails)
                .ToListAsync();
            
            foreach (var sale in sales)
            {
                sale.TotalAmount = await CalculateTotalAmountAsync(sale.SaleID);
            }
            return sales;
        }

        public async Task<Sale?> GetByIdAsync(int id)
        {
            var sale = await _context.Sales.AsNoTracking()
                .Include(s => s.SaleDetails)
                .FirstOrDefaultAsync(e => e.SaleID == id);
            
            if (sale != null)
            {
                sale.TotalAmount = await CalculateTotalAmountAsync(sale.SaleID);
            }
            return sale;
        }

        private async Task<decimal> CalculateTotalAmountAsync(int saleId)
        {
            var total = await _context.SaleDetails
                .Where(sd => sd.SaleID == saleId)
                .SumAsync(sd => (decimal?)sd.OrderedQuantity * sd.UnitPrice - sd.Discount);
            return total ?? 0m;
        }

        public async Task<int> CreateAsync(Sale sale)
        {
            sale.SaleDate = sale.SaleDate.ToUniversalTime();

            // Calculate totals if details are provided
            if (sale.SaleDetails != null && sale.SaleDetails.Any())
            {
                decimal total = 0;
                foreach (var detail in sale.SaleDetails)
                {
                    detail.LineTotal = (detail.OrderedQuantity * detail.UnitPrice) - detail.Discount;
                    total += detail.LineTotal;
                }
                sale.TotalAmount = total;
            }

            // Credit Limit Enforcement
            if (sale.CustomerID > 0)
            {
                var customer = await _context.Customers.FindAsync(sale.CustomerID);
                if (customer != null && customer.CreditLimit > 0)
                {
                    var currentOutstanding = await (from i in _context.SalesInvoices
                                                   join s in _context.Sales on i.SaleID equals s.SaleID
                                                   where s.CustomerID == sale.CustomerID && i.Status != "Paid"
                                                   select i.GrandTotal).SumAsync();

                    if (currentOutstanding + sale.TotalAmount > customer.CreditLimit)
                    {
                        throw new System.InvalidOperationException($"Credit Limit Exceeded! Customer '{customer.FullName}' has a limit of Rs. {customer.CreditLimit}. Current Udharo: Rs. {currentOutstanding}. This sale of Rs. {sale.TotalAmount} will exceed the limit.");
                    }
                }
            }

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
            return sale.SaleID;
        }

        public async Task UpdateAsync(Sale sale)
        {
            // Get the existing sale to check for status change
            var existingSale = await _context.Sales.AsNoTracking().FirstOrDefaultAsync(s => s.SaleID == sale.SaleID);
            bool statusChangedToCompleted = existingSale != null && 
                                           existingSale.Status != "Completed" && 
                                           sale.Status == "Completed";

            sale.SaleDate = sale.SaleDate.ToUniversalTime();

            // Sync Details if provided
            if (sale.SaleDetails != null)
            {
                var existingDetails = await _context.SaleDetails.Where(d => d.SaleID == sale.SaleID).ToListAsync();
                _context.SaleDetails.RemoveRange(existingDetails);
                
                decimal total = 0;
                foreach (var detail in sale.SaleDetails)
                {
                    detail.SaleID = sale.SaleID;
                    detail.SaleDetailID = 0; // Ensure they are treated as new
                    detail.LineTotal = (detail.OrderedQuantity * detail.UnitPrice) - detail.Discount;
                    total += detail.LineTotal;
                    _context.SaleDetails.Add(detail);
                }
                sale.TotalAmount = total;
            }

            _context.Sales.Update(sale);
            await _context.SaveChangesAsync();

            // If status changed to Completed, validate stock and update inventory
            if (statusChangedToCompleted)
            {
                var details = await _context.SaleDetails
                    .Where(d => d.SaleID == sale.SaleID)
                    .ToListAsync();

                // 1. Validation Step: Check if enough stock exists for all items
                foreach (var detail in details)
                {
                    if (detail.ProductID != 0)
                    {
                        var inventory = await _context.Inventories
                            .FirstOrDefaultAsync(i => i.ProductID == detail.ProductID && i.LocationID == (sale.LocationID ?? 1));
                        
                        int currentStock = inventory?.QuantityOnHand ?? 0;
                        if (currentStock < detail.OrderedQuantity)
                        {
                            var product = await _context.Products.FindAsync(detail.ProductID);
                            throw new System.InvalidOperationException($"Insufficient stock for product '{product?.ProductName ?? "Unknown"}'. Available: {currentStock}, Required: {detail.OrderedQuantity}");
                        }
                    }
                }

                // 2. Execution Step: All items have enough stock, proceed with update
                foreach (var detail in details)
                {
                    if (detail.ProductID != 0)
                    {
                        var inventory = await _context.Inventories
                            .FirstOrDefaultAsync(i => i.ProductID == detail.ProductID && i.LocationID == (sale.LocationID ?? 1));

                        // Inventory is guaranteed to exist and have enough stock because of the validation step above
                        inventory!.QuantityOnHand -= detail.OrderedQuantity;
                        inventory.AvailableQuantity -= detail.OrderedQuantity;
                        inventory.LastUpdated = DateTime.UtcNow;
                        _context.Inventories.Update(inventory);

                        // Add stock movement
                        var movement = new StockMovement
                        {
                            ProductID = detail.ProductID,
                            MovementType = "Sale",
                            QuantityChange = -detail.OrderedQuantity,
                            MovementDate = DateTime.UtcNow,
                            Reference = $"SALE-{sale.SaleID}"
                        };
                        _context.StockMovements.Add(movement);
                    }
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Sales.FindAsync(id);
            if (entity != null)
            {
                if (entity.Status == "Completed")
                {
                    throw new System.InvalidOperationException("Cannot delete a completed sale record. Finalized transactions must be kept for auditing.");
                }
                _context.Sales.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}