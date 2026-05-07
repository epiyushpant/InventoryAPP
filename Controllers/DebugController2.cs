using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        /// <summary>
        /// Test endpoint - No authentication required
        /// </summary>
        [HttpGet("test")]
        public IActionResult TestApi()
        {
            return Ok(new
            {
                message = "✅ API is working!",
                timestamp = DateTime.UtcNow,
                note = "This endpoint requires NO authentication"
            });
        }

        /// <summary>
        /// Protected endpoint - Requires valid JWT token
        /// </summary>
        [HttpGet("protected")]
        [Authorize]
        public IActionResult ProtectedEndpoint()
        {
            var userName = HttpContext.User?.Identity?.Name ?? "Unknown";
            return Ok(new
            {
                message = "✅ Token is valid!",
                user = userName,
                timestamp = DateTime.UtcNow,
                note = "This endpoint requires valid JWT token"
            });
        }

        /// <summary>
        /// Shows JWT configuration
        /// </summary>
        [HttpGet("jwt-config")]
        public IActionResult JwtConfig()
        {
            return Ok(new
            {
                issuer = "MyApi",
                audience = "MyApiUsers",
                instructions = new[]
                {
                    "1. POST /api/auth/login with username & password",
                    "2. Copy 'token' value from response",
                    "3. Click 'Authorize' button in Swagger",
                    "4. Paste token (WITHOUT 'Bearer' prefix)",
                    "5. Swagger will add 'Bearer' automatically",
                    "6. Try any protected endpoint"
                }
            });
        }

        /// <summary>
        /// Quick test sequence
        /// </summary>
        [HttpGet("help")]
        public IActionResult Help()
        {
            return Ok(new
            {
                step1 = "GET /api/debug/test (should work - no auth)",
                step2 = "POST /api/auth/login (get token)",
                step3 = "GET /api/debug/protected (test with token)",
                step4 = "GET /api/categories (protected endpoint)",
                note = "Check console output for [JWT] logs"
            });
        }
    }
}
