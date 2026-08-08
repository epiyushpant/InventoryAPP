using Inventory.Models;
using Inventory.Services;
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
            if (string.IsNullOrWhiteSpace(pr.Status))
                pr.Status = "Pending";
            _context.PurchaseRequisitions.Add(pr);
            await _context.SaveChangesAsync();
            return pr.PRID;
        }

        public async Task UpdateAsync(PurchaseRequisition pr)
        {
            var existing = await _context.PurchaseRequisitions.AsNoTracking()
                .FirstOrDefaultAsync(e => e.PRID == pr.PRID)
                ?? throw new InvalidOperationException("Purchase requisition not found.");

            DocumentLock.EnsureEditable("PurchaseRequisition", existing.Status);

            pr.RequiredDate = pr.RequiredDate.ToUniversalTime();
            _context.PurchaseRequisitions.Update(pr);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.PurchaseRequisitions.FindAsync(id);
            if (entity != null)
            {
                DocumentLock.EnsureDeletable("PurchaseRequisition", entity.Status);
                _context.PurchaseRequisitions.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
