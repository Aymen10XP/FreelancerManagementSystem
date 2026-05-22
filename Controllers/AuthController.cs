using Microsoft.AspNetCore.Mvc;
using FreelancerManagementSystem.DTOs;
using FreelancerManagementSystem.Interfaces;
using FreelancerManagementSystem.Models;

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
        public async Task<ActionResult<object>> Register(RegisterDto request)
        {
            var result = await _authService.Register(request);
            if (result == null)
            {
                return BadRequest(new { message = "User already exists or registration failed." });
            }

            return Ok(new
            {
                message = "Registration successful",
                user = new
                {
                    result.Id,
                    result.Email,
                    result.FirstName,
                    result.LastName
                }
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<object>> Login(LoginDto request)
        {
            var token = await _authService.Login(request);
            if (token == null)
            {
                return BadRequest(new { message = "Invalid email or password." });
            }
            return Ok(new { token, message = "Login successful" });
        }
    }
}