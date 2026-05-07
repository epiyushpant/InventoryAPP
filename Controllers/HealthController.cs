using Microsoft.AspNetCore.Mvc;

namespace Inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new { status = "API is running", timestamp = DateTime.UtcNow });
        }

        [HttpGet("endpoints")]
        public IActionResult GetEndpoints()
        {
            return Ok(new
            {
                message = "Available Endpoints",
                endpoints = new[]
                {
                    "GET /api/categories",
                    "GET /api/suppliers",
                    "GET /api/products",
                    "GET /api/inventories",
                    "GET /api/purchaseorders",
                    "GET /api/purchaseorderdetails",
                    "GET /api/sales",
                    "GET /api/saledetails",
                    "GET /api/stockmovements",
                    "POST /api/auth/login",
                    "POST /api/auth/register"
                }
            });
        }
    }
}
