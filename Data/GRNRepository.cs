using Inventory.Models;
using Inventory.Services;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Data
{
    public class GRNRepository
    {
        private readonly ApplicationDbContext _context;

        public GRNRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<GRN>> GetAllAsync()
        {
            return await _context.GRNs.AsNoTracking().ToListAsync();
        }

        public async Task<GRN?> GetByIdAsync(int id)
        {
            return await _context.GRNs.AsNoTracking().FirstOrDefaultAsync(e => e.GRNID == id);
        }

        public async Task<int> CreateAsync(GRN grn)
        {
            grn.ReceivedDate = grn.ReceivedDate.ToUniversalTime();
            
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.GRNs.Add(grn);
                    await _context.SaveChangesAsync();

                    // Update Inventory
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.ProductID == grn.ProductID && i.LocationID == grn.LocationID);

                    // Damaged qty must not increase sellable stock
                    int sellableQty = Math.Max(0, grn.ReceivedQuantity - grn.DamagedQuantity);

                    if (inventory == null)
                    {
                        inventory = new Inventory.Models.Inventory
                        {
                            ProductID = grn.ProductID,
                            LocationID = grn.LocationID,
                            QuantityOnHand = sellableQty,
                            AvailableQuantity = sellableQty,
                            LastUpdated = DateTime.UtcNow,
                            ExpiryDate = grn.ExpiryDate
                        };
                        _context.Inventories.Add(inventory);
                    }
                    else
                    {
                        inventory.QuantityOnHand += sellableQty;
                        inventory.AvailableQuantity += sellableQty;
                        inventory.LastUpdated = DateTime.UtcNow;
                        if (grn.ExpiryDate.HasValue) inventory.ExpiryDate = grn.ExpiryDate;
                        _context.Inventories.Update(inventory);
                    }

                    // 4. Update Product Cost Price (Landing Cost Calculation)
                    var product = await _context.Products.FindAsync(grn.ProductID);
                    if (product != null && grn.ReceivedQuantity > 0)
                    {
                        var poDetail = await _context.PurchaseOrderDetails
                            .FirstOrDefaultAsync(d => d.PurchaseOrderID == grn.PurchaseOrderID && d.ProductID == grn.ProductID);
                        
                        decimal basePrice = poDetail?.UnitPrice ?? product.CostPrice;
                        decimal landingExtraPerUnit = grn.OtherExpenses / grn.ReceivedQuantity;
                        product.CostPrice = basePrice + landingExtraPerUnit;
                        _context.Products.Update(product);
                    }

                    var movement = new StockMovement
                    {
                        ProductID = grn.ProductID,
                        MovementType = "GRN",
                        QuantityChange = sellableQty,
                        MovementDate = DateTime.UtcNow,
                        Reference = $"GRN-{grn.GRNID} (PO-{grn.PurchaseOrderID})"
                    };
                    _context.StockMovements.Add(movement);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    return grn.GRNID;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task UpdateAsync(GRN grn)
        {
            DocumentLock.EnsurePostedImmutable("GRN");
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            DocumentLock.EnsurePostedImmutable("GRN");
            await Task.CompletedTask;
        }
    }
}
