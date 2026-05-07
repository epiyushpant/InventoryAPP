using Inventory.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            return await _context.Sales.AsNoTracking().ToListAsync();
        }

        public async Task<Sale?> GetByIdAsync(int id)
        {
            return await _context.Sales.AsNoTracking().FirstOrDefaultAsync(e => e.SaleID == id);
        }

        public async Task<int> CreateAsync(Sale sale)
        {
            sale.SaleDate = sale.SaleDate.ToUniversalTime();
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
            return sale.SaleID;
        }

        public async Task UpdateAsync(Sale sale)
        {
            sale.SaleDate = sale.SaleDate.ToUniversalTime();
            _context.Sales.Update(sale);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Sales.FindAsync(id);
            if (entity != null)
            {
                _context.Sales.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}