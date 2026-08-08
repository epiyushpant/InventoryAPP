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
    [RequireCapability("form.purchaseRequisitions")]
    public class PurchaseRequisitionsController : ControllerBase
    {
        private readonly PurchaseRequisitionRepository _repo;

        public PurchaseRequisitionsController(PurchaseRequisitionRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PurchaseRequisition>>> GetPRs()
        {
            return Ok(await _repo.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PurchaseRequisition>> GetPR(int id)
        {
            var pr = await _repo.GetByIdAsync(id);
            if (pr == null) return NotFound();
            return Ok(pr);
        }

        [HttpPost]
        public async Task<ActionResult<PurchaseRequisition>> CreatePR(PurchaseRequisition pr)
        {
            try
            {
                var newId = await _repo.CreateAsync(pr);
                pr.PRID = newId;
                return CreatedAtAction(nameof(GetPR), new { id = newId }, pr);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePR(int id, PurchaseRequisition pr)
        {
            if (id != pr.PRID) return BadRequest();
            try
            {
                await _repo.UpdateAsync(pr);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePR(int id)
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
