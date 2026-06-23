using Inventory.Models;
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

                    if (inventory == null)
                    {
                        inventory = new Inventory.Models.Inventory
                        {
                            ProductID = grn.ProductID,
                            LocationID = grn.LocationID,
                            QuantityOnHand = grn.ReceivedQuantity,
                            AvailableQuantity = grn.ReceivedQuantity,
                            LastUpdated = DateTime.UtcNow,
                            ExpiryDate = grn.ExpiryDate // Setting expiry from GRN
                        };
                        _context.Inventories.Add(inventory);
                    }
                    else
                    {
                        inventory.QuantityOnHand += grn.ReceivedQuantity;
                        inventory.AvailableQuantity += grn.ReceivedQuantity;
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

                    // Add Stock Movement
                    var movement = new StockMovement
                    {
                        ProductID = grn.ProductID,
                        MovementType = "GRN",
                        QuantityChange = grn.ReceivedQuantity,
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
            grn.ReceivedDate = grn.ReceivedDate.ToUniversalTime();
            _context.GRNs.Update(grn);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.GRNs.FindAsync(id);
            if (entity != null)
            {
                _context.GRNs.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
