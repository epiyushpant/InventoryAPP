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
    [RequireCapability("form.deliveryNotes")]
    public class DeliveryNotesController : ControllerBase
    {
        private readonly DeliveryNoteRepository _repo;

        public DeliveryNotesController(DeliveryNoteRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeliveryNote>>> GetNotes()
        {
            return Ok(await _repo.GetAllAsync());
        }

        [HttpPost]
        public async Task<ActionResult<DeliveryNote>> CreateNote(DeliveryNote note)
        {
            try
            {
                var newId = await _repo.CreateAsync(note);
                note.DeliveryID = newId;
                return Ok(note);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(int id, DeliveryNote note)
        {
            if (id != note.DeliveryID) return BadRequest();
            try
            {
                await _repo.UpdateAsync(note);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
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
