using Inventory.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Data
{
    public class StockTransferRepository
    {
        private readonly ApplicationDbContext _context;

        public StockTransferRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<StockTransfer>> GetAllAsync()
        {
            return await _context.StockTransfers.AsNoTracking().ToListAsync();
        }

        public async Task<int> CreateAsync(StockTransfer transfer)
        {
            transfer.TransferDate = transfer.TransferDate.ToUniversalTime();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.StockTransfers.Add(transfer);
                    await _context.SaveChangesAsync();

                    if (transfer.Status == "Completed")
                    {
                        await ExecuteTransfer(transfer);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return transfer.TransferID;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task UpdateAsync(StockTransfer transfer)
        {
            var existing = await _context.StockTransfers.AsNoTracking().FirstOrDefaultAsync(t => t.TransferID == transfer.TransferID);
            bool statusChangedToCompleted = existing != null && existing.Status != "Completed" && transfer.Status == "Completed";

            transfer.TransferDate = transfer.TransferDate.ToUniversalTime();
            _context.StockTransfers.Update(transfer);
            await _context.SaveChangesAsync();

            if (statusChangedToCompleted)
            {
                await ExecuteTransfer(transfer);
                await _context.SaveChangesAsync();
            }
        }

        private async Task ExecuteTransfer(StockTransfer transfer)
        {
            // 1. Deduct from Source
            var sourceInv = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductID == transfer.ProductID && i.LocationID == transfer.FromLocationID);
            if (sourceInv == null || sourceInv.QuantityOnHand < transfer.Quantity)
            {
                throw new InvalidOperationException("Insufficient stock in source warehouse.");
            }
            sourceInv.QuantityOnHand -= transfer.Quantity;
            sourceInv.AvailableQuantity -= transfer.Quantity;
            sourceInv.LastUpdated = DateTime.UtcNow;

            // 2. Add to Destination
            var destInv = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductID == transfer.ProductID && i.LocationID == transfer.ToLocationID);
            if (destInv == null)
            {
                destInv = new Inventory.Models.Inventory
                {
                    ProductID = transfer.ProductID,
                    LocationID = transfer.ToLocationID,
                    QuantityOnHand = transfer.Quantity,
                    AvailableQuantity = transfer.Quantity,
                    LastUpdated = DateTime.UtcNow
                };
                _context.Inventories.Add(destInv);
            }
            else
            {
                destInv.QuantityOnHand += transfer.Quantity;
                destInv.AvailableQuantity += transfer.Quantity;
                destInv.LastUpdated = DateTime.UtcNow;
            }

            // 3. Movement Records
            _context.StockMovements.Add(new StockMovement
            {
                ProductID = transfer.ProductID,
                MovementType = "Transfer-Out",
                QuantityChange = -transfer.Quantity,
                MovementDate = DateTime.UtcNow,
                Reference = $"TR-{transfer.TransferID} (To Loc {transfer.ToLocationID})"
            });

            _context.StockMovements.Add(new StockMovement
            {
                ProductID = transfer.ProductID,
                MovementType = "Transfer-In",
                QuantityChange = transfer.Quantity,
                MovementDate = DateTime.UtcNow,
                Reference = $"TR-{transfer.TransferID} (From Loc {transfer.FromLocationID})"
            });
        }
    }
}
