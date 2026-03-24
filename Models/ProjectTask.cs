namespace FreelancerManagementSystem.Models
{
    public class ProjectTask
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public int Priority { get; set; } // 1 = High, 2 = Medium, 3 = Low

        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
    }
}
