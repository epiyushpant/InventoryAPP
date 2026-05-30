using Inventory.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Data
{
    public class StockAdjustmentRepository
    {
        private readonly ApplicationDbContext _context;

        public StockAdjustmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<StockAdjustment>> GetAllAsync()
        {
            return await _context.StockAdjustments.AsNoTracking().ToListAsync();
        }

        public async Task<int> CreateAsync(StockAdjustment adj)
        {
            adj.AdjustmentDate = adj.AdjustmentDate.ToUniversalTime();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.StockAdjustments.Add(adj);
                    await _context.SaveChangesAsync();

                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.ProductID == adj.ProductID && i.LocationID == adj.LocationID);

                    int qtyChange = adj.AdjustmentType == "Add" ? adj.Quantity : -adj.Quantity;

                    if (inventory == null)
                    {
                        if (qtyChange < 0) throw new InvalidOperationException("Cannot deduct stock from non-existent inventory.");
                        
                        inventory = new Inventory.Models.Inventory
                        {
                            ProductID = adj.ProductID,
                            LocationID = adj.LocationID,
                            QuantityOnHand = qtyChange,
                            AvailableQuantity = qtyChange,
                            LastUpdated = DateTime.UtcNow
                        };
                        _context.Inventories.Add(inventory);
                    }
                    else
                    {
                        inventory.QuantityOnHand += qtyChange;
                        inventory.AvailableQuantity += qtyChange;
                        inventory.LastUpdated = DateTime.UtcNow;
                        _context.Inventories.Update(inventory);
                    }

                    // Add Stock Movement
                    var movement = new StockMovement
                    {
                        ProductID = adj.ProductID,
                        MovementType = "Adjustment",
                        QuantityChange = qtyChange,
                        MovementDate = DateTime.UtcNow,
                        Reference = $"ADJ-{adj.AdjustmentID}: {adj.Reason}"
                    };
                    _context.StockMovements.Add(movement);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return adj.AdjustmentID;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}
