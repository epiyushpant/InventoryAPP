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
    [RequireCapability("form.salesInvoices")]
    public class SalesInvoicesController : ControllerBase
    {
        private readonly SalesInvoiceRepository _repo;

        public SalesInvoicesController(SalesInvoiceRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesInvoice>>> GetInvoices()
        {
            var invoices = await _repo.GetAllAsync();
            return Ok(invoices);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SalesInvoice>> GetInvoice(int id)
        {
            var invoice = await _repo.GetByIdAsync(id);
            if (invoice == null)
                return NotFound();
            return Ok(invoice);
        }

        [HttpPost]
        public async Task<ActionResult<SalesInvoice>> CreateInvoice(SalesInvoice invoice)
        {
            try
            {
                var newId = await _repo.CreateAsync(invoice);
                invoice.InvoiceID = newId;
                return CreatedAtAction(nameof(GetInvoice), new { id = newId }, invoice);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice(int id, SalesInvoice invoice)
        {
            if (id != invoice.InvoiceID)
                return BadRequest();

            try
            {
                await _repo.UpdateAsync(invoice);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}
