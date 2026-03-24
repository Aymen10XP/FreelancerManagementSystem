namespace FreelancerManagementSystem.Models
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public DateTime Deadline { get; set; }
        public string Status { get; set; } = "Pending"; // It could be: Active, Completed, Cancelled

        // Foreign Key to User (The Freelancer)
        public Guid Freelancer { get; set; }
        public User User { get; set; } = null!;

        public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    }
}

