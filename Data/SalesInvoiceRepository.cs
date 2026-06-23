using Inventory.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Data
{
    public class SalesInvoiceRepository
    {
        private readonly ApplicationDbContext _context;

        public SalesInvoiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SalesInvoice>> GetAllAsync()
        {
            return await _context.SalesInvoices.AsNoTracking().ToListAsync();
        }

        public async Task<SalesInvoice?> GetByIdAsync(int id)
        {
            return await _context.SalesInvoices.AsNoTracking().FirstOrDefaultAsync(e => e.InvoiceID == id);
        }

        public async Task<int> CreateAsync(SalesInvoice invoice)
        {
            invoice.InvoiceDate = invoice.InvoiceDate.ToUniversalTime();
            _context.SalesInvoices.Add(invoice);
            await _context.SaveChangesAsync();
            return invoice.InvoiceID;
        }

        public async Task UpdateAsync(SalesInvoice invoice)
        {
            invoice.InvoiceDate = invoice.InvoiceDate.ToUniversalTime();
            _context.SalesInvoices.Update(invoice);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.SalesInvoices.FindAsync(id);
            if (entity != null)
            {
                _context.SalesInvoices.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
