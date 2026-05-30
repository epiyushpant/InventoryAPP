using Inventory.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Data
{
    public class DeliveryNoteRepository
    {
        private readonly ApplicationDbContext _context;

        public DeliveryNoteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DeliveryNote>> GetAllAsync()
        {
            return await _context.DeliveryNotes.AsNoTracking().ToListAsync();
        }

        public async Task<int> CreateAsync(DeliveryNote note)
        {
            note.ShipmentDate = note.ShipmentDate.ToUniversalTime();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.DeliveryNotes.Add(note);
                    await _context.SaveChangesAsync();

                    // Find the sale to get the source location (warehouse)
                    var sale = await _context.Sales.FindAsync(note.SaleID);
                    int locationId = sale?.LocationID ?? 1;

                    // Update Inventory
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.ProductID == note.ProductID && i.LocationID == locationId);

                    if (inventory == null || inventory.QuantityOnHand < note.ShippedQuantity)
                    {
                        throw new InvalidOperationException("Insufficient stock in the warehouse to fulfill this delivery.");
                    }

                    inventory.QuantityOnHand -= note.ShippedQuantity;
                    inventory.AvailableQuantity -= note.ShippedQuantity;
                    inventory.LastUpdated = DateTime.UtcNow;
                    _context.Inventories.Update(inventory);

                    // Add Stock Movement
                    var movement = new StockMovement
                    {
                        ProductID = note.ProductID,
                        MovementType = "Shipment",
                        QuantityChange = -note.ShippedQuantity,
                        MovementDate = DateTime.UtcNow,
                        Reference = $"DN-{note.DeliveryID} (SO-{note.SaleID})"
                    };
                    _context.StockMovements.Add(movement);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return note.DeliveryID;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.DeliveryNotes.FindAsync(id);
            if (entity != null)
            {
                _context.DeliveryNotes.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
