using Inventory.Data;
using Inventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
            var notes = await _repo.GetAllAsync();
            return Ok(notes);
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
            if (id != note.DeliveryID)
                return BadRequest();

            await _repo.UpdateAsync(note);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}
