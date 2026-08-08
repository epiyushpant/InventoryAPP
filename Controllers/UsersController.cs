using Inventory.Models;
using Inventory.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Controllers
{
    [Authorize(Roles = "Admin")] // Restrict to Admin
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITenantContext _tenantContext;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ITenantContext tenantContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tenantContext = tenantContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName ?? "",
                    Email = user.Email ?? "",
                    FullName = user.FullName,
                    Roles = roles.ToList()
                });
            }

            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new UserDto
            {
                Id = user.Id,
                Username = user.UserName ?? "",
                Email = user.Email ?? "",
                FullName = user.FullName,
                Roles = roles.ToList()
            });
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.Username) || string.IsNullOrWhiteSpace(body.Password))
                return BadRequest(new { message = "Username and password are required." });

            var user = new ApplicationUser
            {
                UserName = body.Username.Trim(),
                Email = string.IsNullOrWhiteSpace(body.Email) ? $"{body.Username.Trim()}@local" : body.Email.Trim(),
                FullName = body.FullName,
                Role = (body.Roles?.FirstOrDefault()) ?? "User",
                TenantId = _tenantContext.HasTenant ? _tenantContext.TenantId : 1
            };

            var result = await _userManager.CreateAsync(user, body.Password);
            if (!result.Succeeded)
                return BadRequest(new { message = "Could not create user.", errors = result.Errors });

            var roles = (body.Roles ?? new List<string> { "User" })
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (roles.Count == 0) roles.Add("User");

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));
                await _userManager.AddToRoleAsync(user, role);
            }

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new UserDto
            {
                Id = user.Id,
                Username = user.UserName ?? "",
                Email = user.Email ?? "",
                FullName = user.FullName,
                Roles = roles
            });
        }

        [HttpPut("{id}/roles")]
        [HttpPost("{id}/roles")] // alias for older clients
        public async Task<IActionResult> UpdateRoles(string id, [FromBody] List<string>? roles)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            roles ??= new List<string>();
            // Normalize / dedupe
            roles = roles
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var currentRoles = await _userManager.GetRolesAsync(user);

            var rolesToAdd = roles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToList();
            foreach (var role in rolesToAdd)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    var createRole = await _roleManager.CreateAsync(new IdentityRole(role));
                    if (!createRole.Succeeded)
                        return BadRequest(new { message = $"Could not create role '{role}'.", errors = createRole.Errors });
                }

                var addResult = await _userManager.AddToRoleAsync(user, role);
                if (!addResult.Succeeded)
                    return BadRequest(new { message = $"Could not add role '{role}'.", errors = addResult.Errors });
            }

            var rolesToRemove = currentRoles.Except(roles, StringComparer.OrdinalIgnoreCase).ToList();
            if (rolesToRemove.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                    return BadRequest(new { message = "Could not remove old roles.", errors = removeResult.Errors });
            }

            // Keep ApplicationUser.Role in sync for any legacy readers
            user.Role = roles.FirstOrDefault() ?? "User";
            await _userManager.UpdateAsync(user);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return NoContent();
        }
    }

    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public List<string>? Roles { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
