using Inventory.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Data
{
    public class SaleDetailRepository
    {
        private readonly ApplicationDbContext _context;

        public SaleDetailRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SaleDetail>> GetAllAsync()
        {
            return await _context.SaleDetails.AsNoTracking().ToListAsync();
        }

        public async Task<SaleDetail?> GetByIdAsync(int id)
        {
            return await _context.SaleDetails.AsNoTracking().FirstOrDefaultAsync(e => e.SaleDetailID == id);
        }

        public async Task<int> CreateAsync(SaleDetail saledetail)
        {
            _context.SaleDetails.Add(saledetail);
            await _context.SaveChangesAsync();
            return saledetail.SaleDetailID;
        }

        public async Task UpdateAsync(SaleDetail saledetail)
        {
            _context.SaleDetails.Update(saledetail);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.SaleDetails.FindAsync(id);
            if (entity != null)
            {
                _context.SaleDetails.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}