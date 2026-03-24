namespace FreelancerManagementSystem.Models
{
    public class ProjectTask
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Todo, InProgress, Review, Done
        public int Order { get; set; }

        public int Priority { get; set; } // 1 = High, 2 = Medium, 3 = Low

        public Guid? AssignedToId { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

       // public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public User? AssignedTo { get; set; }
    }
}
