using Inventory.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Data
{
    public class StockMovementRepository
    {
        private readonly ApplicationDbContext _context;

        public StockMovementRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<StockMovement>> GetAllAsync()
        {
            return await _context.StockMovements.AsNoTracking().ToListAsync();
        }

        public async Task<StockMovement?> GetByIdAsync(int id)
        {
            return await _context.StockMovements.AsNoTracking().FirstOrDefaultAsync(e => e.MovementID == id);
        }

        public async Task<int> CreateAsync(StockMovement stockmovement)
        {
            stockmovement.MovementDate = stockmovement.MovementDate?.ToUniversalTime();
            _context.StockMovements.Add(stockmovement);
            await _context.SaveChangesAsync();
            return stockmovement.MovementID;
        }

        public async Task UpdateAsync(StockMovement stockmovement)
        {
            stockmovement.MovementDate = stockmovement.MovementDate?.ToUniversalTime();
            _context.StockMovements.Update(stockmovement);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.StockMovements.FindAsync(id);
            if (entity != null)
            {
                _context.StockMovements.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}