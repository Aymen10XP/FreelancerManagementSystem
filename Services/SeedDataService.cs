using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FreelancerManagementSystem.Data;
using FreelancerManagementSystem.Models;

namespace FreelancerManagementSystem.Services
{
    public class SeedDataService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public SeedDataService(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedDataAsync()
        {
            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedProjectsAsync();
            await SeedProjectTasksAsync();
            await SeedContractsAsync();
            await SeedInvoicesAndPaymentsAsync();
        }

        private async Task SeedRolesAsync()
        {
            var roles = new[] { "Admin", "Client", "Freelancer" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }
            }
        }

        private async Task SeedUsersAsync()
        {
            // Use _context.Users from Identity instead of checking directly
            if (!_context.Users.Any())
            {
                // Admin User
                var admin = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "admin@freelance.com",
                    Email = "admin@freelance.com",
                    FirstName = "System",
                    LastName = "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                await _userManager.CreateAsync(admin, "Admin@123");
                await _userManager.AddToRoleAsync(admin, "Admin");

                // Client Users
                var clients = new[]
                {
            new { Email = "john.client@example.com", FirstName = "John", LastName = "Smith" },
            new { Email = "sarah.client@example.com", FirstName = "Sarah", LastName = "Johnson" },
            new { Email = "michael.client@example.com", FirstName = "Michael", LastName = "Brown" }
        };

                foreach (var client in clients)
                {
                    var clientUser = new ApplicationUser
                    {
                        Id = Guid.NewGuid(),
                        UserName = client.Email,
                        Email = client.Email,
                        FirstName = client.FirstName,
                        LastName = client.LastName,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _userManager.CreateAsync(clientUser, "Client@123");
                    await _userManager.AddToRoleAsync(clientUser, "Client");
                }

                // Freelancer Users
                var freelancers = new[]
                {
            new { Email = "emma.dev@example.com", FirstName = "Emma", LastName = "Wilson" },
            new { Email = "james.design@example.com", FirstName = "James", LastName = "Taylor" },
            new { Email = "sophia.mobile@example.com", FirstName = "Sophia", LastName = "Martinez" },
            new { Email = "oliver.backend@example.com", FirstName = "Oliver", LastName = "Anderson" },
            new { Email = "mia.frontend@example.com", FirstName = "Mia", LastName = "Thomas" }
        };

                foreach (var freelancer in freelancers)
                {
                    var freelancerUser = new ApplicationUser
                    {
                        Id = Guid.NewGuid(),
                        UserName = freelancer.Email,
                        Email = freelancer.Email,
                        FirstName = freelancer.FirstName,
                        LastName = freelancer.LastName,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _userManager.CreateAsync(freelancerUser, "Freelancer@123");
                    await _userManager.AddToRoleAsync(freelancerUser, "Freelancer");
                }
            }
        }

        // Rest of the seed methods remain the same...
        private async Task SeedProjectsAsync() { /* ... */ }
        private async Task SeedProjectTasksAsync() { /* ... */ }
        private async Task SeedContractsAsync() { /* ... */ }
        private async Task SeedInvoicesAndPaymentsAsync() { /* ... */ }

        // Helper methods...
    }
}