using Inventory.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Data
{
    public class InventoryRepository
    {
        private readonly ApplicationDbContext _context;

        public InventoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Inventory.Models.Inventory>> GetAllAsync()
        {
            return await _context.Inventories.AsNoTracking().ToListAsync();
        }

        public async Task<Inventory.Models.Inventory?> GetByIdAsync(int id)
        {
            return await _context.Inventories.AsNoTracking().FirstOrDefaultAsync(e => e.InventoryID == id);
        }

        public async Task<int> CreateAsync(Inventory.Models.Inventory inventory)
        {
            inventory.LastUpdated = inventory.LastUpdated?.ToUniversalTime();
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            return inventory.InventoryID;
        }

        public async Task UpdateAsync(Inventory.Models.Inventory inventory)
        {
            inventory.LastUpdated = inventory.LastUpdated?.ToUniversalTime();
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Inventories.FindAsync(id);
            if (entity != null)
            {
                _context.Inventories.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}