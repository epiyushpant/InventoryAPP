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
            var orders = await _context.PurchaseOrders.AsNoTracking()
                .Include(po => po.PurchaseOrderDetails)
                .ToListAsync();
            
            // Calculate total amount for each order based on details
            foreach (var order in orders)
            {
                order.TotalAmount = await CalculateTotalAmountAsync(order.PurchaseOrderID);
            }
            
            return orders;
        }

        public async Task<PurchaseOrder?> GetByIdAsync(int id)
        {
            var order = await _context.PurchaseOrders.AsNoTracking()
                .Include(po => po.PurchaseOrderDetails)
                .FirstOrDefaultAsync(e => e.PurchaseOrderID == id);
            
            if (order != null)
            {
                order.TotalAmount = await CalculateTotalAmountAsync(id);
            }
            return order;
        }

        private async Task<decimal> CalculateTotalAmountAsync(int purchaseOrderId)
        {
            var total = await _context.PurchaseOrderDetails
                .Where(d => d.PurchaseOrderID == purchaseOrderId)
                .SumAsync(d => d.OrderedQuantity * d.UnitPrice);
            
            return total;
        }

        public async Task<int> CreateAsync(PurchaseOrder purchaseorder)
        {
            purchaseorder.OrderDate = purchaseorder.OrderDate.ToUniversalTime();
            purchaseorder.ExpectedDeliveryDate = purchaseorder.ExpectedDeliveryDate?.ToUniversalTime();

            // Calculate total amount if details are provided
            if (purchaseorder.PurchaseOrderDetails != null && purchaseorder.PurchaseOrderDetails.Any())
            {
                decimal total = 0;
                foreach (var detail in purchaseorder.PurchaseOrderDetails)
                {
                    detail.LineTotal = detail.OrderedQuantity * detail.UnitPrice;
                    total += detail.LineTotal;
                }
                purchaseorder.TotalAmount = total;
            }

            _context.PurchaseOrders.Add(purchaseorder);
            await _context.SaveChangesAsync();
            return purchaseorder.PurchaseOrderID;
        }

        public async Task UpdateAsync(PurchaseOrder purchaseorder)
        {
            // Get the existing order to check for status change
            var existingOrder = await _context.PurchaseOrders.AsNoTracking().FirstOrDefaultAsync(po => po.PurchaseOrderID == purchaseorder.PurchaseOrderID);
            bool statusChangedToCompleted = existingOrder != null && 
                                           existingOrder.Status != "Completed" && 
                                           purchaseorder.Status == "Completed";

            purchaseorder.OrderDate = purchaseorder.OrderDate.ToUniversalTime();
            purchaseorder.ExpectedDeliveryDate = purchaseorder.ExpectedDeliveryDate?.ToUniversalTime();

            // Sync Details if provided
            if (purchaseorder.PurchaseOrderDetails != null)
            {
                var existingDetails = await _context.PurchaseOrderDetails.Where(d => d.PurchaseOrderID == purchaseorder.PurchaseOrderID).ToListAsync();
                _context.PurchaseOrderDetails.RemoveRange(existingDetails);
                
                decimal total = 0;
                foreach (var detail in purchaseorder.PurchaseOrderDetails)
                {
                    detail.PurchaseOrderID = purchaseorder.PurchaseOrderID;
                    detail.PODetailID = 0; // Ensure they are treated as new
                    detail.LineTotal = detail.OrderedQuantity * detail.UnitPrice;
                    total += detail.LineTotal;
                    _context.PurchaseOrderDetails.Add(detail);
                }
                purchaseorder.TotalAmount = total;
            }

            _context.PurchaseOrders.Update(purchaseorder);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.PurchaseOrders.FindAsync(id);
            if (entity != null)
            {
                if (entity.Status == "Completed")
                {
                    throw new System.InvalidOperationException("Cannot delete a completed purchase order. Please cancel it first or keep it for records.");
                }
                _context.PurchaseOrders.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}