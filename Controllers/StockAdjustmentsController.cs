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
    public class StockAdjustmentsController : ControllerBase
    {
        private readonly StockAdjustmentRepository _repo;

        public StockAdjustmentsController(StockAdjustmentRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockAdjustment>>> GetAdjustments()
        {
            var data = await _repo.GetAllAsync();
            return Ok(data);
        }

        [HttpPost]
        public async Task<ActionResult<StockAdjustment>> CreateAdjustment(StockAdjustment adj)
        {
            try
            {
                var newId = await _repo.CreateAsync(adj);
                adj.AdjustmentID = newId;
                return Ok(adj);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
