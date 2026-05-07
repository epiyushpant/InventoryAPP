using Inventory.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Data
{
    public class PurchaseOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PurchaseOrder>> GetAllAsync()
        {
            return await _context.PurchaseOrders.AsNoTracking().ToListAsync();
        }

        public async Task<PurchaseOrder?> GetByIdAsync(int id)
        {
            return await _context.PurchaseOrders.AsNoTracking().FirstOrDefaultAsync(e => e.PurchaseOrderID == id);
        }

        public async Task<int> CreateAsync(PurchaseOrder purchaseorder)
        {
            purchaseorder.OrderDate = purchaseorder.OrderDate.ToUniversalTime();
            purchaseorder.ExpectedDate = purchaseorder.ExpectedDate?.ToUniversalTime();
            _context.PurchaseOrders.Add(purchaseorder);
            await _context.SaveChangesAsync();
            return purchaseorder.PurchaseOrderID;
        }

        public async Task UpdateAsync(PurchaseOrder purchaseorder)
        {
            purchaseorder.OrderDate = purchaseorder.OrderDate.ToUniversalTime();
            purchaseorder.ExpectedDate = purchaseorder.ExpectedDate?.ToUniversalTime();
            _context.PurchaseOrders.Update(purchaseorder);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.PurchaseOrders.FindAsync(id);
            if (entity != null)
            {
                _context.PurchaseOrders.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}