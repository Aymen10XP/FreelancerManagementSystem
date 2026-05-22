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
            await SeedTeamDemoUsersAsync();
            await SeedProjectsAsync();
            await SeedProjectTasksAsync();
            await SeedContractsAsync();
            await SeedInvoicesAndPaymentsAsync();
            await RepairTeamDemoDataAsync();
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

        private async Task SeedTeamDemoUsersAsync()
        {
            await EnsureDemoUserAsync(
                "aziz.admin@example.com",
                "Mohamed Aziz",
                "Hafhouf",
                "Admin",
                "Admin@123");

            await EnsureDemoUserAsync(
                "aymen.client@example.com",
                "Aymen",
                "Ben Rjab",
                "Client",
                "Client@123");

            await EnsureDemoUserAsync(
                "fadi.freelancer@example.com",
                "Fadi",
                "Zammiti",
                "Freelancer",
                "Freelancer@123");

            await EnsureDemoUserAsync(
                "majdi.freelancer@example.com",
                "Majdi",
                "Zammiti",
                "Freelancer",
                "Freelancer@123");
        }

        private async Task<ApplicationUser> EnsureDemoUserAsync(
            string email,
            string firstName,
            string lastName,
            string role,
            string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    CreatedAt = DateTime.UtcNow
                };
                await _userManager.CreateAsync(user, password);
            }
            else
            {
                user.UserName = email;
                user.Email = email;
                user.FirstName = firstName;
                user.LastName = lastName;
                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            if (!await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            if (await _userManager.HasPasswordAsync(user))
            {
                await _userManager.RemovePasswordAsync(user);
            }
            await _userManager.AddPasswordAsync(user, password);

            return user;
        }

        private async Task SeedProjectsAsync()
        {
            if (await _context.Projects.AnyAsync())
            {
                return;
            }

            var clients = (await _userManager.GetUsersInRoleAsync("Client")).ToList();
            var freelancers = (await _userManager.GetUsersInRoleAsync("Freelancer")).ToList();
            if (!clients.Any() || !freelancers.Any())
            {
                return;
            }

            var now = DateTime.UtcNow;
            var projects = new[]
            {
                new Project
                {
                    Id = Guid.NewGuid(),
                    Name = "SaaS Landing Page Redesign",
                    Description = "Refresh a conversion-focused landing page with responsive sections and analytics-ready CTAs.",
                    Status = "Active",
                    StartDate = now.AddDays(-18),
                    Deadline = now.AddDays(18),
                    Budget = 4200,
                    ClientId = clients[0].Id,
                    FreelancerId = freelancers[4].Id,
                    CreatedAt = now.AddDays(-18)
                },
                new Project
                {
                    Id = Guid.NewGuid(),
                    Name = "Mobile Booking App",
                    Description = "Build the core screens and API integration for a service booking mobile experience.",
                    Status = "Pending",
                    StartDate = now.AddDays(5),
                    Deadline = now.AddDays(45),
                    Budget = 7800,
                    ClientId = clients[1].Id,
                    FreelancerId = freelancers[2].Id,
                    CreatedAt = now.AddDays(-6)
                },
                new Project
                {
                    Id = Guid.NewGuid(),
                    Name = "Backend Billing Automation",
                    Description = "Automate invoice generation, webhook processing, and monthly client reporting.",
                    Status = "Completed",
                    StartDate = now.AddDays(-70),
                    Deadline = now.AddDays(-10),
                    EndDate = now.AddDays(-8),
                    Budget = 9600,
                    ClientId = clients[2].Id,
                    FreelancerId = freelancers[3].Id,
                    CreatedAt = now.AddDays(-75),
                    UpdatedAt = now.AddDays(-8)
                }
            };

            _context.Projects.AddRange(projects);
            await _context.SaveChangesAsync();
        }

        private async Task SeedProjectTasksAsync()
        {
            if (await _context.ProjectTasks.AnyAsync())
            {
                return;
            }

            var projects = await _context.Projects.OrderBy(p => p.CreatedAt).ToListAsync();
            foreach (var project in projects)
            {
                var assignedTo = project.FreelancerId;
                var now = DateTime.UtcNow;
                var tasks = new[]
                {
                    new ProjectTask
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = project.Id,
                        Title = "Discovery and scope",
                        Description = "Confirm requirements, deliverables, constraints, and success metrics.",
                        Status = "Done",
                        Priority = 1,
                        Order = 0,
                        AssignedToId = assignedTo,
                        DueDate = project.StartDate.AddDays(3),
                        CreatedAt = project.CreatedAt
                    },
                    new ProjectTask
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = project.Id,
                        Title = "Design implementation",
                        Description = "Build the primary screens and reusable components.",
                        Status = project.Status == "Completed" ? "Done" : "In Progress",
                        Priority = 2,
                        Order = 0,
                        AssignedToId = assignedTo,
                        DueDate = project.StartDate.AddDays(14),
                        CreatedAt = project.CreatedAt.AddDays(1)
                    },
                    new ProjectTask
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = project.Id,
                        Title = "Client review",
                        Description = "Collect feedback, polish details, and prepare the final handoff.",
                        Status = project.Status == "Completed" ? "Done" : "Review",
                        Priority = 2,
                        Order = 0,
                        AssignedToId = assignedTo,
                        DueDate = project.Deadline.AddDays(-4),
                        CreatedAt = project.CreatedAt.AddDays(2)
                    }
                };

                _context.ProjectTasks.AddRange(tasks);
            }

            await _context.SaveChangesAsync();
        }

        private async Task SeedContractsAsync()
        {
            if (await _context.Contracts.AnyAsync())
            {
                return;
            }

            var projects = await _context.Projects.Where(p => p.FreelancerId.HasValue).ToListAsync();
            var contracts = projects.Select(project => new Contract
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                ClientId = project.ClientId,
                FreelancerId = project.FreelancerId!.Value,
                Title = $"{project.Name} Contract",
                Content = $"Agreement for {project.Name}, including delivery milestones, revision windows, and acceptance criteria.",
                Terms = "50% deposit, 50% on delivery. Scope changes require written approval.",
                Rate = project.Budget,
                RateType = "Fixed",
                TotalAmount = project.Budget,
                StartDate = project.StartDate,
                EndDate = project.Deadline,
                Status = project.Status == "Completed" ? "Completed" : "Active",
                CreatedAt = project.CreatedAt.AddDays(1)
            });

            _context.Contracts.AddRange(contracts);
            await _context.SaveChangesAsync();
        }

        private async Task SeedInvoicesAndPaymentsAsync()
        {
            if (await _context.Invoices.AnyAsync())
            {
                return;
            }

            var contracts = await _context.Contracts.ToListAsync();
            foreach (var contract in contracts)
            {
                var isCompleted = contract.Status == "Completed";
                var invoice = new Invoice
                {
                    Id = Guid.NewGuid(),
                    InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                    ContractId = contract.Id,
                    ClientId = contract.ClientId,
                    FreelancerId = contract.FreelancerId,
                    Amount = contract.TotalAmount,
                    Status = isCompleted ? "Paid" : "Sent",
                    IssueDate = contract.StartDate.AddDays(2),
                    DueDate = contract.EndDate,
                    PaidDate = isCompleted ? contract.EndDate.AddDays(-2) : null,
                    Description = $"Invoice for {contract.Title}",
                    CreatedAt = contract.StartDate.AddDays(2)
                };

                _context.Invoices.Add(invoice);

                if (isCompleted)
                {
                    _context.Payments.Add(new Payment
                    {
                        Id = Guid.NewGuid(),
                        InvoiceId = invoice.Id,
                        UserId = contract.ClientId,
                        Amount = invoice.Amount,
                        PaymentDate = invoice.PaidDate ?? DateTime.UtcNow,
                        PaymentMethod = "Bank transfer",
                        TransactionId = $"TX-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                        Status = "Completed",
                        Notes = "Seeded full payment for demo."
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task RepairTeamDemoDataAsync()
        {
            var aymen = await _userManager.FindByEmailAsync("aymen.client@example.com");
            var fadi = await _userManager.FindByEmailAsync("fadi.freelancer@example.com");
            var majdi = await _userManager.FindByEmailAsync("majdi.freelancer@example.com");

            if (aymen == null || fadi == null || majdi == null)
            {
                return;
            }

            var projects = await _context.Projects
                .OrderBy(p => p.CreatedAt)
                .ThenBy(p => p.Name)
                .ToListAsync();

            var teamProjects = new[]
            {
                new
                {
                    Name = "FreelanceOS Dashboard Polish",
                    Description = "Mohamed Aziz, Aymen, Fadi, and Majdi refined the dashboard, CRUD flows, and professor-ready demo data.",
                    Budget = 4200m,
                    Freelancer = fadi
                },
                new
                {
                    Name = "Client Workflow Automation",
                    Description = "Aymen validated the client journey while Majdi implemented task, invoice, and payment handoff scenarios.",
                    Budget = 7800m,
                    Freelancer = majdi
                },
                new
                {
                    Name = "Backend Integration Sprint",
                    Description = "Fadi and Majdi connected contracts, invoices, payments, and task boards into one functional platform.",
                    Budget = 9600m,
                    Freelancer = fadi
                }
            };

            for (var i = 0; i < projects.Count && i < teamProjects.Length; i++)
            {
                var demo = teamProjects[i];
                projects[i].Name = demo.Name;
                projects[i].Description = demo.Description;
                projects[i].Budget = demo.Budget;
                projects[i].ClientId = aymen.Id;
                projects[i].FreelancerId = demo.Freelancer.Id;
                projects[i].UpdatedAt = DateTime.UtcNow;
            }

            var contracts = await _context.Contracts.ToListAsync();
            foreach (var contract in contracts)
            {
                var project = projects.FirstOrDefault(p => p.Id == contract.ProjectId);
                if (project == null || !project.FreelancerId.HasValue)
                {
                    continue;
                }

                contract.ClientId = aymen.Id;
                contract.FreelancerId = project.FreelancerId.Value;
                contract.Title = $"{project.Name} Agreement";
                contract.Content = $"Team agreement for {project.Name}, prepared by Mohamed Aziz Hafhouf with Aymen Ben Rjab, Fadi Zammiti, and Majdi Zammiti.";
                contract.Terms = "Demo terms: scope approved by Aymen, delivery handled by the assigned freelancer, payment tracked in FreelanceOS.";
                contract.TotalAmount = project.Budget;
                contract.Rate = project.Budget;
                contract.UpdatedAt = DateTime.UtcNow;
            }

            var invoices = await _context.Invoices.ToListAsync();
            foreach (var invoice in invoices)
            {
                var contract = contracts.FirstOrDefault(c => c.Id == invoice.ContractId);
                if (contract == null)
                {
                    continue;
                }

                invoice.ClientId = aymen.Id;
                invoice.FreelancerId = contract.FreelancerId;
                invoice.Amount = contract.TotalAmount;
                invoice.Description = $"Team demo invoice for {contract.Title}";
                invoice.UpdatedAt = DateTime.UtcNow;
            }

            var tasks = await _context.ProjectTasks.ToListAsync();
            foreach (var task in tasks)
            {
                var project = projects.FirstOrDefault(p => p.Id == task.ProjectId);
                task.AssignedToId = project?.FreelancerId;

                if (task.Title == "Discovery and scope")
                {
                    task.Title = "Team scope review";
                    task.Description = "Mohamed Aziz and Aymen reviewed the platform scope and demo story.";
                }
                else if (task.Title == "Design implementation")
                {
                    task.Title = "Frontend and API integration";
                    task.Description = "Fadi and Majdi connected the premium UI to the real backend CRUD scenarios.";
                }
                else if (task.Title == "Client review")
                {
                    task.Title = "Professor demo rehearsal";
                    task.Description = "The team validated projects, contracts, invoices, payments, and task board flows.";
                }

                task.UpdatedAt = DateTime.UtcNow;
            }

            var payments = await _context.Payments.ToListAsync();
            foreach (var payment in payments)
            {
                payment.UserId = aymen.Id;
                payment.Notes = "Team demo payment recorded by Aymen Ben Rjab.";
            }

            await _context.SaveChangesAsync();
        }
    }
}
