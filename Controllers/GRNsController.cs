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
    [RequireCapability("form.grns")]
    public class GRNsController : ControllerBase
    {
        private readonly GRNRepository _repo;

        public GRNsController(GRNRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GRN>>> GetGRNs()
        {
            var grns = await _repo.GetAllAsync();
            return Ok(grns);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GRN>> GetGRN(int id)
        {
            var grn = await _repo.GetByIdAsync(id);
            if (grn == null) return NotFound();
            return Ok(grn);
        }

        [HttpPost]
        public async Task<ActionResult<GRN>> CreateGRN(GRN grn)
        {
            try
            {
                var newId = await _repo.CreateAsync(grn);
                grn.GRNID = newId;
                return CreatedAtAction(nameof(GetGRN), new { id = newId }, grn);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGRN(int id, GRN grn)
        {
            if (id != grn.GRNID) return BadRequest();
            try
            {
                await _repo.UpdateAsync(grn);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGRN(int id)
        {
            try
            {
                await _repo.DeleteAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
