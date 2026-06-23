using Inventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // Allow dropdown lists to load without auth blocks
    public class AddressesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AddressesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("countries")]
        public async Task<ActionResult<IEnumerable<Country>>> GetCountries()
        {
            var countries = await _context.Countries
                .OrderBy(c => c.CountryName == "Nepal" ? 0 : 1) // Prioritize Nepal at the top
                .ThenBy(c => c.CountryName)
                .ToListAsync();
            return Ok(countries);
        }

        [HttpGet("nepal/provinces")]
        public async Task<ActionResult<IEnumerable<Province>>> GetProvinces()
        {
            var provinces = await _context.Provinces
                .OrderBy(p => p.ProvinceID)
                .ToListAsync();
            return Ok(provinces);
        }

        [HttpGet("nepal/provinces/{provinceId}/districts")]
        public async Task<ActionResult<IEnumerable<District>>> GetDistricts(int provinceId)
        {
            var districts = await _context.Districts
                .Where(d => d.ProvinceID == provinceId)
                .OrderBy(d => d.DistrictName)
                .ToListAsync();
            return Ok(districts);
        }

        [HttpGet("nepal/districts/{districtId}/municipalities")]
        public async Task<ActionResult<IEnumerable<Municipality>>> GetMunicipalities(int districtId)
        {
            var municipalities = await _context.Municipalities
                .Where(m => m.DistrictID == districtId)
                .OrderBy(m => m.MunicipalityName)
                .ToListAsync();
            return Ok(municipalities);
        }
    }
}
