using System.Security.Claims;
using Inventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        private async Task<ApplicationUser?> CurrentUserAsync()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return id == null ? null : await _userManager.FindByIdAsync(id);
        }

        private static ProfileDto ToDto(ApplicationUser u) => new()
        {
            Id = u.Id,
            Username = u.UserName ?? "",
            Email = u.Email ?? "",
            FullName = u.FullName,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Phone = u.PhoneNumber,
            Bio = u.Bio,
            Role = u.Role,
            Country = u.Country,
            City = u.City,
            PostalCode = u.PostalCode,
            TaxId = u.TaxId,
            Facebook = u.Facebook,
            Twitter = u.Twitter,
            Linkedin = u.Linkedin,
            Instagram = u.Instagram,
        };

        [HttpGet("me")]
        public async Task<ActionResult<ProfileDto>> GetMe()
        {
            var user = await CurrentUserAsync();
            if (user == null) return Unauthorized();
            var roles = await _userManager.GetRolesAsync(user);
            var dto = ToDto(user);
            dto.Role = roles.FirstOrDefault() ?? user.Role;
            return Ok(dto);
        }

        [HttpPut("me")]
        public async Task<ActionResult<ProfileDto>> UpdateMe([FromBody] UpdateProfileDto body)
        {
            var user = await CurrentUserAsync();
            if (user == null) return Unauthorized();

            user.FirstName = body.FirstName?.Trim();
            user.LastName = body.LastName?.Trim();
            user.FullName = string.IsNullOrWhiteSpace(body.FullName)
                ? string.Join(" ", new[] { user.FirstName, user.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)))
                : body.FullName.Trim();
            user.PhoneNumber = body.Phone?.Trim();
            user.Bio = body.Bio?.Trim();
            user.Country = body.Country?.Trim();
            user.City = body.City?.Trim();
            user.PostalCode = body.PostalCode?.Trim();
            user.TaxId = body.TaxId?.Trim();
            user.Facebook = body.Facebook?.Trim();
            user.Twitter = body.Twitter?.Trim();
            user.Linkedin = body.Linkedin?.Trim();
            user.Instagram = body.Instagram?.Trim();

            if (!string.IsNullOrWhiteSpace(body.Email) && !string.Equals(body.Email.Trim(), user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var setEmail = await _userManager.SetEmailAsync(user, body.Email.Trim());
                if (!setEmail.Succeeded)
                    return BadRequest(new { message = "Could not update email.", errors = setEmail.Errors });
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { message = "Could not update profile.", errors = result.Errors });

            return Ok(ToDto(user));
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.CurrentPassword) || string.IsNullOrWhiteSpace(body.NewPassword))
                return BadRequest(new { message = "Current and new password are required." });

            var user = await CurrentUserAsync();
            if (user == null) return Unauthorized();

            var result = await _userManager.ChangePasswordAsync(user, body.CurrentPassword, body.NewPassword);
            if (!result.Succeeded)
                return BadRequest(new { message = "Could not change password.", errors = result.Errors });

            return Ok(new { message = "Password changed successfully." });
        }

        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
        {
            var user = await CurrentUserAsync();
            if (user == null) return Unauthorized();

            // Rotates the security stamp so any stamp-validated sessions are invalidated.
            await _userManager.UpdateSecurityStampAsync(user);
            return Ok(new { message = "Signed out of other sessions." });
        }

        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMe()
        {
            var user = await CurrentUserAsync();
            if (user == null) return Unauthorized();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { message = "Could not delete account.", errors = result.Errors });

            return NoContent();
        }
    }

    public class ProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Bio { get; set; }
        public string Role { get; set; } = "User";
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? TaxId { get; set; }
        public string? Facebook { get; set; }
        public string? Twitter { get; set; }
        public string? Linkedin { get; set; }
        public string? Instagram { get; set; }
    }

    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Bio { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? TaxId { get; set; }
        public string? Facebook { get; set; }
        public string? Twitter { get; set; }
        public string? Linkedin { get; set; }
        public string? Instagram { get; set; }
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
