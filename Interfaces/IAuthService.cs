using FreelancerManagementSystem.Models;
using FreelancerManagementSystem.DTOs;

namespace FreelancerManagementSystem.Interfaces
{
    public class IAuthService
    {
        Task<User?> Register(RegisterDto request);
        Task<string?> Login(LoginDto request); // Returns jwt or null
    }
}
