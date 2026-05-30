using Inventory.Data;
using Inventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
            var prs = await _repo.GetAllAsync();
            return Ok(prs);
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
            var newId = await _repo.CreateAsync(pr);
            pr.PRID = newId;
            return CreatedAtAction(nameof(GetPR), new { id = newId }, pr);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePR(int id, PurchaseRequisition pr)
        {
            if (id != pr.PRID) return BadRequest();
            await _repo.UpdateAsync(pr);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePR(int id)
        {
            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}
