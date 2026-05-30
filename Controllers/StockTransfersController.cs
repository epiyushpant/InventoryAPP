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
    public class StockTransfersController : ControllerBase
    {
        private readonly StockTransferRepository _repo;

        public StockTransfersController(StockTransferRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockTransfer>>> GetTransfers()
        {
            var data = await _repo.GetAllAsync();
            return Ok(data);
        }

        [HttpPost]
        public async Task<ActionResult<StockTransfer>> CreateTransfer(StockTransfer transfer)
        {
            try
            {
                var newId = await _repo.CreateAsync(transfer);
                transfer.TransferID = newId;
                return Ok(transfer);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransfer(int id, StockTransfer transfer)
        {
            if (id != transfer.TransferID) return BadRequest();
            try
            {
                await _repo.UpdateAsync(transfer);
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
