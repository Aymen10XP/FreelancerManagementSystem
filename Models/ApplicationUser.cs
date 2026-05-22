using Microsoft.AspNetCore.Identity;

namespace FreelancerManagementSystem.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Project> ProjectsAsClient { get; set; } = new List<Project>();
        public ICollection<Project> ProjectsAsFreelancer { get; set; } = new List<Project>();
        public ICollection<Contract> ContractsAsFreelancer { get; set; } = new List<Contract>();
        public ICollection<Contract> ContractsAsClient { get; set; } = new List<Contract>();
        public ICollection<Invoice> InvoicesAsClient { get; set; } = new List<Invoice>();
        public ICollection<Invoice> InvoicesAsFreelancer { get; set; } = new List<Invoice>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}