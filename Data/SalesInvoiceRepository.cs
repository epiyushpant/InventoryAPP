using Inventory.Models;
using Inventory.Services;
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
            invoice.InvoiceDate = invoice.InvoiceDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(invoice.InvoiceDate, DateTimeKind.Utc)
                : invoice.InvoiceDate.ToUniversalTime();

            var sale = await _context.Sales
                .Include(s => s.SaleDetails)
                .FirstOrDefaultAsync(s => s.SaleID == invoice.SaleID)
                ?? throw new InvalidOperationException($"Sale #{invoice.SaleID} not found.");

            if (await _context.SalesInvoices.AnyAsync(i => i.SaleID == invoice.SaleID))
                throw new InvalidOperationException($"Sale #SO-{invoice.SaleID} already has an invoice.");

            var details = sale.SaleDetails?.ToList() ?? new List<SaleDetail>();
            if (details.Count == 0)
            {
                details = await _context.SaleDetails.Where(d => d.SaleID == invoice.SaleID).ToListAsync();
            }

            if (details.Count == 0)
                throw new InvalidOperationException("Cannot invoice a sale with no line items.");

            var productIds = details.Select(d => d.ProductID).Distinct().ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductID))
                .ToDictionaryAsync(p => p.ProductID);

            var lineInputs = details.Select(d =>
            {
                var lineTotal = d.LineTotal != 0
                    ? d.LineTotal
                    : (d.OrderedQuantity * d.UnitPrice) - d.Discount;
                var isTaxable = products.TryGetValue(d.ProductID, out var p) ? p.IsTaxable : true;
                return (lineTotal, isTaxable);
            });

            var (taxable, nonTaxable, tax, grand) = NepalVat.FromLines(lineInputs);
            invoice.TaxableAmount = taxable;
            invoice.NonTaxableAmount = nonTaxable;
            invoice.TaxAmount = tax;
            invoice.GrandTotal = grand;

            if (taxable > 0)
            {
                var customer = await _context.Customers.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CustomerID == sale.CustomerID);
                NepalPan.RequireForTaxableInvoice(customer?.PAN);
            }

            var (fyStart, fyEnd, fyLabel) = NepalFiscalYear.GetBounds(invoice.InvoiceDate);
            var seq = await _context.SalesInvoices.CountAsync(i =>
                i.InvoiceDate >= fyStart && i.InvoiceDate <= fyEnd) + 1;
            invoice.InvoiceNumber = NepalFiscalYear.FormatInvoiceNumber(fyLabel, seq);

            if (string.IsNullOrWhiteSpace(invoice.Status))
                invoice.Status = "Due";

            _context.SalesInvoices.Add(invoice);
            await _context.SaveChangesAsync();
            return invoice.InvoiceID;
        }

        public async Task UpdateAsync(SalesInvoice invoice)
        {
            // Structural fields locked — payment status only
            var existing = await _context.SalesInvoices.FindAsync(invoice.InvoiceID)
                ?? throw new InvalidOperationException("Invoice not found.");

            if (!DocumentLock.IsPaymentStatusEditable(invoice.Status))
                throw new InvalidOperationException($"Invalid payment status '{invoice.Status}'.");

            existing.Status = invoice.Status;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            throw new InvalidOperationException(
                "Sales invoices cannot be deleted. Adjust payment status or issue a credit note (future).");
        }
    }
}
