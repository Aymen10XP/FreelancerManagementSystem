namespace FreelancerManagementSystem.Models
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Budget { get; set; }
        public DateTime CreatedAt { get; set; }

        public Guid ClientId { get; set; }
        public Guid? FreelancerId { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public DateTime Deadline { get; set; }
        public string Status { get; set; } = "Pending"; // It could be: Active, Completed, Cancelled

        // Foreign Key to User (The Freelancer)
        public Guid Freelancer { get; set; }
        public User Client { get; set; } = null!;

        public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    }
}

