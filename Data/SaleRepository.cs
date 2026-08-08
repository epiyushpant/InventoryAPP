using Inventory.Models;
using Inventory.Services;
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

            if (string.IsNullOrWhiteSpace(sale.Status))
                sale.Status = "Draft";

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
                        throw new InvalidOperationException(
                            $"Credit Limit Exceeded! Customer '{customer.FullName}' has a limit of Rs. {customer.CreditLimit}. Current Udharo: Rs. {currentOutstanding}. This sale of Rs. {sale.TotalAmount} will exceed the limit.");
                    }
                }
            }

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
            return sale.SaleID;
        }

        public async Task UpdateAsync(Sale sale)
        {
            var existingSale = await _context.Sales.AsNoTracking().FirstOrDefaultAsync(s => s.SaleID == sale.SaleID)
                ?? throw new InvalidOperationException("Sale not found.");

            DocumentLock.EnsureEditable("Sale", existingSale.Status);

            bool completing =
                !string.Equals(existingSale.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                && string.Equals(sale.Status, "Completed", StringComparison.OrdinalIgnoreCase);

            sale.SaleDate = sale.SaleDate.ToUniversalTime();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (sale.SaleDetails != null)
                {
                    var existingDetails = await _context.SaleDetails.Where(d => d.SaleID == sale.SaleID).ToListAsync();
                    _context.SaleDetails.RemoveRange(existingDetails);

                    decimal total = 0;
                    foreach (var detail in sale.SaleDetails)
                    {
                        detail.SaleID = sale.SaleID;
                        detail.SaleDetailID = 0;
                        detail.LineTotal = (detail.OrderedQuantity * detail.UnitPrice) - detail.Discount;
                        total += detail.LineTotal;
                        _context.SaleDetails.Add(detail);
                    }
                    sale.TotalAmount = total;
                }

                if (completing)
                {
                    _context.Sales.Update(sale);
                    await _context.SaveChangesAsync();

                    var details = await _context.SaleDetails.Where(d => d.SaleID == sale.SaleID).ToListAsync();
                    int locationId = sale.LocationID ?? existingSale.LocationID ?? 1;

                    foreach (var detail in details)
                    {
                        if (detail.ProductID == 0) continue;

                        var inventory = await _context.Inventories
                            .FirstOrDefaultAsync(i => i.ProductID == detail.ProductID && i.LocationID == locationId);

                        int available = inventory?.AvailableQuantity ?? 0;
                        if (available < detail.OrderedQuantity)
                        {
                            var product = await _context.Products.FindAsync(detail.ProductID);
                            throw new InvalidOperationException(
                                $"Insufficient stock for '{product?.ProductName ?? "Unknown"}'. Available: {available}, Required: {detail.OrderedQuantity}");
                        }
                    }

                    foreach (var detail in details)
                    {
                        if (detail.ProductID == 0) continue;

                        var inventory = await _context.Inventories
                            .FirstOrDefaultAsync(i => i.ProductID == detail.ProductID && i.LocationID == locationId);

                        inventory!.QuantityOnHand -= detail.OrderedQuantity;
                        inventory.AvailableQuantity -= detail.OrderedQuantity;
                        inventory.LastUpdated = DateTime.UtcNow;
                        _context.Inventories.Update(inventory);

                        _context.StockMovements.Add(new StockMovement
                        {
                            ProductID = detail.ProductID,
                            MovementType = "Sale",
                            QuantityChange = -detail.OrderedQuantity,
                            MovementDate = DateTime.UtcNow,
                            Reference = $"SALE-{sale.SaleID}"
                        });
                    }

                    await _context.SaveChangesAsync();
                }
                else
                {
                    _context.Sales.Update(sale);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Sales.FindAsync(id);
            if (entity != null)
            {
                DocumentLock.EnsureDeletable("Sale", entity.Status);
                _context.Sales.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
