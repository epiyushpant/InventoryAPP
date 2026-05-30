using Inventory.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Data
{
    public class PurchaseOrderDetailRepository
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrderDetailRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PurchaseOrderDetail>> GetAllAsync()
        {
            return await _context.PurchaseOrderDetails.AsNoTracking().ToListAsync();
        }

        public async Task<PurchaseOrderDetail?> GetByIdAsync(int id)
        {
            return await _context.PurchaseOrderDetails.AsNoTracking().FirstOrDefaultAsync(e => e.PODetailID == id);
        }

        public async Task<int> CreateAsync(PurchaseOrderDetail purchaseorderdetail)
        {
            purchaseorderdetail.LineTotal = purchaseorderdetail.OrderedQuantity * purchaseorderdetail.UnitPrice;
            _context.PurchaseOrderDetails.Add(purchaseorderdetail);
            await _context.SaveChangesAsync();
            return purchaseorderdetail.PODetailID;
        }

        public async Task UpdateAsync(PurchaseOrderDetail purchaseorderdetail)
        {
            purchaseorderdetail.LineTotal = purchaseorderdetail.OrderedQuantity * purchaseorderdetail.UnitPrice;
            _context.PurchaseOrderDetails.Update(purchaseorderdetail);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.PurchaseOrderDetails.FindAsync(id);
            if (entity != null)
            {
                _context.PurchaseOrderDetails.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}