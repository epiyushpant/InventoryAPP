using Inventory.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Data
{
    public class PurchaseRequisitionRepository
    {
        private readonly ApplicationDbContext _context;

        public PurchaseRequisitionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PurchaseRequisition>> GetAllAsync()
        {
            return await _context.PurchaseRequisitions.AsNoTracking().ToListAsync();
        }

        public async Task<PurchaseRequisition?> GetByIdAsync(int id)
        {
            return await _context.PurchaseRequisitions.AsNoTracking().FirstOrDefaultAsync(e => e.PRID == id);
        }

        public async Task<int> CreateAsync(PurchaseRequisition pr)
        {
            pr.RequiredDate = pr.RequiredDate.ToUniversalTime();
            _context.PurchaseRequisitions.Add(pr);
            await _context.SaveChangesAsync();
            return pr.PRID;
        }

        public async Task UpdateAsync(PurchaseRequisition pr)
        {
            pr.RequiredDate = pr.RequiredDate.ToUniversalTime();
            _context.PurchaseRequisitions.Update(pr);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.PurchaseRequisitions.FindAsync(id);
            if (entity != null)
            {
                _context.PurchaseRequisitions.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
