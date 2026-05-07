using Inventory.Data;
using Inventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StockMovementsController : ControllerBase
    {
        private readonly StockMovementRepository _repo;

        public StockMovementsController(StockMovementRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockMovement>>> GetStockMovements()
        {
            var movements = await _repo.GetAllAsync();
            return Ok(movements);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StockMovement>> GetStockMovement(int id)
        {
            var movement = await _repo.GetByIdAsync(id);
            if (movement == null)
                return NotFound();
            return Ok(movement);
        }

        [HttpPost]
        public async Task<ActionResult<StockMovement>> CreateStockMovement(StockMovement movement)
        {
            var newId = await _repo.CreateAsync(movement);
            movement.MovementID = newId;
            return CreatedAtAction(nameof(GetStockMovement), new { id = newId }, movement);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStockMovement(int id, StockMovement movement)
        {
            if (id != movement.MovementID)
                return BadRequest();

            await _repo.UpdateAsync(movement);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStockMovement(int id)
        {
            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}
