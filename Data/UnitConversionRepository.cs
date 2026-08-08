using Inventory.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Data
{
    public class UnitConversionRepository
    {
        private readonly ApplicationDbContext _context;

        public UnitConversionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UnitConversion>> GetAllAsync()
        {
            return await _context.UnitConversions.AsNoTracking().OrderBy(c => c.ProductID).ToListAsync();
        }

        public async Task<UnitConversion?> GetByIdAsync(int id)
        {
            return await _context.UnitConversions.AsNoTracking().FirstOrDefaultAsync(c => c.ConversionID == id);
        }

        public async Task<int> CreateAsync(UnitConversion conversion)
        {
            if (conversion.Factor <= 0)
                throw new InvalidOperationException("Conversion factor must be greater than zero.");

            _context.UnitConversions.Add(conversion);
            await _context.SaveChangesAsync();
            return conversion.ConversionID;
        }

        public async Task UpdateAsync(UnitConversion conversion)
        {
            if (conversion.Factor <= 0)
                throw new InvalidOperationException("Conversion factor must be greater than zero.");

            _context.UnitConversions.Update(conversion);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _context.UnitConversions.FindAsync(id);
            if (existing == null) return;
            _context.UnitConversions.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }
}
