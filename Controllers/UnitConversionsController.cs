using Inventory.Data;
using Inventory.Filters;
using Inventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [RequireCapability("form.unitConversions")]
    public class UnitConversionsController : ControllerBase
    {
        private readonly UnitConversionRepository _repo;

        public UnitConversionsController(UnitConversionRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UnitConversion>>> GetAll()
            => Ok(await _repo.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<UnitConversion>> GetById(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<UnitConversion>> Create(UnitConversion conversion)
        {
            try
            {
                var id = await _repo.CreateAsync(conversion);
                conversion.ConversionID = id;
                return CreatedAtAction(nameof(GetById), new { id }, conversion);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UnitConversion conversion)
        {
            if (id != conversion.ConversionID) return BadRequest();
            try
            {
                await _repo.UpdateAsync(conversion);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}
