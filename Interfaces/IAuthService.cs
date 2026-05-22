using FreelancerManagementSystem.Models;
using FreelancerManagementSystem.DTOs;

namespace FreelancerManagementSystem.Interfaces
{
    public interface IAuthService
    {
        Task<ApplicationUser?> Register(RegisterDto request);
        Task<string?> Login(LoginDto request); // Returns JWT token or null
    }
}