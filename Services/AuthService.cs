using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FreelancerManagementSystem.Data;
using FreelancerManagementSystem.Models;
using FreelancerManagementSystem.DTOs;
using FreelancerManagementSystem.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FreelancerManagementSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;

        public AuthService(AppDbContext context, IConfiguration configuration, UserManager<User> userManager)
        {
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
        }

        /// <summary>
        /// Register method now delegated to AccountController using UserManager.
        /// This method is kept for backward compatibility but returns null.
        /// Use AccountController.Register() instead.
        /// </summary>
        public async Task<User?> Register(RegisterDto request)
        {
            // Registration is now handled by AccountController with Identity
            // This method is deprecated
            return null;
        }

        /// <summary>
        /// Login method now delegated to AccountController using SignInManager.
        /// This method is kept for backward compatibility but returns null.
        /// Use AccountController.Login() instead.
        /// </summary>
        public async Task<string?> Login(LoginDto request)
        {
            // Login is now handled by AccountController with Identity
            // This method is deprecated
            return null;
        }

        /// <summary>
        /// Generate JWT token for API access using Identity Claims.
        /// </summary>
        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim("Role", user.Role ?? "")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["AppSettings:Token"] ?? "default-secret-key-change-in-production"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
