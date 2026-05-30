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
            var newId = await _repo.CreateAsync(grn);
            grn.GRNID = newId;
            return CreatedAtAction(nameof(GetGRN), new { id = newId }, grn);
        }
    }
}
