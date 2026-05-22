using System.Security.Claims;
using FreelancerManagementSystem.Data;
using FreelancerManagementSystem.DTOs;
using FreelancerManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreelancerManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers([FromQuery] string? role = null)
        {
            var users = await _context.Users.OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ToListAsync();
            var result = new List<UserResponseDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var primaryRole = roles.FirstOrDefault() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(role) && !string.Equals(primaryRole, role, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result.Add(MapUser(user, primaryRole));
            }

            return Ok(result);
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserResponseDto>> GetMe()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(id, out var userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(MapUser(user, roles.FirstOrDefault() ?? string.Empty));
        }

        private static UserResponseDto MapUser(ApplicationUser user, string role)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = role,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
