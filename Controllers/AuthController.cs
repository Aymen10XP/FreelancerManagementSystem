using FreelancerManagementSystem.DTOs;
using FreelancerManagementSystem.Interfaces;
using FreelancerManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace FreelancerManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterDto request)
        {
            var user = await _authService.Register(request);
            if (user == null)
            {
                return BadRequest("User already exists.");
            }
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginDto request)
        {
            var token = await _authService.Login(request);
            if (token == null)
            {
                return BadRequest("Invalid email or password.");
            }
            return Ok(token);
        }
    }
}

